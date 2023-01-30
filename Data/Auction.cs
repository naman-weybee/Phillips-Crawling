using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phillips_Crawling_Task.Data
{
    public class Auction
    {
        [Key]
        public int Id { get; set; }
        public Watch_Auctions WatchAuctionId { get; set; }
        public string AuctionId { get; set; }
        public string ModelName { get; set; }
        public string ImageURL { get; set; }
        public string Material { get; set; }
        public string Dimensions { get; set; }
        public string Unit { get; set; }
    }
}
