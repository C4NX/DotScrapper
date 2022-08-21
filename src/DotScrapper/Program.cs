﻿using System.ComponentModel;
using System.Diagnostics;
using DotScrapper;
using DotScrapper.Actions;
using DotScrapper.Scrappers;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Serilog;

bool IsLoggerEnable = true;

string? useParam = Arguments.GetArgumentData(args, "use", "u");

string? outParam = Arguments.GetArgumentData(args, "out", "o")
                   ?? "Out\\";

string? queryParam = Arguments.GetArgumentData(args, "query", "q")
                     ?? "Cat";

Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding(x=>!IsLoggerEnable)
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

var logger = Log.Logger;

logger.Information("DotScrapper: {version}", typeof(Program).Assembly.GetName().Version);

// -[-]autoclean, msedgedriver cleaner.
if (Arguments.HasArguments(args, "autoclean", "a"))
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

EdgeDriver? driver;

try
{
    var chromeDriverService = EdgeDriverService.CreateDefaultService();
    chromeDriverService.HideCommandPromptWindow = true;
    var edgeOptions = new EdgeOptions();

    // -[-]hide option
    if (!Arguments.HasArguments(args, "show", "s"))
    {
        edgeOptions.AddArgument("--headless");
    }

    driver = new EdgeDriver(chromeDriverService, edgeOptions);

    logger.Information("Using EdgeDriver: {version}", driver.Capabilities.GetCapability("browserVersion"));
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

Console.CancelKeyPress += (sender, e) =>
{
    logger.Information("Exiting...");

    // using that because Selenium may try to continue the connection to that driver, and create errors, like WebDriverException.
    IsLoggerEnable = false;
    driver.Quit();
    Environment.Exit(0);
};


if (!Directory.Exists(outParam))
    Directory.CreateDirectory(outParam);

var scrappers = new ScrapperManager();
scrappers.ScanAssembly(typeof(Program).Assembly);

logger.Information("Available scrappers: {scrappers}"
    , string.Join(", ", scrappers.All().Select(x=>x.GetType().FullName)));

try
{
    var downloader = new ScrapperDownloader(driver)
                                        .Using(scrappers.GetByName(useParam) ?? throw new NotFoundException($"{useParam} not found."))
                                        .UsingPost(new ToThumbnail())
                                        /*.UsingPost(new ClearExif())*/;


    logger.Information("Using: {usings}", string.Join(", ", downloader.Scrappers.Select(x=>x.GetType().Name)));
    logger.Information("To: {dir}", Path.GetFullPath(outParam));

    await downloader.DownloadAsync(new ScrapperQuery(queryParam), outParam, true);
}
catch (Exception ex)
{
    logger.Error(ex, "An error was catch while scrapping.");
}
finally
{
    driver.Quit();
}