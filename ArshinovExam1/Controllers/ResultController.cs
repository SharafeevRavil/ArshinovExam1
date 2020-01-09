using System.Linq;
using System.Threading.Tasks;
using ArshinovExam1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArshinovExam1.Controllers
{
    public class ResultController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(string domain)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                return View(db.Sites.Where(x => x.Domain == domain).ToList());
            }
        }
    }
}