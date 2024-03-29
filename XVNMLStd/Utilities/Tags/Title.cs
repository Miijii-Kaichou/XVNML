﻿using XVNML.Core.Tags;
namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("title", typeof(Metadata), TagOccurance.PragmaLocalOnce)]
    public sealed class Title : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            value ??= TagName;
        }
    }
}
