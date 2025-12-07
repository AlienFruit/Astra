using Microsoft.AspNetCore.Mvc;

namespace AlienFruit.Astra.Demo.Controllers
{
    public class DynamicContentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Tab1()
        {
            var html = @"
<div class=""card"">
    <div class=""card-header bg-primary text-white"">
        <h3 class=""mb-0"">Tab 1 - First Content</h3>
    </div>
    <div class=""card-body"">
        <h5 class=""card-title"">Welcome to the first tab!</h5>
        <p class=""card-text"">
            This is the first example of dynamically loaded content. Content loads without changing the browser address.
        </p>
        <ul class=""list-group list-group-flush"">
            <li class=""list-group-item"">List item 1</li>
            <li class=""list-group-item"">List item 2</li>
            <li class=""list-group-item"">List item 3</li>
        </ul>
    </div>
</div>";
            return Content(html, "text/html");
        }

        public IActionResult Tab2()
        {
            // This content is loaded from Tab2.cshtml view file
            return View();
        }

        public IActionResult Tab3()
        {
            // This content is loaded from Tab3.cshtml view file
            return View();
        }
    }
}

