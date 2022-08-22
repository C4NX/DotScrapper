﻿using DotScrapper;
using DotScrapper.Scrappers;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Text;

// ReSharper disable InconsistentNaming

var ARG_HELP = Arguments.Add(new ("help", "h", "You are here."));
var ARG_USE = Arguments.Add(new("use", "u", "Set the scraper to use."));
var ARG_OUTPUT = Arguments.Add(new("out", "o", "Set an output path for it."));
var ARG_QUERY = Arguments.Add(new("query", "q", "Set the query to use."));
var ARG_POST_ACTION = Arguments.Add(new("post", "p", "Set the post-scrapping action."));
var ARG_HEADLESS = Arguments.Add(new("show", "s", "Shows the browser window in use."));
var ARG_VERBOSE = Arguments.Add(new("verbose", "v", "logs. but deeper..."));
var ARG_HELPLIST = Arguments.Add(new("list", null, "Full list all scrappers/post-actions."));
var ARG_AUTOCLEAN = Arguments.Add(new("autoclean", "a", "Auto kill all still-running web driver (to remove)"));
Arguments.Default = Arguments.LoadArguments(args);

// get version.txt created in pre-build.
string? gitVersion = null;
await using (Stream? stream = typeof(Program).Assembly
                 .GetManifestResourceStream($"{typeof(IScrapper).Namespace}.version.txt"))
{
    if (stream != null)
    {
        using var reader = new StreamReader(stream);

        gitVersion = reader.ReadToEnd()
            .ReplaceLineEndings(string.Empty);
    }
}

Console.OutputEncoding = Encoding.Unicode;
string dotScrapperVersionString = $"DotScrapper ✂ - {typeof(Program).Assembly.GetName().Version}, {gitVersion}";

ScrapperManager scrapperManager = ScrapperManager.FromAssembly(typeof(Program).Assembly);

if (ARG_HELP.IsPresent())
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(dotScrapperVersionString);
    Console.ResetColor();
    Console.WriteLine("Basic Usage:");
    Console.WriteLine("\t./DotScrapper -q <query> -u <service> -o <out>");
    Console.WriteLine("\tTo learn more about it, please watch the documentation on github.");
    Console.WriteLine("Arguments: ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\t"+string.Join($"{Environment.NewLine}\t", Arguments.GetEnumerable().Select(x=>$"{x.Name}{(x.SmallName != null ? "|"+x.SmallName : string.Empty)}{(x.Description != null ? $" - {x.Description}" : string.Empty)}")));
    Console.ResetColor();
    Console.WriteLine($"Scrappers:");
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine($"\t{string.Join(", ", scrapperManager.AllScrappers().Select(x => x.Definition.Name))}");
    Console.ResetColor();
    Console.WriteLine($"Post-Actions:");
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine($"\t{string.Join(", ", scrapperManager.AllPostActions().Select(x => x.GetType().Name))}");
    Console.ResetColor();
    Console.WriteLine("Contribute:");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("\thttps://github.com/C4NX/DotScrapper");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Made with ❤ by NX.");
    return;
}

if (ARG_HELPLIST.IsPresent())
{
    var ANSI_RESET = "\u001B[0m";

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(dotScrapperVersionString);
    Console.ResetColor();
    Console.WriteLine($"Scrappers:");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    foreach (var x in scrapperManager.AllScrappers())
        Console.WriteLine($"\t{(x.Definition.RequireChromium ? "[WD 🔎]" : "       ")} {ANSI_RESET}{x.Definition.Name} - {x.Definition.Description ?? "<no-description>"}");
    Console.ResetColor();
    Console.WriteLine($"Post-Actions:");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\t{string.Join(", ", scrapperManager.AllPostActions().Select(x=>x.GetType().Name))}");
    Console.ResetColor();
    Console.WriteLine("Assemblies:");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    foreach (var x in AppDomain.CurrentDomain.GetAssemblies())
        Console.WriteLine($"\t{x.FullName}");
    Console.ResetColor();
    return;
}


