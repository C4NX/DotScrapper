using DotScrapper;
using DotScrapper.Scrappers;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using DotScrapper.IO;
using OpenQA.Selenium.Chrome;

// ReSharper disable InconsistentNaming

namespace DotScrapper
{
    internal class Program
    {
        const string NullStr = "<null>";
        
        #region Arguments

        private static ArgumentDefinition? ArgumentHelp;
        private static ArgumentDefinition? ArgumentUse;
        private static ArgumentDefinition? ArgumentCompress;
        private static ArgumentDefinition? ArgumentOutput;
        private static ArgumentDefinition? ArgumentQuery;
        private static ArgumentDefinition? ArgumentMax;
        private static ArgumentDefinition? ArgumentPostAction;
        private static ArgumentDefinition? ArgumentShow;
        private static ArgumentDefinition? ArgumentVerbose;
        private static ArgumentDefinition? ArgumentList;
        private static ArgumentDefinition? ArgumentProxy;
        private static ArgumentDefinition? ArgumentAutoClean;
        
        private static ArgumentDefinition? ArgumentForceEdge;
        private static ArgumentDefinition? ArgumentForceChrome;
        
        #endregion

        private static bool hasVerboseLog;
        
        static ScrapperManager? ScrapperRegister;

        static ILogger Logger
            => Log.Logger;
        private static bool isLoggerEnabled = true;
        private static readonly string VersionString
            = GetVersionString();

        static void CreateArguments()
        {
            ArgumentHelp 
                = Arguments.Add(new ("help", "h", "You are here."));
            ArgumentUse
                = Arguments.Add(new("use", "u", "Set the scraper to use."));
            ArgumentCompress 
                = Arguments.Add(new("compress", "x", "Compress the output folder to a zip."));
            ArgumentOutput 
                = Arguments.Add(new("out", "o", "Set an output path for it."));
            ArgumentQuery
                = Arguments.Add(new("query", "q", "Set the query to use."));
            ArgumentMax
                = Arguments.Add(new("max", "m", "Set the maximum number of images to get."));
            ArgumentPostAction
                = Arguments.Add(new("post", "p", "Set the post-scrapping action."));
            ArgumentShow
                = Arguments.Add(new("show", "s", "Shows the browser window in use."));
            ArgumentVerbose 
                = Arguments.Add(new("verbose", "v", "logs. but deeper..."));
            ArgumentList 
                = Arguments.Add(new("list", null, "Full list all scrappers/post-actions."));
            ArgumentProxy 
                = Arguments.Add(new("proxy", null, "Tell all requests to pass on a proxy."));
            ArgumentAutoClean 
                = Arguments.Add(new("autoclean", "a", "Auto kill all still-running web driver (to remove)"));

            ArgumentForceEdge
                = Arguments.Add(new("edge", null, "Force use of edge WebDriver"));
            ArgumentForceChrome
                = Arguments.Add(new("chrome", null, "Force use of chrome WebDriver"));
        }
        
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            
            CreateArguments();
            Arguments.Default = Arguments.LoadArguments(args);
            hasVerboseLog = ArgumentVerbose?.IsPresent() 
                            ?? false;
            
            ScrapperRegister = ScrapperManager.FromAssembly(typeof(Program).Assembly);

            if (CheckForHelpArgument()
                || CheckForListArgument())
                return;
            
            ConfigureLogger();
            
            string? useParam = ArgumentUse?.GetActualData(nameof(Bing));

            string? outParam = ArgumentOutput?.GetActualData("Out\\");

            string? queryParam = ArgumentQuery?.GetActualData("Cat");

            string? postParam = ArgumentPostAction?.GetActualData();
                
            Logger.Information(VersionString);

            ConfigureProxyWithArguments();
            CheckAutoClean();
            
            ScrapperContext scrapperContext 
                = new(null, new HttpClient(new SocketsHttpHandler()));

            var scrappers = GetScrappersArgument();
            if (scrappers.Count == 0)
            {
                Logger.Error("No scrapper was provided, please use -u(se) <scrapper>");
                return;
            }
            
            // use edge chromium only if RequireChromium in that scrapper.
            if (scrappers.Any(x => x.Definition.RequireChromium))
                ConfigureWebDriver(scrapperContext);
            
            // handle ctrl exit.
            Console.CancelKeyPress += (sender, e) =>
            {
                Logger.Information("Exiting...");

                // using that because Selenium may try to continue the connection to that driver, and create errors, like WebDriverException.
                isLoggerEnabled = false;
                scrapperContext.Dispose();
                Environment.Exit(0);
            };
            
            // try create output
            if (!Directory.Exists(outParam))
                Directory.CreateDirectory(outParam!);

            LogPostInformation();
            
