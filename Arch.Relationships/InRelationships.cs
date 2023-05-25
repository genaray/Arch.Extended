namespace Arch.Relationships;

/// <summary>
///     Component holding a reference to the buffer that its owning entity is being
///     targeted in.
/// </summary>
internal readonly struct InRelationships
{
    /// <summary>
    ///     The buffer holding a relationship with the owning entity of this component.
    /// </summary>
    internal readonly IBuffer Relationships;

    internal InRelationships(IBuffer relationships)
    {
        Relationships = relationships;
    }
}
