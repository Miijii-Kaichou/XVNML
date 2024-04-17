using Newtonsoft.Json;
using XVNML.Core.Dialogue.Enums;

namespace XVNML.Core.Dialogue.Structs
{
    internal class CastMemberSignature
    {
        [JsonProperty] internal bool IsNarrative;
        [JsonProperty] internal bool IsFull;
        [JsonProperty] internal bool IsPartial;
        [JsonProperty] internal bool IsAnonymous;
        [JsonProperty] internal bool IsPersistent;
        [JsonProperty] internal bool IsPrimitive;
        [JsonProperty] internal bool IsInversed;
        [JsonProperty] internal Role CurrentRole;

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
}