﻿using System;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("portrait", typeof(PortraitDefinitions), TagOccurance.Multiple)]
    public sealed class Portrait : TagBase
    {
        public Image? image;
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
                parserRef!.QueueForReferenceSolve(OnImgReferenceSolve);
                return;
            }

            throw new Exception("\"img\" parameter must pass a reference");
        }

        void OnImgReferenceSolve()
        {
            TagBase? imageDefinitions = null;
            TagBase? target = null;
            object? value = null;
            try
            {
                //Iterate through until you find the right source target;
                imageDefinitions = parserRef!._rootTag?.elements?.Where(tag => tag.GetType() == typeof(ImageDefinitions)).First();
                value = GetParameterValue("img");
                target = imageDefinitions?.GetElement<Image>(value?.ToString()!);
                image = (Image)Convert.ChangeType(target, typeof(Image))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {value?.ToString()!}" +
                    $": img {imageDefinitions!.tagTypeName}");
            }
        }
    }
}