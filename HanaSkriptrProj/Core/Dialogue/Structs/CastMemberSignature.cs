internal struct CastMemberSignature
{
    internal bool? IsNarrative;
    internal bool? IsFull;
    internal bool? IsPartial;
    internal bool? IsAnonymous;
    internal bool? IsPersistent;
    internal bool? IsPrimitive;
    internal bool? IsInversed;
    internal Role? CurrentRole;
    public CastMemberSignature()
    {
        IsNarrative = false;
        IsFull = true;
        IsPartial = !IsFull;
        IsAnonymous = false;
        IsPersistent = false;
        IsPrimitive = true;
        IsInversed = false;
        CurrentRole = Role.Undefined;
    }
}
