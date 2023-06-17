﻿using System;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

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
                "img"
            };

            base.OnResolve(fileOrigin);

            var imgRef = GetParameter("img");
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
            var img = GetParameterValue("img");
            try
            {
                if (img.ToString().ToLower() == "nil")
                {
                    XVNMLLogger.LogWarning($"Image Source was set to null for: {TagName}", this);
                    return;
                }
                //Iterate through until you find the right source target;
                imageDefinitions = ParserRef!._rootTag?.elements?
                    .Where(tag => tag.GetType() == typeof(ImageDefinitions))
                    .First();
               
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