            // start scrapping !
            try
            {
                var downloader = new ScrapperDownloader(scrapperContext)
                    .Using(scrappers);

                // add post param post actions.
                if (postParam != null)
                    downloader.UsingPost(ScrapperRegister.GetPostActionByName(postParam));


                // show a concat of scrappers and post-scraps
                Logger.Information("Using: {Usings}",
                    string.Join(", ",
                        downloader.Scrappers.Select(x => x.GetType().Name)
                            .Concat(downloader.PostScrapsActions.Select(x => x?.GetType().Name))));

                Logger.Information("Query: {Query}, To: {Dir}", queryParam ?? NullStr, outParam ?? NullStr);

                await downloader.DownloadAsync(new ScrapperQuery(queryParam ?? NullStr
                        , ArgumentMax != null && ArgumentMax.IsPresent() 
                            ? int.Parse(ArgumentMax.GetActualData(int.MaxValue.ToString())!) 
                            : null)
                    , outParam ?? "Out\\", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error was catch while scrapping");
            }
            finally
            {
                if (outParam != null)
                {
                    try
                    {
                        var zipFilename = Path.GetDirectoryName(outParam) + ".zip";
                        if (File.Exists(zipFilename))
                            File.Delete(zipFilename);

                        ZipFile.CreateFromDirectory(outParam, zipFilename);
                        Logger.Information("Created ZIP: {Fn}", zipFilename);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Zip error");
                    }

                }

                scrapperContext.Dispose();
            }
        }

        static bool CheckForHelpArgument()
        {
            if (ArgumentHelp != null && ArgumentHelp.IsPresent())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(VersionString);
                Console.ResetColor();
                Console.WriteLine("Basic Usage:");
                Console.WriteLine("\t./DotScrapper -q <query> -u <service> -o <out>");
                Console.WriteLine("\tTo learn more about it, please watch the documentation on github.");
                Console.WriteLine("Arguments: ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\t"+string.Join($"{Environment.NewLine}\t", Arguments.GetEnumerable().Select(x=>$"{x.Name}{(x.SmallName != null ? "|"+x.SmallName : string.Empty)}{(x.Description != null ? $" - {x.Description}" : string.Empty)}")));
                Console.ResetColor();
                if (ScrapperRegister != null)
                {
                    Console.WriteLine($"Scrappers:");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\t{string.Join(", ", ScrapperRegister.AllScrappers().Select(x => x.Definition.Name))}");
                    Console.ResetColor();
                    Console.WriteLine($"Post-Actions:");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\t{string.Join(", ", ScrapperRegister.AllPostActions().Select(x => x.GetType().Name))}");
                }
                Console.ResetColor();
                Console.WriteLine("Contribute:");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\thttps://github.com/C4NX/DotScrapper");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Made with ❤ by NX.");
                return true;
            }

            return false;
        }

        static bool CheckForListArgument()
        {
            if (ArgumentList != null && ArgumentList.IsPresent())
            {
                var ANSI_RESET = "\u001B[0m";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(VersionString);
                Console.ResetColor();
                if (ScrapperRegister != null)
                {
                    Console.WriteLine($"Scrappers:");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    foreach (var x in ScrapperRegister.AllScrappers())
                        Console.WriteLine($"\t{(x.Definition.RequireChromium ? "[WD 🔎]" : "       ")} {ANSI_RESET}{x.Definition.Name} - {x.Definition.Description ?? "<no-description>"}");
                    Console.ResetColor();
                    Console.WriteLine($"Post-Actions:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\t{string.Join(", ", ScrapperRegister.AllPostActions().Select(x=>x.GetType().Name))}");
                    Console.ResetColor();
                }
                Console.WriteLine("Assemblies:");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                foreach (var x in AppDomain.CurrentDomain.GetAssemblies())
                    Console.WriteLine($"\t{x.FullName}");
                Console.ResetColor();
                
                return true;
            }

            return false;
        }

        static void ConfigureLogger()
        {
            LoggingLevelSwitch? loggingLevel = new LoggingLevelSwitch(hasVerboseLog
                ? LogEventLevel.Verbose
                : LogEventLevel.Information);
            
            Log.Logger = new LoggerConfiguration()
                // when DotScrapper exit, some logs still happen so we have that var to handle it.
                .Filter.ByExcluding(x=>!isLoggerEnabled)
                .MinimumLevel.ControlledBy(loggingLevel)
                .WriteTo.Console()
                .WriteTo.Debug()
                .CreateLogger();
        }

        static void ConfigureProxyWithArguments()
        {
            if (ArgumentProxy != null && ArgumentProxy.IsPresent())
            {
                var proxyString = ArgumentProxy.GetActualData();
                HttpClient.DefaultProxy =
                    new WebProxy(ArgumentProxy.GetActualData() ?? throw new ArgumentException("No 'proxy' data argument"));

                Logger.Information("Proxy set to: {Url}", proxyString);
            }
        }

