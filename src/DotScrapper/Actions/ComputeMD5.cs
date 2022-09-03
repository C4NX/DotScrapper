using Serilog;
using System.Security.Cryptography;

namespace DotScrapper.Actions
{
    public class ComputeMD5 : IScrapAction
    {
        public Task Apply(ActionContext ctx)
        {
            var md5 = CalculateMd5(ctx.Path);
            var name = Path.GetFileNameWithoutExtension(ctx.Path);

            if (name != md5)
            {
                Log.Verbose("Moving {filename} to his MD5:{Md5}", ctx.Path, md5);
                ctx.Move(Path.Combine(Path.GetDirectoryName(ctx.Path) 
                                      ?? string.Empty
                    ,$"{md5}{Path.GetExtension(ctx.Path)}"));
            }

            return Task.CompletedTask;
        }

        private static string CalculateMd5(string filePathName)
        {
            using var stream = File.OpenRead(filePathName);
            using var md5 = MD5.Create();

            return Convert.ToHexString(md5.ComputeHash(stream));
        }
    }
}
