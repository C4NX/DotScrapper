using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace DotScrapper
{
    /// <summary>
    /// An interface to apply post actions to an sixlabors image.
    /// </summary>
    public interface IPostScrapAction
    {
        void Apply(Image image);
    }
}
