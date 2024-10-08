﻿using Newtonsoft.Json;
using XVNML.Core.Tags;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("tag", typeof(Tags), TagOccurance.Multiple)]
    public sealed class Tag : TagBase
    {
        [JsonProperty] public string? name;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            name = TagName!;
        }
    }
}
