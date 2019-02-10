using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SampleSite.Models;
using SampleSite.Nuget;
using Microsoft.AspNetCore.Mvc;

namespace SampleSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> MyNuget()
        {
            var es = (await PackageSearch.GetAsync("Matteo Fabbri"))
                .Data
                .Where(x => x.Authors.Length == 1 && x.Authors[0] == "Matteo Fabbri")
                .OrderBy(x => x.Title)
                .ToArray();

            return View(es);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
