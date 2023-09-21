using System.Buffers;

namespace Arch.Persistence;

/// <summary>
///     The <see cref="StreamBufferWriter"/> class
///     is a small wrapper around a <see cref="Stream"/> implementing a <see cref="IBufferWriter{T}"/>.
///     It buffers incoming bytes in an internally stored array and flushes it regulary into the <see cref="_destination"/>-<see cref="Stream"/>.
/// </summary>
public sealed class StreamBufferWriter : IBufferWriter<byte>, IDisposable
{
    /// <summary>
    ///     The buffer.
    /// </summary>
    private byte[] _buffer;
    
    /// <summary>
    ///     The <see cref="Stream"/>.
    /// </summary>
    private readonly Stream _destination;
    
    /// <summary>
    ///     If this instance owns the <see cref="_destination"/> stream.
    /// </summary>
    private readonly bool _ownsStream;
    
    /// <summary>
    ///     The current position and the amount of total leased bytes. 
    /// </summary>
    private int _position, _leased;
    
    /// <summary>
    ///     Creates a new <see cref="StreamBufferWriter"/> instance.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/>.</param>
    /// <param name="bufferSize">The buffer-size of the <see cref="_buffer"/>.</param>
    /// <param name="ownsStream">If it owns the stream.</param>
    public StreamBufferWriter(Stream destination, int bufferSize = 1024, bool ownsStream = true)
    {
        const int minBufferSize = 128;
        if (bufferSize < minBufferSize)
        {
            bufferSize = minBufferSize;
        }
        
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        _ownsStream = ownsStream;
        _destination = destination;
    }
    
    /// <summary>
    ///     Leases an amount of bytes from the <see cref="_buffer"/>.
    /// </summary>
    /// <param name="sizeHint">The total amount.</param>
    /// <returns>The leased amount.</returns>
    private int Lease(int sizeHint)
    {
        var available = _buffer.Length - _position;
        if (available < sizeHint && _position != 0)
        {   // try to get more
            Flush();
            available = _buffer.Length - _position;
        }

        _leased = available;
        return available;
    }
    
    /// <summary>
    ///     Flushes the buffered bytes to the <see cref="_destination"/>.
    /// </summary>
    /// <param name="flushUnderlyingStream">If it also should flush the <see cref="Stream"/>.</param>
    public void Flush(bool flushUnderlyingStream = false)
    {
        if (_position != 0)
        {
            _destination.Write(_buffer, 0, _position);
            _position = 0;
        }
        if (flushUnderlyingStream)
        {
            _destination.Flush();
        }
    }

    /// <summary>
    ///     Advances the buffer, notifies this instance that there was something new written into the <see cref="_buffer"/> memory. 
    /// </summary>
    /// <param name="count">The amount of bytes written.</param>
    /// <exception cref="ArgumentOutOfRangeException">Throws if we are out of memory.</exception>
    void IBufferWriter<byte>.Advance(int count)
    {
        if (count > _leased || count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        _position += count;
        _leased = 0;
    }
    
    /// <summary>
    ///     Returns a partion of the <see cref="_buffer"/> as a <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="sizeHint">The total amount.</param>
    /// <returns>The new <see cref="Memory{T}"/> instance.</returns>
    Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
    {
        var actual = Lease(sizeHint);
        return new Memory<byte>(_buffer, _position, actual);
    }

    /// <summary>
    ///     Returns a partion of the <see cref="_buffer"/> as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="sizeHint">The total amount.</param>
    /// <returns>The new <see cref="Span{T}"/> instance.</returns>
    Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
    {
        var actual = Lease(sizeHint);
        return new Span<byte>(_buffer, _position, actual);
    }
    
    /// <summary>
    ///     Disposes this instance, flushes and releases all memory. 
    /// </summary>
    public void Dispose()
    {
        Flush(true);
        
        var tmp = _buffer;
        _buffer = null;
        ArrayPool<byte>.Shared.Return(tmp);

        if (_ownsStream)
        {
            _destination.Dispose();
        }
    }
}
