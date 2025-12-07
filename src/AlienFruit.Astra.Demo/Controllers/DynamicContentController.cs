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
        <h3 class=""mb-0"">Tab 1 - Первый контент</h3>
    </div>
    <div class=""card-body"">
        <h5 class=""card-title"">Добро пожаловать на первую вкладку!</h5>
        <p class=""card-text"">
            Это первый пример динамически загружаемого контента. Контент загружается без изменения адреса в браузере.
        </p>
        <ul class=""list-group list-group-flush"">
            <li class=""list-group-item"">Элемент списка 1</li>
            <li class=""list-group-item"">Элемент списка 2</li>
            <li class=""list-group-item"">Элемент списка 3</li>
        </ul>
    </div>
</div>";
            return Content(html, "text/html");
        }

        public IActionResult Tab2()
        {
            var html = @"
<div class=""card"">
    <div class=""card-header bg-success text-white"">
        <h3 class=""mb-0"">Tab 2 - Второй контент</h3>
    </div>
    <div class=""card-body"">
        <h5 class=""card-title"">Вторая вкладка активна!</h5>
        <p class=""card-text"">
            Это второй пример динамически загружаемого контента. Обратите внимание, что адрес в браузере не изменился.
        </p>
        <div class=""row"">
            <div class=""col-md-6"">
                <div class=""card bg-light"">
                    <div class=""card-body"">
                        <h6>Блок 1</h6>
                        <p>Информация в первом блоке</p>
                    </div>
                </div>
            </div>
            <div class=""col-md-6"">
                <div class=""card bg-light"">
                    <div class=""card-body"">
                        <h6>Блок 2</h6>
                        <p>Информация во втором блоке</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>";
            return Content(html, "text/html");
        }

        public IActionResult Tab3()
        {
            // Этот контент загружается из View файла Tab3.cshtml
            return View();
        }
    }
}

