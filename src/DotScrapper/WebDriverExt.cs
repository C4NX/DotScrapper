﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Serilog;

namespace DotScrapper
{
    public static class WebDriverExt
    {
        public static readonly By ChildBy = By.XPath(".//*");

        public static object? ExecuteScriptAsyncWithoutError(this WebDriver driver, string jsScript, params object[] args)
        {
            try
            {
                return driver.ExecuteScript(jsScript, args);
            }
            catch (Exception ex)
            {
                Log.Verbose(ex, $"JS Script Error: {ex.Message}");
                return null;
            }
        }

        public static IWebElement? FindElementOrNull(this WebDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NotFoundException e)
            {
                Log.Verbose($"Element not found: {by.Criteria}");
                return null;
            }
        }

        public static void DeleteElementById(this WebDriver driver, string id)
            => ExecuteScriptAsyncWithoutError(driver, $"document.getElementById(\"{id}\")?.remove()");
    }
}
