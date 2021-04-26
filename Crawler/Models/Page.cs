using System;

namespace Crawler.Models
{
    public class Page
    {

        public Page(Guid id, string url)
        {
            this.Id = id;
            this.Url = url;
        }

        public Guid Id { set; get; }
        public string Url { set; get; }
        public int Status { set; get; }
        public int CountOfWords { set; get; }
    }
}
