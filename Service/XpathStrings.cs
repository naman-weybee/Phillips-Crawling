using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phillips_Crawling_Task.Service
{
    public class XpathStrings
    {
        public static readonly string AllWatchAuctionsXpath = "//li[@class='has-image auction col-sm-2']";
        public static readonly string SingleTitle = @"./div[@class='content-body col-sm-2 col-md-5']//h2/a";
        public static readonly string SingleImageURL = @".//a//img";
        public static readonly string SingleLink = @".//a";
        public static readonly string SingleTimeDuration = @".//p";
        public static readonly string allLotsString = "//div[@class='phillips-lot']";
        public static readonly string singleLotLink = ".//a[@class='detail-link']";
        public static readonly string singleWatchId = ".//strong[@class='phillips-lot__description__lot-number-wrapper__lot-number']";
        public static readonly string singleModelName = "//strong[contains(text(),'Model Name')]/following-sibling::text";
        public static readonly string singleLoadImageURL = "//div[@class='phillips-image']/img";
        public static readonly string singleMaterial = "//strong[contains(text(),'Material')]/following-sibling::text";
        public static readonly string singleDimensionString = "//strong[contains(text(),'Dimensions')]/following-sibling::text";
        public static readonly string singlePriceString = "//p[@class='lot-page__lot__sold']";
        public static readonly string singleManufacturer = "//strong[contains(text(),'Manufacturer')]/following-sibling::text";
        public static readonly string singleRefrenceNo = "//strong[contains(text(),'Reference No')]/following-sibling::text";
    }
}
