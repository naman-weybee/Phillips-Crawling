using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phillips_Crawling_Task.Data
{
    public class Phillips_DBContext : DbContext
    {
        public Phillips_DBContext()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"data source=DESKTOP-9J2CV47; database=Phillips; integrated security=SSPI");
        }

        public DbSet<Watch_Auctions> tbl_Watch_Auctions { get; set; }
        public DbSet<Auction> tbl_Auction { get; set; }
    }
}