using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace DotScrapper
{
    public interface IPostScrapAction
    {
        void Apply(Image image);
    }
}
