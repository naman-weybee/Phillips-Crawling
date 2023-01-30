using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using Phillips_Crawling_Task.Data;
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Net;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Phillips_Crawling
{
    class Program
    {
        private const string Url = "https://www.phillips.com/auctions/past/filter/Departments%3DWatches/sort/newest";
        private static readonly Phillips_DBContext _context = new();

        public static TimeSpan MyDefaultTimeout { get; private set; }

        static void Main(string[] args)
        {
            GetWatchAuctionDetails();
            Console.ReadLine();
        }

        public static string GetFullyLoadedWebPageContent(WebDriver driver)
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
                    Thread.Sleep(10000);
                    break;
                }
            } while (true);
            return driver.PageSource;
        }

        public static void GetWatchAuctionDetails()
        {
            WebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(Url);

            string PageSource = GetFullyLoadedWebPageContent(driver);
            driver.Close();
            var Details = new HtmlDocument();
            Details.LoadHtml(PageSource);

            var AllWatchAuctions = Details.DocumentNode.SelectNodes("//li[@class='has-image auction col-sm-2']");
            var Titles = Details.DocumentNode.SelectNodes("//div[@class='content-body col-sm-2 col-md-5']//h2/a");
            var ImageURL = Details.DocumentNode.SelectNodes("//img[@class='phillips-image__image']");
            var Links = Details.DocumentNode.SelectNodes("//div[@class='content-body col-sm-2 col-md-5']/h2/a");
            var TimeDuration = Details.DocumentNode.SelectNodes("//div[@class='content-body col-sm-2 col-md-5']");
            var SingleTitle = @"./div[@class='content-body col-sm-2 col-md-5']//h2/a";
            var SingleImageURL = @".//a//img";
            var SingleLink = @".//a";
            var SingleTimeDuration = @".//p";

            Regex timeDurationRegex = new(@"(\d+)(\s?\,?\-?\&?\s?)((\d+)?(\s?\,?\-?\s?)(\w+)?(\s?\,?\-?\s?)(\d{4})?(((\s?\,?\-?\s?))(\d+)?(\s?\,?\-?\s?)(\d+)?(\s?\,?\-?\s?)(\w+)?(\s?\,?\-?\s?)(\d{4}))?)?", RegexOptions.IgnoreCase);

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

                    Title = watchAuction.SelectSingleNode(SingleTitle).InnerHtml.Replace("&amp;#8211 ", "&").Replace("&amp;", "&").Trim();
                    imageURL = watchAuction.SelectSingleNode(SingleImageURL).GetAttributes("src").First().Value;
                    link = "https://www.phillips.com/" + watchAuction.SelectSingleNode(SingleLink).GetAttributes("href").First().Value;
                    timeDuration = watchAuction.SelectSingleNode(SingleTimeDuration).InnerHtml.Trim();

                    Regex uniquId = new(@"(^/)?(\w+)$", RegexOptions.IgnoreCase);
                    var id = uniquId.Match(link).Value;

                    var timeMatchRegex = timeDurationRegex.Match(timeDuration.Replace("&amp;#8211 ", "&").Replace("&amp;", "&").Replace("\n", ""));

                    if (timeMatchRegex.Success)
                    {
                        StartDate = timeMatchRegex.Groups[1].Value;
                        StartMonth = timeMatchRegex.Groups[6].Value == "" ? timeMatchRegex.Groups[16].Value : timeMatchRegex.Groups[6].Value;
                        StartYear = timeMatchRegex.Groups[8].Value == "" ? timeMatchRegex.Groups[18].Value : timeMatchRegex.Groups[8].Value;
                        EndDate = timeMatchRegex.Groups[12].Value == "" ? timeMatchRegex.Groups[4].Value == "" ? timeMatchRegex.Groups[1].Value : timeMatchRegex.Groups[4].Value : timeMatchRegex.Groups[12].Value;
                        EndMonth = timeMatchRegex.Groups[16].Value == "" ? timeMatchRegex.Groups[6].Value : timeMatchRegex.Groups[16].Value;
                        EndYear = timeMatchRegex.Groups[18].Value == "" ? timeMatchRegex.Groups[8].Value : timeMatchRegex.Groups[18].Value;
                    }

                    Auctions auctions = new()
                    {
                        Id = id,
                        Title = Title,
                        ImageURL = imageURL,
                        Link = link,
                        StartDate = StartDate,
                        StartMonth = StartMonth,
                        StartYear = StartYear,
                        EndDate = EndDate,
                        EndMonth = EndMonth,
                        EndYear = EndYear
                    };
                    _context.tbl_Auctions.Add(auctions);

                    Console.WriteLine("----------Auction----------");
                    Console.WriteLine("Id: " + id);
                    Console.WriteLine("Title: " + Title);
                    Console.WriteLine("ImageURL: " + imageURL);
                    Console.WriteLine("Link: " + link);
                    Console.WriteLine("TimeDuration: " + timeDuration);
                    Console.WriteLine("StartDate: " + StartDate);
                    Console.WriteLine("StartMonth: " + StartMonth);
                    Console.WriteLine("StartYear: " + StartYear);
                    Console.WriteLine("EndDate: " + EndDate);
                    Console.WriteLine("EndMonth: " + EndMonth);
                    Console.WriteLine("EndYear: " + EndYear);

                    HtmlWeb web = new();
                    //HtmlDocument doc = web.Load(link);

                    WebDriver driver1 = new ChromeDriver();
                    driver1.Navigate().GoToUrl(link);

                    string PageSource1 = GetFullyLoadedWebPageContent(driver1);
                    driver1.Close();

                    var pageDetails = new HtmlDocument();
                    pageDetails.LoadHtml(PageSource1);

                    var allLots = pageDetails.DocumentNode.SelectNodes("//a[@class='detail-link']");
                    var lotDetails = pageDetails.DocumentNode.SelectNodes("//ul[@class='lot-page__details__list']");

                    foreach (var lot in allLots)
                    {
                        var dimensionLength = "";
                        var dimensionWidth = "";
                        var unit = "";
                        var price = "";
                        var currency = "";
                        var refrenceNo = "";
                        var manufacturer = "";
                        var material = "";
                        var modelName = "";
                        var dimensionString = "";
                        var priceString = "";

                        HtmlDocument doc = new();
                        doc.LoadHtml(lot.InnerHtml);

                        var lotLink = lot.GetAttributes("href").First().Value;
                        HtmlDocument lotDoc = web.Load(lotLink);

                        var watchId = lotDoc.DocumentNode.SelectNodes("//h3[@class='lot-page__lot__number']").First().InnerText.Replace("Σ", "").Trim();
                        modelName = lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Model Name')]/following-sibling::text") != null ? lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Model Name')]/following-sibling::text").First().InnerText.Trim() : null;
                        var lotImageURL = doc.DocumentNode.SelectNodes("//div[@class='phillips-image']/img").First().GetAttributes("src").First().Value;
                        material = lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Material')]/following-sibling::text") != null ? lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Material')]/following-sibling::text").First().InnerText.Trim() : null;
                        dimensionString = lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Dimensions')]/following-sibling::text") != null ? lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Dimensions')]/following-sibling::text").First().InnerText.Trim() : null;
                        priceString = lotDoc.DocumentNode.SelectNodes("//p[@class='lot-page__lot__sold']") != null ? lotDoc.DocumentNode.SelectNodes("//p[@class='lot-page__lot__sold']").First().InnerText.Replace(",", "").Trim() : null;
                        manufacturer = lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Manufacturer')]/following-sibling::text") != null ? lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Manufacturer')]/following-sibling::text").First().InnerText.Replace(",", "").Trim() : null;
                        refrenceNo = lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Reference No')]/following-sibling::text") != null ? lotDoc.DocumentNode.SelectNodes("//strong[contains(text(),'Reference No')]/following-sibling::text").First().InnerText.Replace(",", "").Trim() : null;

                        Regex dimensionRegex = new(@"(\d+\.?\d+?)(\s?\,?\.?\-?\s?)(\w+)((\s?\,?\.?\-?\s?)(\w+)?(\s?\,?\.?\-?\s?)(\d+\.?\d+?)(\s?\,?\.?\-?\s?)(\w+))?", RegexOptions.IgnoreCase);
                        var dimensionMatchRegex = dimensionRegex.Match(dimensionString);

                        if (dimensionMatchRegex.Success)
                        {
                            dimensionLength = dimensionMatchRegex.Groups[1].Value;
                            dimensionWidth = dimensionMatchRegex.Groups[8].Value ?? null;
                            unit = dimensionMatchRegex.Groups[3].Value;
                        }

                        Regex priceRegex = new(@"^(\w+)?(\s?)(\w+)?(\s?)(\D*)(\d+)(\D*)$", RegexOptions.IgnoreCase);
                        var cost = priceRegex.Match(priceString);

                        if (cost.Success)
                        {
                            price = cost.Groups[6].Value;
                            currency = cost.Groups[5].Value;
                        }

                        Watch watch = new()
                        {
                            AuctionId = id,
                            WatchId = watchId,
                            ModelName = modelName,
                            ImageURL = imageURL,
                            Material = material,
                            DimensionLength = dimensionLength,
                            DimensionWidth = dimensionWidth,
                            Unit = unit,
                            Manufacturer = manufacturer,
                            Price = price,
                            Currency = currency,
                            ReferenceNo = refrenceNo
                        };
                        _context.tbl_Watch.Add(watch);

                        Console.WriteLine("----------Watch----------");
                        Console.WriteLine("AuctionId: " + id);
                        Console.WriteLine("WatchId: " + watchId);
                        Console.WriteLine("ModelName: " + modelName);
                        Console.WriteLine("Material: " + material);
                        Console.WriteLine("DimensionLength: " + dimensionLength);
                        Console.WriteLine("DimensionWidth: " + dimensionWidth);
                        Console.WriteLine("Unit: " + unit);
                        Console.WriteLine("Manufacturer: " + manufacturer);
                        Console.WriteLine("Price: " + price);
                        Console.WriteLine("Currency: " + currency);
                        Console.WriteLine("RefrenceNo: " + refrenceNo);
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                }
                _context.SaveChangesAsync();
                Console.WriteLine("Data Inserte Completed...!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}