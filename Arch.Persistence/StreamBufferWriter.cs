using System.Buffers;

namespace Arch.Persistence;

public sealed class StreamBufferWriter : IBufferWriter<byte>
{
    private byte[] _buffer;
    private readonly Stream _destination;
    private readonly bool _ownsStream;
    private int _position, _leased;
    
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

    void IBufferWriter<byte>.Advance(int count)
    {
        if (count > _leased || count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        _position += count;
        _leased = 0;
    }
    
    Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
    {
        var actual = Lease(sizeHint);
        return new Memory<byte>(_buffer, _position, actual);
    }

    Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
    {
        var actual = Lease(sizeHint);
        return new Span<byte>(_buffer, _position, actual);
    }
    
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
