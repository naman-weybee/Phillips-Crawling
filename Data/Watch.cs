using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phillips_Crawling_Task.Data
{
    public class Watch
    {
        [Key]
        public int Id { get; set; }
        public Auctions Auction { get; set; }
        public string AuctionId { get; set; }
        public string? WatchId { get; set; }
        public string? ModelName { get; set; }
        public string? ImageURL { get; set; }
        public string? Material { get; set; }
        public string? DimensionLength { get; set; }
        public string? DimensionWidth { get; set; }
        public string? Unit { get; set; }
        public string? Manufacturer { get; set; }
        public string? Price { get; set; }
        public string? Currency { get; set; }
        public string? ReferenceNo { get; set; }
    }
}
