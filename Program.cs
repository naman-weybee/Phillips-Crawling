using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Phillips_Crawling_Task.Data;
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Phillips_Crawling
{
    class Program
    {
        private static readonly Phillips_DBContext _context = new();
        private const string Url = "https://www.phillips.com/auctions/past/filter/Departments%3DWatches/sort/newest";

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
                    Thread.Sleep(1500);
                }
                else
                {
                    Thread.Sleep(2500);
                    break;
                }
            } while (true);
            return driver.PageSource;
        }

        public static async void GetWatchAuctionDetails()
        {
            ChromeOptions opt = new();
            opt.AddArguments("--headless");
            opt.AddArgument("--log-level=3");
            opt.AddArguments("--disable-gpu");
            opt.AddArguments("--start-maximized");
            opt.AddArguments("--no-sandbox");

            //WebDriver driver = new ChromeDriver(opt);
            ChromeDriver driver = new(ChromeDriverService.CreateDefaultService(), opt, TimeSpan.FromMinutes(3));

            driver.Navigate().GoToUrl(Url);

            string AuctionPageSource = GetFullyLoadedWebPageContent(driver);
            var Details = new HtmlDocument();
            Details.LoadHtml(AuctionPageSource);

            var AllWatchAuctions = Details.DocumentNode.SelectNodes("//li[@class='has-image auction col-sm-2']");
            var SingleTitle = @"./div[@class='content-body col-sm-2 col-md-5']//h2/a";
            var SingleImageURL = @".//a//img";
            var SingleLink = @".//a";
            var SingleTimeDuration = @".//p";
            var allLotsString = "//div[@class='phillips-lot']";
            var singleLotLink = ".//a[@class='detail-link']";
            var singleWatchId = ".//strong[@class='phillips-lot__description__lot-number-wrapper__lot-number']";
            var singleModelName = "//strong[contains(text(),'Model Name')]/following-sibling::text";
            var singleLoadImageURL = "//div[@class='phillips-image']/img";
            var singleMaterial = "//strong[contains(text(),'Material')]/following-sibling::text";
            var singleDimensionString = "//strong[contains(text(),'Dimensions')]/following-sibling::text";
            var singlePriceString = "//p[@class='lot-page__lot__sold']";
            var singleManufacturer = "//strong[contains(text(),'Manufacturer')]/following-sibling::text";
            var singleRefrenceNo = "//strong[contains(text(),'Reference No')]/following-sibling::text";

            Regex timeDurationRegex = new(@"(\d+)(\s?\,?\-?\&?\s?)((\d+)?(\s?\,?\-?\s?)(\w+)?(\s?\,?\-?\s?)(\d{4})?(((\s?\,?\-?\s?))(\d+)?(\s?\,?\-?\s?)(\d+)?(\s?\,?\-?\s?)(\w+)?(\s?\,?\-?\s?)(\d{4}))?)?", RegexOptions.IgnoreCase);
            Regex uniqueIdRegex = new(@"(^/)?(\w+)$", RegexOptions.IgnoreCase);
            Regex dimensionRegex = new(@"(\d+\.?\d+?)(\s?\,?\.?\-?\s?)(\w+)((\s?\,?\.?\-?\s?)(\w+)?(\s?\,?\.?\-?\s?)(\d+\.?\d+?)(\s?\,?\.?\-?\s?)(\w+))?", RegexOptions.IgnoreCase);
            Regex priceRegex = new(@"^(\w+)?(\s?)(\w+)?(\s?)(\D*)(\d+)(\D*)$", RegexOptions.IgnoreCase);
            Regex watchIdRegex = new(@"(\w+)?(\s?\-?\,?)(\d+)?(\s?\-?\,?)(\w+)?(\s?\-?\,?)(\d+)?", RegexOptions.IgnoreCase);

            try
            {
                foreach (var watchAuction in AllWatchAuctions)
                {
                    var Title = string.Empty;
                    var imageURL = string.Empty;
                    var link = string.Empty;
                    var timeDuration = string.Empty;
                    var StartDate = string.Empty;
                    var StartMonth = string.Empty;
                    var StartYear = string.Empty;
                    var EndDate = string.Empty;
                    var EndMonth = string.Empty;
                    var EndYear = string.Empty;

                    Title = watchAuction.SelectSingleNode(SingleTitle)?.InnerHtml.Replace("&amp;#8211 ", "&").Replace("&amp;", "&").Replace("&nbsp", "").Trim() ?? string.Empty;
                    imageURL = watchAuction.SelectSingleNode(SingleImageURL)?.GetAttributes("src").First().Value ?? string.Empty;
                    link = "https://www.phillips.com/" + watchAuction.SelectSingleNode(SingleLink)?.GetAttributes("href").First().Value ?? string.Empty;
                    timeDuration = watchAuction.SelectSingleNode(SingleTimeDuration)?.InnerHtml.Trim() ?? string.Empty;

                    var id = uniqueIdRegex.Match(link).Value;
                    var timeMatchRegex = timeDurationRegex.Match(timeDuration.Replace("&amp;#8211 ", "&").Replace("&amp;", "&").Replace("&nbsp", "").Replace("\n", ""));

                    if (timeMatchRegex.Success)
                    {
                        StartDate = timeMatchRegex.Groups[1].Value;
                        StartMonth = timeMatchRegex.Groups[6].Value == string.Empty ? timeMatchRegex.Groups[16].Value : timeMatchRegex.Groups[6].Value;
                        StartYear = timeMatchRegex.Groups[8].Value == string.Empty ? timeMatchRegex.Groups[18].Value : timeMatchRegex.Groups[8].Value;
                        EndDate = timeMatchRegex.Groups[12].Value == string.Empty ? timeMatchRegex.Groups[4].Value == string.Empty ? timeMatchRegex.Groups[1].Value : timeMatchRegex.Groups[4].Value : timeMatchRegex.Groups[12].Value;
                        EndMonth = timeMatchRegex.Groups[16].Value == string.Empty ? timeMatchRegex.Groups[6].Value : timeMatchRegex.Groups[16].Value;
                        EndYear = timeMatchRegex.Groups[18].Value == string.Empty ? timeMatchRegex.Groups[8].Value : timeMatchRegex.Groups[18].Value;
                    }

                    Console.WriteLine($"----------Auction with Id = {id}----------");
                    Console.WriteLine($"Id: {id}");
                    Console.WriteLine($"Title: {Title}");
                    Console.WriteLine($"ImageURL: {imageURL}");
                    Console.WriteLine($"Link: {link}");
                    Console.WriteLine($"TimeDuration: {timeDuration}");
                    Console.WriteLine($"StartDate: {StartDate}");
                    Console.WriteLine($"StartMonth: {StartMonth}");
                    Console.WriteLine($"StartYear: {StartYear}");
                    Console.WriteLine($"EndDate: {EndDate}");
                    Console.WriteLine($"EndMonth: {EndMonth}");
                    Console.WriteLine($"EndYear: {EndYear}");
                    Console.WriteLine();

                    var auction = await _context.tbl_Auctions.Where(x => x.Id == id).FirstOrDefaultAsync();
                    if (auction != null)
                    {
                        auction.Id = id;
                        auction.Title = Title;
                        auction.ImageURL = imageURL;
                        auction.Link = link;
                        auction.StartDate = StartDate;
                        auction.StartMonth = StartMonth;
                        auction.StartYear = StartYear;
                        auction.EndDate = EndDate;
                        auction.EndMonth = EndMonth;
                        auction.EndYear = EndYear;
                        _context.tbl_Auctions.Update(auction);
                    }
                    else
                    {
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
                        await _context.tbl_Auctions.AddAsync(auctions);
                    }

                    driver.Navigate().GoToUrl(link);
                    string LotPageSource = GetFullyLoadedWebPageContent(driver);

                    HtmlWeb web = new();
                    var pageDetails = new HtmlDocument();
                    pageDetails.LoadHtml(LotPageSource);

                    var allLots = pageDetails.DocumentNode.SelectNodes(allLotsString);
                    if (allLots != null)
                    {
                        foreach (var lot in allLots)
                        {
                            if (lot != null)
                            {
                                var dimensionLength = string.Empty;
                                var dimensionWidth = string.Empty;
                                var unit = string.Empty;
                                var price = string.Empty;
                                var currency = string.Empty;
                                var refrenceNo = string.Empty;
                                var manufacturer = string.Empty;
                                var material = string.Empty;
                                var modelName = string.Empty;
                                var dimensionString = string.Empty;
                                var priceString = string.Empty;
                                var watchId = string.Empty;
                                var watchIdString = string.Empty;
                                var lotImageURL = string.Empty;

                                HtmlDocument doc = new();
                                doc.LoadHtml(lot.InnerHtml);

                                var lotLink = lot?.SelectSingleNode(singleLotLink)?.GetAttributes("href").First().Value ?? string.Empty;
                                if (!string.IsNullOrEmpty(lotLink))
                                {
                                    //HtmlDocument lotDoc = web.Load(lotLink);
                                    driver.Navigate().GoToUrl(lotLink);
                                    string lotSource = GetFullyLoadedWebPageContent(driver);
                                    var lotDoc = new HtmlDocument();
                                    lotDoc.LoadHtml(lotSource);

                                    watchIdString = doc.DocumentNode.SelectSingleNode(singleWatchId)?.InnerText.Replace("Σ", "").Replace("?", "").Replace("~", "").Replace("≈", "").Replace("&nbsp", "").Trim() ?? string.Empty;
                                    modelName = lotDoc.DocumentNode.SelectNodes(singleModelName)?.First().InnerText.Trim() ?? string.Empty;
                                    lotImageURL = doc.DocumentNode.SelectNodes(singleLoadImageURL)?.First().GetAttributes("src").First().Value ?? string.Empty;
                                    material = lotDoc.DocumentNode.SelectNodes(singleMaterial)?.First().InnerText.Trim() ?? string.Empty;
                                    dimensionString = lotDoc.DocumentNode.SelectNodes(singleDimensionString)?.First().InnerText.Trim() ?? string.Empty;
                                    priceString = lotDoc.DocumentNode.SelectNodes(singlePriceString)?.First().InnerText.Replace(",", "").Trim() ?? string.Empty;
                                    manufacturer = lotDoc.DocumentNode.SelectNodes(singleManufacturer)?.First().InnerText.Trim() ?? string.Empty;
                                    refrenceNo = lotDoc.DocumentNode.SelectNodes(singleRefrenceNo)?.First().InnerText.Trim() ?? string.Empty;

                                    var dimensionMatchRegex = dimensionRegex.Match(dimensionString!) ?? null;
                                    var cost = priceRegex.Match(priceString!) ?? null;
                                    var watchIdMatchRegex = watchIdRegex.Match(watchIdString!) ?? null;

                                    if (dimensionMatchRegex!.Success)
                                    {
                                        dimensionLength = dimensionMatchRegex?.Groups[1].Value ?? string.Empty;
                                        dimensionWidth = dimensionMatchRegex?.Groups[8].Value ?? string.Empty;
                                        unit = dimensionMatchRegex?.Groups[3].Value ?? string.Empty;
                                    }

                                    if (cost!.Success)
                                    {
                                        price = cost?.Groups[6].Value ?? string.Empty;
                                        currency = cost?.Groups[5].Value ?? string.Empty;
                                    }

                                    if (watchIdMatchRegex!.Success)
                                    {
                                        watchId = watchIdMatchRegex?.Groups[1].Value.Trim() ?? string.Empty;
                                    }

                                    Console.WriteLine($"----------Watch with watchId = {watchId}----------");
                                    Console.WriteLine($"AuctionId: {id}");
                                    Console.WriteLine($"WatchId: {watchId}");
                                    Console.WriteLine($"ModelName: {modelName}");
                                    Console.WriteLine($"Manufacturer: {manufacturer}");
                                    Console.WriteLine($"ImageURL: {lotImageURL}");
                                    Console.WriteLine($"Material: {material}");
                                    Console.WriteLine($"DimensionLength: {dimensionLength}");
                                    Console.WriteLine($"DimensionWidth: {dimensionWidth}");
                                    Console.WriteLine($"Unit: {unit}");
                                    Console.WriteLine($"Price: {price}");
                                    Console.WriteLine($"Currency: {currency}");
                                    Console.WriteLine($"RefrenceNo: {refrenceNo}");
                                    Console.WriteLine();

                                    var watch = await _context.tbl_Watch.Where(x => x.AuctionId == id && x.WatchId == watchId).FirstOrDefaultAsync();
                                    if (watch != null)
                                    {
                                        watch.Id = watch.Id;
                                        watch.AuctionId = id;
                                        watch.WatchId = watchId;
                                        watch.ModelName = modelName;
                                        watch.ImageURL = lotImageURL;
                                        watch.Material = material;
                                        watch.DimensionLength = dimensionLength;
                                        watch.DimensionWidth = dimensionWidth;
                                        watch.Unit = unit;
                                        watch.Manufacturer = manufacturer;
                                        watch.Price = price;
                                        watch.Currency = currency;
                                        watch.ReferenceNo = refrenceNo;
                                        _context.tbl_Watch.Update(watch);
                                    }
                                    else
                                    {
                                        Watch newWatch = new()
                                        {
                                            AuctionId = id,
                                            WatchId = watchId,
                                            ModelName = modelName,
                                            ImageURL = lotImageURL,
                                            Material = material,
                                            DimensionLength = dimensionLength,
                                            DimensionWidth = dimensionWidth,
                                            Unit = unit,
                                            Manufacturer = manufacturer,
                                            Price = price,
                                            Currency = currency,
                                            ReferenceNo = refrenceNo
                                        };
                                        await _context.tbl_Watch.AddAsync(newWatch);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("This lot is no longer available...!");
                                    Console.WriteLine();
                                }
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    Console.WriteLine();
                    Console.WriteLine("====================================================================================================");
                    Console.WriteLine($"Data Inserted/Updated Successfully in Auctions and Watch Table with AuctionId = {id}");
                    Console.WriteLine("====================================================================================================");
                    Console.WriteLine();
                }
                driver.Close();
                Console.WriteLine("Data Inserted/Updated Successfully...!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}