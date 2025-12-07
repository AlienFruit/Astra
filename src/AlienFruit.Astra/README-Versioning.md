# Версионирование ресурсов в AlienFruit.Astra

## Обзор

AlienFruit.Astra теперь поддерживает версионирование ресурсов для кэш-бастинга, аналогично механизму `asp-append-version` в ASP.NET Core.

## Конфигурация

### Включение версионирования

Добавьте в `appsettings.json`:

```json
{
  "AstraConfiguration": {
    "UseCompression": true,
    "ResourcesRoute": "astra",
    "EnableVersioning": true,
    "ResourceVersion": "1.0.0"
  }
}
```

### Параметры конфигурации

- `EnableVersioning` (bool) - включает/выключает версионирование
- `ResourceVersion` (string) - глобальная версия для всех ресурсов (опционально)

## Способы использования

### 1. Автоматическое версионирование в RenderHeaders()

При включенном версионировании все ресурсы в `RenderHeaders()` автоматически получают параметр версии:

```csharp
// В контроллере или сервисе
htmlResourceRenderer.AddStylesheetResource("my-style", "path/to/style.css");
htmlResourceRenderer.AddScriptResource("my-script", "path/to/script.js");

// В Razor view
@resourceResolver.RenderHeader()
```

Результат:
```html
<link href="/astra/my-style?v=abc12345" rel="stylesheet" type="text/css" />
<script src="/astra/my-script?v=def67890"></script>
```

### 2. Использование Tag Helper

Создайте Tag Helper для удобного использования в Razor views:

```html
<!-- В Razor view -->
<astra-style href="my-style"></astra-style>
<astra-script src="my-script"></astra-script>
```

### 3. Программное получение URL

```csharp
// В контроллере
public class HomeController : Controller
{
    private readonly IAstraResourceUrlHelper _urlHelper;
    
    public HomeController(IAstraResourceUrlHelper urlHelper)
    {
        _urlHelper = urlHelper;
    }
    
    public IActionResult Index()
    {
        var scriptUrl = _urlHelper.GetResourceUrl("my-script");
        ViewBag.ScriptUrl = scriptUrl;
        return View();
    }
}
```

## Алгоритм версионирования

1. **Глобальная версия**: Если указан `ResourceVersion`, используется он
2. **Хеш содержимого**: Если глобальная версия не указана, вычисляется SHA256 хеш содержимого ресурса
3. **Fallback**: При ошибке вычисления хеша используется временная метка

## Интеграция с ASP.NET Core

### Регистрация сервисов

```csharp
// В Program.cs
builder.AddAstra();

// В конфигурации маршрутов
app.UseAstra();
```

### Использование вместе с asp-append-version

Вы можете использовать оба механизма одновременно:

```html
<!-- Статические файлы с asp-append-version -->
<link href="~/css/site.css" asp-append-version="true" />

<!-- Astra ресурсы с автоматическим версионированием -->
@resourceResolver.RenderHeader()
```

## Примеры

### Полный пример контроллера

```csharp
public class DemoController : Controller
{
    private readonly IHtmlResourceRenderer _resourceRenderer;
    
    public DemoController(IHtmlResourceRenderer resourceRenderer)
    {
        _resourceRenderer = resourceRenderer;
        
        // Регистрируем ресурсы
        _resourceRenderer.AddStylesheetResource("demo-style", "Demo.Resources.style.css");
        _resourceRenderer.AddScriptResource("demo-script", "Demo.Resources.script.js");
    }
    
    public IActionResult Index()
    {
        return View();
    }
}
```

### Razor View

```html
@{
    ViewData["Title"] = "Demo";
}

<!-- Использование Tag Helper -->
<astra-style href="demo-style"></astra-style>
<astra-script src="demo-script"></astra-script>

<!-- Или через RenderHeader() -->
@resourceResolver.RenderHeader()

<div id="demo-content">
    <h1>Демонстрация версионирования</h1>
    <button id="demo-button">Нажми меня</button>
</div>
```

## Преимущества

1. **Автоматический кэш-бастинг**: При изменении содержимого ресурса URL автоматически обновляется
2. **Производительность**: Хеши вычисляются один раз при регистрации ресурса
3. **Гибкость**: Поддержка как глобальной версии, так и индивидуальных хешей
4. **Совместимость**: Работает вместе с существующими механизмами ASP.NET Core
5. **Простота использования**: Минимальные изменения в существующем коде

## Миграция

Для добавления версионирования к существующему коду:

1. Добавьте `EnableVersioning: true` в конфигурацию
2. Существующие вызовы `RenderHeader()` автоматически получат версионирование
3. При необходимости добавьте Tag Helper для более удобного использования 