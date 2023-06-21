﻿using System;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("portrait", typeof(PortraitDefinitions), TagOccurance.Multiple)]
    public sealed class Portrait : TagBase
    {
        public Image? imageTarget;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ImageParameterString
            };

            base.OnResolve(fileOrigin);

            var imgRef = GetParameter(ImageParameterString);
            if (imgRef != null && imgRef.isReferencing)
            {
                
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                ParserRef!.QueueForReferenceSolve(OnImageReferenceSolve);
                return;
            }

            throw new Exception("\"img\" parameter must pass a reference");
        }

        void OnImageReferenceSolve()
        {
            TagBase? imageDefinitions = null;
            TagBase? target = null;
            var img = GetParameterValue<string>(ImageParameterString);
            try
            {
                if (img?.ToLower() == NullParameterString)
                {
                    XVNMLLogger.LogWarning($"Image Source was set to null for: {TagName}", this);
                    return;
                }
                //Iterate through until you find the right source target;
                imageDefinitions = ParserRef!.root?.GetElement<ImageDefinitions>();
               
                target = imageDefinitions?.GetElement<Image>(img?.ToString()!);
                imageTarget = (Image)Convert.ChangeType(target, typeof(Image))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {img?.ToString()!}" +
                    $": img {imageDefinitions!.tagTypeName}");
            }
        }
    }
}