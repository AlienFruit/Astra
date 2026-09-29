using Microsoft.AspNetCore.Mvc;

namespace AlienFruit.Astra.Demo.Controllers
{
    public class AccordionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("accordion/content/{accordionName}")]
        public async Task<IActionResult> AccordionContent(string accordionName)
        {
            await Task.Delay(1000);
            return Ok($"Content for {accordionName}");
        }
    }
}