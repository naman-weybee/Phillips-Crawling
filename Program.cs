using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Net;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml;
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

            string PageSource = GetFullyLoadedWebPageContent(driver);
            var Details = new HtmlDocument();
            Details.LoadHtml(PageSource);

            var AllWatchAuctions = Details.DocumentNode.SelectNodes("//li[@class='has-image auction col-sm-2']");
            var Titles = Details.DocumentNode.SelectNodes("//div[@class='content-body col-sm-2 col-md-5']//h2/a");
            var ImageURL = Details.DocumentNode.SelectNodes("//img[@class='phillips-image__image']");
            var Links = Details.DocumentNode.SelectNodes("//div[@class='content-body col-sm-2 col-md-5']/h2/a");
            var TimeDuration = Details.DocumentNode.SelectNodes("//div[@class='content-body col-sm-2 col-md-5']/p");
            var SingleTitle = @"./div[@class='content-body col-sm-2 col-md-5']//h2/a";
            var SingleImageURL = @".//a//img";
            var SingleLink = @".//a";
            var SingleTimeDuration = @".//p";

            var timeDurationRegex = new Regex(@"(\s?\,?\-?)(\d+)?(\s?\-?\,?)(\d+)?(\s?\-?\,?)(\d+)?(\s?\,?\-?)(\w+)(\s?\,?\-?)(\d{4})");

            try
            {
                foreach (var watchAuction in AllWatchAuctions)
                {
                    var Title = "";
                    var imageURL = "";
                    var link = "";
                    var timeDuration = "";
                    var StartDate = "";
                    var StartMonth = "";
                    var StartYear = "";
                    var EndDate = "";
                    var EndMonth = "";
                    var EndYear = "";

                    int index = AllWatchAuctions.IndexOf(watchAuction);

                    Title = watchAuction.SelectSingleNode(SingleTitle).InnerHtml.Trim();
                    Console.WriteLine("Title: " + Title);

                    imageURL = watchAuction.SelectSingleNode(SingleImageURL).GetAttributes("src").First().Value;
                    Console.WriteLine("ImageURL: " + imageURL);

                    link = "https://www.phillips.com/" + watchAuction.SelectSingleNode(SingleLink).GetAttributes("href").First().Value;
                    Console.WriteLine("Link: " + link);

                    timeDuration = watchAuction.SelectSingleNode(SingleTimeDuration).InnerHtml.Trim();
                    Console.WriteLine("TimeDuration: " + timeDuration);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}