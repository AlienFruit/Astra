using AlienFruit.Astra.Demo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AlienFruit.Astra.Demo.Controllers
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

        public async Task<IActionResult> LongLoading()
        {
            // Имитация долгой загрузки - задержка 3 секунды
            await Task.Delay(3000);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
