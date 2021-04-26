using System;

namespace Crawler.Models
{
    public class Page
    {

        public Page(Guid id, string url)
        {
            this.id = id;
            this.url = url;
        }

        public Guid id { set; get; }
        public string url { set; get; }
        public int status { set; get; }
        public int countOfWords { set; get; }
    }
}
