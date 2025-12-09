# AlienFruit.Astra

A powerful library for dynamic content loading in ASP.NET Core MVC applications that enables smooth, AJAX-powered navigation between website pages without full page refreshes.

## Features

- **Dynamic Content Loading**: Load page content dynamically via AJAX
- **Smooth Navigation**: Navigate between pages without full browser refreshes
- **Resource Management**: Efficiently manage CSS and JavaScript resources
- **Resource Versioning**: Automatic cache busting with content-based versioning
- **Compression Support**: Built-in resource compression using NUglify
- **Tag Helpers**: Convenient Razor syntax for including resources
- **Dependency Injection**: Full ASP.NET Core DI integration

## Installation

```bash
dotnet add package AlienFruit.Astra
```

## Quick Start

### 1. Configure Services

In `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Astra services
builder.AddAstra();
```

### 2. Configure Middleware

```csharp
var app = builder.Build();

// ... other middleware configuration ...

app.UseAstra();
```

### 3. Configure View Files

Add tag helpers and AstraEngine injection to your view files:

In `_ViewImports.cshtml`:
```cshtml
@addTagHelper *, AlienFruit.Astra
```

In `_ViewStart.cshtml`:
```cshtml
@using AlienFruit.Astra.Core
@inject AstraEngine AstraEngine
@{
    @AstraEngine.IncompleteLoadCheck()
    Layout = AstraEngine.RouteLayout(Context, "_Layout");
}
```

**Важно:** Строка `@AstraEngine.IncompleteLoadCheck()` обеспечивает механизм защиты от неполной загрузки ресурсов. Эта функция добавляет JavaScript код, который проверяет наличие специального мета-тега после загрузки DOM. Если мета-тег отсутствует (что может произойти при асинхронной загрузке ресурсов), страница автоматически перезагружается для корректного отображения.

### 4. Use in Razor Layout

Add AstraEngine injection and resource rendering to your `_Layout.cshtml`:

```cshtml
@using AlienFruit.Astra.Core
@using AlienFruit.Astra.Models
@using System.Reflection
@inject AstraEngine AstraEngine

<!DOCTYPE html>
<html>
<head>
    <!-- ... other head elements ... -->
    @AstraEngine.RenderHeaders()
</head>
<body>
    <!-- ... navigation and content ... -->
</body>
</html>
```

### 5. Create Dynamic Links and ViewBoxes

Use custom Razor tags for dynamic content loading:

```cshtml
@* Dynamic Link *@
<view-box-link id="home-link"
    default-class-name="nav-link"
    selected-class-name="nav-link active"
    view-box-id="main-content"
    uri="/Home/Index">
    Home
</view-box-link>

@* Content Container *@
<view-box id="main-content"
    class="container-fluid"
    role="main">
    @RenderBody()
</view-box>
```

## 6. Configuration

Add to `appsettings.json`:

```json
{
  "AstraConfiguration": {
    "EnableVersioning": true,
    "UseCompression": false,
    "CacheDurationMinutes": 60
  }
}
```

Or configure programmatically during service registration:

```csharp
builder.AddAstra(options =>
{
    options.EnableVersioning = true;
    options.UseCompression = false;
    options.CacheMaxAge = 3600; // 1 hour in seconds
});
```

## 7. Resource Versioning

Astra supports automatic resource versioning for cache busting:

- **Content Hashing**: Automatic SHA256 hashing of resource content
- **Global Version**: Optional global version for all resources
- **Tag Helpers**: Convenient syntax for versioned resources

## Documentation

For detailed documentation, visit the [GitHub repository](https://github.com/alienfruit/AlienFruit.Astra).

## License

MIT