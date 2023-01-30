using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phillips_Crawling_Task.Data
{
    public class Auctions
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageURL { get; set; }
        public string Link { get; set; }
        public string StartDate { get; set; }
        public string StartMonth { get; set; }
        public string StartYear { get; set; }
        public string EndDate { get; set; }
        public string EndMonth { get; set; }
        public string EndYear { get; set; }
    }
}
