﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace DotScrapper.Actions
{
    public class ClearExif : IPostScrapAction
    {
        public void Apply(Image image)
        {
            if (image.Metadata.ExifProfile == null) return;

            foreach (var exifProfileValue in image.Metadata.ExifProfile.Values
                         .ToArray())
                image.Metadata.ExifProfile.RemoveValue(exifProfileValue.Tag);
            foreach (var exifProfileInvalidTag in image.Metadata.ExifProfile.InvalidTags
                         .ToArray())
                image.Metadata.ExifProfile.RemoveValue(exifProfileInvalidTag);
        }
    }
}
