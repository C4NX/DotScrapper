namespace DotScrapper.IO;

public static class BuildInGit
{
    public static string? GetBuildInGitVersion()
    {
        using Stream? stream = typeof(Program).Assembly
            .GetManifestResourceStream($"{typeof(IScrapper).Namespace}.version.txt");
        
        if (stream != null)
        {
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd()
                .ReplaceLineEndings(string.Empty);
        }

        return null;
    }
}