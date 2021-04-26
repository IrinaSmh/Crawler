using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Crawler.Models;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Crawler.Controllers
{
    [Route("crawler/")]
    [ApiController]
    public class PagesController : ControllerBase
    {
        private readonly PageContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PagesController(PageContext context, IHttpClientFactory clientFactory, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
            _serviceScopeFactory = serviceScopeFactory;
        }


        [HttpGet("status/{id}")]
        public async Task<ActionResult<int>> GetStatusCode(Guid id)
        {
            var page = await _context.Pages.FindAsync(id);

            if (page == null)
            {
                return NotFound();
            }

            return page.Status;
        }

        [HttpGet("result/{id}")]
        public async Task<ActionResult<int>> GetResult(Guid id)
        {
            var page = await _context.Pages.FindAsync(id);

            if (page == null || page.Status == 0 || page.Status == 2)
            {
                return NotFound();
            }

            return page.CountOfWords;
        }

        
        private async Task<int> GetAndSeparateHtml(PageContext pageContext, Guid id, string url)
        {
            int countOfWords = 0;
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _clientFactory.CreateClient();

            await EditPost(id, 0, countOfWords, pageContext);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                countOfWords = getWords(result);
                await EditPost(id, 1, countOfWords, pageContext);
            }
            else
            {
                await EditPost(id, 2, -1, pageContext);
            }
          
            return countOfWords;
        }

        private int getWords(string textHtml)
        {
            string result = textHtml;

            //удаление тегов
            result = Regex.Replace(result, "<[^>]+>", string.Empty);
            //удаление цифр
            result = Regex.Replace(result, "[0-9]", string.Empty);
            List<string> words = new List<string>();

            //выделение и подсчет слов
            Regex regex = new Regex(@"(\w+)");

            MatchCollection matches = regex.Matches(result);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                    words.Add(match.Value);
            }

            int count = words.Count;

            return count;
        }


        private async Task<ActionResult<Page>> EditPost(Guid? id, int statusCode, int result, PageContext pageContext)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pageToUpdate = await pageContext.Pages.FirstOrDefaultAsync(p=> p.Id == id);
            if (pageToUpdate != null)
            {
                pageToUpdate.Status = statusCode;
                pageToUpdate.CountOfWords = result;
            } 
               
            await pageContext.SaveChangesAsync();
            
            return CreatedAtAction("GetPage", new { id = pageToUpdate.Id }, pageToUpdate);
        }


        [HttpPost("crawl")]
        public async Task<Guid> PostPage(string url)
        {
            Page page = new Page(Guid.NewGuid(), url);
            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            new Thread(new ThreadStart(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<PageContext>();
                await GetAndSeparateHtml(db, page.Id, url);
            })).Start();

            return page.Id;
        }
    }
}