// when DotScrapper exit, some logs still happen so we have that var to handle it.
bool isLoggerEnable = true;
LoggingLevelSwitch? loggingLevel = new LoggingLevelSwitch(ARG_VERBOSE.IsPresent()
    ? LogEventLevel.Verbose
    : LogEventLevel.Information);

string? useParam = ARG_USE.GetActualData(nameof(Bing))
                   ?? nameof(Bing);

string? outParam = ARG_OUTPUT.GetActualData("Out\\");

string? queryParam = ARG_QUERY.GetActualData("Cat");

string? postParam = ARG_POST_ACTION.GetActualData();

Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding(x=>!isLoggerEnable)
    .MinimumLevel.ControlledBy(loggingLevel)
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

var logger = Log.Logger;

logger.Information(dotScrapperVersionString);

// -[-]autoclean, msedgedriver cleaner.
if (ARG_AUTOCLEAN.IsPresent())
{
    foreach (var item in Process.GetProcessesByName("msedgedriver"))
    {
        try
        {
            if (item.MainModule?.FileName == Path.GetFullPath("msedgedriver.exe"))
            {
                logger.Information("Killing running msedgedriver... (PID: {pid})", item.Id);
                item.Kill(true);
            }
        }
        catch (Exception)
        {
            logger.Warning("msedgedriver (PID: {pid}) is still running...", item.Id);
            // ignored
        }
    }
}

ScrapperContext ctx 
    = new(null, new HttpClient(new SocketsHttpHandler{ AllowAutoRedirect = true }));

IScrapper scrapper = scrapperManager.GetByName(useParam) ?? throw new ArgumentException($"{useParam} not found.");

// use edge chromium only if RequireChromium in that scrapper.
if (scrapper.Definition.RequireChromium)
{
    try
    {
        logger.Debug("Creating EdgeDriver...");

        var edgeDriverService = EdgeDriverService.CreateDefaultService();
        edgeDriverService.HideCommandPromptWindow = true;
        var edgeOptions = new EdgeOptions();

        // -[-]hide option
        if (!ARG_HEADLESS.IsPresent())
            edgeOptions.AddArgument("--headless");

        var driver = new EdgeDriver(edgeDriverService, edgeOptions);
        logger.Information("Using EdgeDriver: {version}", driver.Capabilities.GetCapability("browserVersion"));

        // use that driver !
        ctx.UseChromium(driver);
    }
    catch (DriverServiceNotFoundException ex)
    {
        logger.Fatal(ex, "WebDriver not found.");
        return;
    }
    catch (Exception ex)
    {
        logger.Fatal(ex, "Unknown Error: {message}", ex.Message);
        return;
    }
}

Console.CancelKeyPress += (sender, e) =>
{
    logger.Information("Exiting...");

    // using that because Selenium may try to continue the connection to that driver, and create errors, like WebDriverException.
    isLoggerEnable = false;
    ctx.Dispose();
    Environment.Exit(0);
};


if (!Directory.Exists(outParam))
    Directory.CreateDirectory(outParam);

logger.Information("Available scrappers: {scrappers}"
    ,
    string.Join(", ",
        scrapperManager.AllScrappers()
            .Select(x => x.Definition.Description != null
                ? $"{x.Definition.Name} ({x.Definition.Description})"
                : x.Definition.Name)));

logger.Information("Available post-actions: {actions}"
    , string.Join(", ", scrapperManager.AllPostActions().Select(x => x.GetType().Name)));

try
{
    var downloader = new ScrapperDownloader(ctx, scrapper);

    // add post param post actions.
    if (postParam != null)
        downloader.UsingPost(scrapperManager.GetPostActionByName(postParam));


    // show a concat of scrappers and post-scraps
    logger.Information("Using: {usings}",
        string.Join(", ",
            downloader.Scrappers.Select(x => x.GetType().Name)
                .Concat(downloader.PostScrapsActions.Select(x => x?.GetType().Name))));

    logger.Information("Query: {query}", queryParam);

    logger.Information("To: {dir}", Path.GetFullPath(outParam));

    await downloader.DownloadAsync(new ScrapperQuery(queryParam), outParam, CancellationToken.None);
}
catch (Exception ex)
{
    logger.Error(ex, "An error was catch while scrapping.");
}
finally
{
    ctx.Dispose();
}