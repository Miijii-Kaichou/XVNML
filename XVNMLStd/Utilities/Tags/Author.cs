﻿using XVNML.Core.Tags;

using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("author", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Author : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            value ??= TagName;
        }
    }
}
