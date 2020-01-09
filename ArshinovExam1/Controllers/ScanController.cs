using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using ArshinovExam1.Models;
using Microsoft.EntityFrameworkCore;

namespace ArshinovExam1.Controllers
{
    public class ScanController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetResult(string url, int? depth)
        {
            if(!depth.HasValue)
			{
				return null;
			}

            lock (allUrlsLock)
            {
	            allUrls = new List<string>();
            }
            await DoRecursion(url, depth.Value);
            lock (allUrlsLock)
            {
	            return View(allUrls);
            }
        }

        private List<string> allUrls;
        private readonly object allUrlsLock = new object();
		
		private async Task DoRecursion(string url, int depth)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			
			using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
			using(Stream stream = response.GetResponseStream())
			using(StreamReader reader = new StreamReader(stream))
			{
				lock (allUrlsLock)
				{
					allUrls.Add(url);
				}

				if (depth == 0)
				{
					return;
				}
				
				string html = await reader.ReadToEndAsync();
			
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				
				var urls = doc.DocumentNode.Descendants("a")
					.Where(x => x.Attributes.Contains("href"))
					.Select(x => x.Attributes["href"].Value)
					.Where(x => !x.Contains("http"))
					.Select(x => "https://" + response.ResponseUri.Host + x);

				List<Task> tasks = new List<Task>();
				foreach (var curUrl in urls)
				{
					tasks.Add(DoRecursion(curUrl, depth - 1));
				}
				await Task.WhenAll(tasks);
			}
		}

		[HttpPost]
		public async Task<IActionResult> SaveResults(ICollection<string> urls)
		{
			List<Task> tasks = new List<Task>();
			using (ApplicationDbContext db = new ApplicationDbContext())
			{
				foreach (var url in urls)
				{
					tasks.Add(WriteDB(db, url));
				}

				await Task.WhenAll(tasks);
				await db.SaveChangesAsync();
			}
			return Ok("Сохранено");
		}

		public async Task WriteDB(ApplicationDbContext db, string url)
		{
			if (db.Sites.Any(x => x.Url == url))
			{
				return;
			}
			
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		
			using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
			using(Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				string html = await reader.ReadToEndAsync();
				await db.Sites.AddAsync(new Site() {Url = url, Text = html.Substring(0, 500), Domain = response.ResponseUri.Host});
			}
		}
    }
}