using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using System;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Phillips_Crawling
{
    class Program
    {
        private const string Url = "https://www.phillips.com/auctions/past/filter/Departments%3DWatches/sort/newest";

        static void Main(string[] args)
        {
            GetWatchAuctionDetails();
            Console.ReadLine();
        }

        private static string GetFullyLoadedWebPageContent(WebDriver driver)
        {
            long scrollHeight = 0;
            IJavaScriptExecutor js = driver;
            do
            {
                var newScrollHeight = (long)js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight); return document.body.scrollHeight;");
                if (newScrollHeight != scrollHeight)
                {
                    scrollHeight = newScrollHeight;
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            } while (true);
            return driver.PageSource;
        }

        private static void GetWatchAuctionDetails()
        {
            WebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(Url);

            var PageSource = GetFullyLoadedWebPageContent(driver);
        }
    }
}