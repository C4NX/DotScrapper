﻿using DotScrapper;
using DotScrapper.Scrappers;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
// ReSharper disable InconsistentNaming


var ARG_HELP = Arguments.Add(new ("help", "h"));
var ARG_USE = Arguments.Add(new("use", "u"));
var ARG_OUTPUT = Arguments.Add(new("out", "o"));
var ARG_QUERY = Arguments.Add(new("query", "q"));
var ARG_POST_ACTION = Arguments.Add(new("post", "p"));
var ARG_HEADLESS = Arguments.Add(new("show", "s"));
var ARG_VERBOSE = Arguments.Add(new("verbose", "v"));
var ARG_AUTOCLEAN = Arguments.Add(new("autoclean", "a"));
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

logger.Information("DotScrapper: {version}, {gitVersion}",
    typeof(Program).Assembly.GetName()
        .Version,
    gitVersion ?? "<no-git-version>");

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

var scrappers
    = ScrapperManager.FromAssembly(typeof(Program).Assembly);

ScrapperContext ctx 
    = new(null, new HttpClient(new SocketsHttpHandler{ AllowAutoRedirect = true }));

IScrapper scrapper = scrappers.GetByName(useParam) ?? throw new ArgumentException($"{useParam} not found.");

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
        scrappers.AllScrappers()
            .Select(x => x.Definition.Description != null
                ? $"{x.Definition.Name} ({x.Definition.Description})"
                : x.Definition.Name)));

logger.Information("Available post-actions: {actions}"
    , string.Join(", ", scrappers.AllPostActions().Select(x => x.GetType().Name)));

try
{
    var downloader = new ScrapperDownloader(ctx, scrapper);

    // add post param post actions.
    if (postParam != null)
        downloader.UsingPost(scrappers.GetPostActionByName(postParam));


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