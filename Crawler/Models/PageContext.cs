using Microsoft.EntityFrameworkCore;

namespace Crawler.Models
{
    public class PageContext : DbContext
    {
        public PageContext(DbContextOptions<PageContext> options)
            : base(options)
        {

        }

        public DbSet<Page> Pages { set; get; }
    }
}