        static void CheckAutoClean()
        {
            if (ArgumentAutoClean != null && ArgumentAutoClean.IsPresent())
            {
                foreach (var item in Process.GetProcessesByName("msedgedriver"))
                {
                    try
                    {
                        if (Path.GetDirectoryName(item.MainModule?.FileName) == Directory.GetCurrentDirectory())
                        {
                            Logger.Information("Killing running msedgedriver... (PID: {pid})", item.Id);
                            item.Kill(true);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.Warning("msedgedriver (PID: {Pid}) is still running...", item.Id);
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        /// Configure the webdriver to use, webdriver can be forced to be use but in auto, windows chose edge and other system use chrome.
        /// </summary>
        /// <param name="ctx"></param>
        static void ConfigureWebDriver(ScrapperContext ctx)
        {
            if (ArgumentForceEdge != null && ArgumentForceEdge.IsPresent())
                ConfigureEdge(ctx);
            else if (ArgumentForceChrome != null && ArgumentForceChrome.IsPresent())
                ConfigureChrome(ctx);
            else
            {
                if (OperatingSystem.IsWindows())
                    ConfigureEdge(ctx);
                else
                    ConfigureChrome(ctx);
            }
        }

        static bool ConfigureChrome(ScrapperContext ctx)
        {
            Logger.Debug("Starting ChromeDriver...");

            try
            {
                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;

                var chromeOptions = new ChromeOptions();
                if (ArgumentProxy != null && ArgumentProxy.IsPresent())
                {
                    var proxyString = ArgumentProxy.GetActualData();
                    chromeOptions.Proxy = new Proxy
                    {
                        IsAutoDetect = false,
                        Kind = ProxyKind.Manual,
                        HttpProxy = proxyString
                    };
                }
                
                if (ArgumentShow != null && !ArgumentShow.IsPresent())
                    chromeOptions.AddArgument("--headless");
                
                var driver = new ChromeDriver(chromeDriverService, chromeOptions);
                Logger.Information("Using Chrome Driver: {Version}", driver.Capabilities.GetCapability("browserVersion"));

                ctx.UseChromium(driver);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Chrome driver failed");
                return false;
            }
        }
        
        static bool ConfigureEdge(ScrapperContext ctx)
        {
            try
            {
                Logger.Debug("Starting EdgeDriver...");

                var edgeDriverService = EdgeDriverService.CreateDefaultService();

                edgeDriverService.HideCommandPromptWindow = true;
                var edgeOptions = new EdgeOptions();

                if (ArgumentProxy != null && ArgumentProxy.IsPresent())
                {
                    var proxyString = ArgumentProxy.GetActualData();
                    edgeOptions.Proxy = new Proxy
                    {
                        IsAutoDetect = false,
                        Kind = ProxyKind.Manual,
                        HttpProxy = proxyString
                    };
                }

                // -[-]hide option
                if (ArgumentShow != null && !ArgumentShow.IsPresent())
                    edgeOptions.AddArgument("--headless");

        
        
                var driver = new EdgeDriver(edgeDriverService, edgeOptions);
                Logger.Information("Using EdgeDriver: {Version}", driver.Capabilities.GetCapability("browserVersion"));

                // use that driver !
                ctx.UseChromium(driver);
                return true;
            }
            catch (DriverServiceNotFoundException ex)
            {
                Logger.Fatal(ex, "WebDriver not found");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Unknown Error: {Message}", ex.Message);
            }
            
            return false;
        }
        
        static void LogPostInformation()
        {
            if (ScrapperRegister == null) 
                return;
            
            Logger.Information("Available scrappers: {Scrappers}"
                ,
                string.Join(", ",
                    ScrapperRegister.AllScrappers()
                        .Select(x => x.Definition.Name)));

            Logger.Information("Available post-actions: {Actions}"
                , string.Join(", ", ScrapperRegister.AllPostActions().Select(x => x.GetType().Name)));
        }
        
        static string GetVersionString()
            => $"DotScrapper ✂ - {typeof(Program).Assembly.GetName().Version}, {BuildInGit.GetBuildInGitVersion() ?? NullStr}";

        static IList<IScrapper> GetScrappersArgument()
            => ScrapperRegister == null 
                ? new List<IScrapper>()
                : new List<IScrapper>(
                ArgumentUse?.GetActualData(String.Empty)!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        var scrapperName = x.Trim();
                        var r = ScrapperRegister.GetByName(scrapperName);
                        if(r == null)
                            Logger.Warning("Scrapper '{ScrapperName}' does not exist", scrapperName);
                        return r;
                    })
                    .Where(x => x != null)!);
    }
}