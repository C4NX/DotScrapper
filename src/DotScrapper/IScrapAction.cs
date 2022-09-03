using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace DotScrapper
{
    /// <summary>
    /// An interface for applying action to a <see cref="ActionContext"/>.
    /// </summary>
    public interface IScrapAction
    {
        Task Apply(ActionContext ctx);
    }
}
