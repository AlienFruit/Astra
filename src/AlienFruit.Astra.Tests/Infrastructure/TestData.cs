namespace AlienFruit.Astra.Tests.Infrastructure;

/// <summary>
/// Тестовые данные и константы для использования в тестах
/// </summary>
public static class TestData
{
    // Идентификаторы
    public const string TestViewBoxId = "test-viewbox";
    public const string TestViewBoxLinkId = "test-link";
    public const string ParentViewBoxId = "parent-viewbox";
    public const string ChildViewBoxId = "child-viewbox";

    // URI и пути
    public const string TestUri = "/test";
    public const string HomeUri = "/home";
    public const string AboutUri = "/about";
    public const string ContactUri = "/contact";
    public const string AjaxUri = "/api/test";

    // CSS классы
    public const string DefaultClass = "nav-link";
    public const string SelectedClass = "nav-link active";
    public const string PrimaryClass = "btn btn-primary";
    public const string PrimaryActiveClass = "btn btn-primary active";

    // JavaScript функции
    public const string OnStartLoadingFunction = "onStartLoading";
    public const string OnFinishLoadingFunction = "onFinishLoading";
    public const string OnTimeoutFunction = "onTimeout";
    public const string OnScriptsExecutedFunction = "onScriptsExecuted";
    public const string OnClickFunction = "onLinkClick";

    // HTML контент
    public const string TestHtmlContent = "<div>Test content</div>";
    public const string ErrorMessage = "Connection error occurred";
    public const string DefaultErrorMessage = "Failed to connect to server";

    // Ресурсы
    public const string TestResourceName = "test-resource.js";
    public const string ViewBoxJsResource = "viewbox.js";
    public const string LoadCheckJsResource = "load-check.js";
    public const string TestTemplatePath = "AlienFruit.Astra.TestTemplate.js";

    // HTTP заголовки для AJAX
    public const string AjaxHeaderName = "X-Requested-With";
    public const string AjaxHeaderValue = "XMLHttpRequest";

    // Задержки и таймауты
    public const int DefaultStartLoadingDelay = 100;
    public const int CustomStartLoadingDelay = 200;

    // Тестовые пути к embedded ресурсам
    public const string TestEmbeddedResourcePath = "AlienFruit.Astra.Test.Resource.js";
    public const string ViewBoxErrorResourcePath = "AlienFruit.Astra.ViewBox.Resources.ViewBoxError.html";

    // Стилевые атрибуты
    public const string TestStyle = "color: red; font-size: 14px;";
    public const string TestClass = "test-class another-class";

    // ARIA роли
    public const string MainRole = "main";
    public const string NavigationRole = "navigation";
    public const string ComplementaryRole = "complementary";

    // HTML теги
    public const string DivTag = "div";
    public const string SpanTag = "span";
    public const string AnchorTag = "a";
    public const string MainTag = "main";

    // MIME типы
    public const string JavaScriptMimeType = "application/javascript";
    public const string CssMimeType = "text/css";
    public const string HtmlMimeType = "text/html";

    // Расширения файлов
    public const string JsExtension = ".js";
    public const string CssExtension = ".css";
    public const string HtmlExtension = ".html";
}
