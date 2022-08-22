using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DotScrapper.Scrappers;
using NUnit.Framework;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;

namespace DotScrapper.Test
{
    public class Tests
    {
        public static ChromiumDriver driver;

        [SetUp]
        public void Setup()
        {
            var edgeDriverService = EdgeDriverService.CreateDefaultService();
            edgeDriverService.HideCommandPromptWindow = true;
            var edgeOptions = new EdgeOptions();
            edgeOptions.AddArgument("--headless");
            driver = new EdgeDriver(edgeDriverService, edgeOptions);
        }

        [Test]
        public void TestArguments()
        {
            var abc = new ArgumentDefinition("abc", "a");
            var bca = new ArgumentDefinition("bca", "b");
            var cba = new ArgumentDefinition("cba", "c");

            Arguments.Add(abc);
            Arguments.Add(bca);
            Arguments.Add(cba);

            // test for normal name.
            var arguments = Arguments.LoadArguments(new[] { "--abc", "value" });
            CollectionAssert.IsNotEmpty(arguments);
            Assert.AreEqual(1, arguments.Count);
            CollectionAssert.Contains(arguments.Select(x=>x.Ref), abc);
            CollectionAssert.Contains(arguments.Select(x=>x.Value), "value");

            // test for small key.
            arguments = Arguments.LoadArguments(new[] { "--ab -c", "value" });
            CollectionAssert.IsNotEmpty(arguments);
            Assert.AreEqual(3, arguments.Count);
            CollectionAssert.Contains(arguments.Select(x => x.Value), "value");

            // test for duplicate.
            arguments = Arguments.LoadArguments(new[] { "-abc", "-abc", "value", "-ab" });
            CollectionAssert.IsNotEmpty(arguments);
            Assert.AreEqual(3, arguments.Count);
            CollectionAssert.Contains(arguments.Select(x => x.Value), "value");
        }
    }
}