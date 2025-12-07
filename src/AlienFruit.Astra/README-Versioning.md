# Resource Versioning in AlienFruit.Astra

## Overview

AlienFruit.Astra now supports resource versioning for cache busting, similar to the `asp-append-version` mechanism in ASP.NET Core.

## Configuration

### Enabling Versioning

Add to `appsettings.json`:

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

### Configuration Parameters

- `EnableVersioning` (bool) - enables/disables versioning
- `ResourceVersion` (string) - global version for all resources (optional)

## Usage Methods

### 1. Automatic Versioning in RenderHeaders()

When versioning is enabled, all resources in `RenderHeaders()` automatically receive version parameter:

```csharp
// In controller or service
htmlResourceRenderer.AddStylesheetResource("my-style", "path/to/style.css");
htmlResourceRenderer.AddScriptResource("my-script", "path/to/script.js");

// В Razor view
@resourceResolver.RenderHeader()
```

Result:
```html
<link href="/astra/my-style?v=abc12345" rel="stylesheet" type="text/css" />
<script src="/astra/my-script?v=def67890"></script>
```

### 2. Using Tag Helper

Create a Tag Helper for convenient use in Razor views:

```html
<!-- В Razor view -->
<astra-style href="my-style"></astra-style>
<astra-script src="my-script"></astra-script>
```

### 3. Programmatic URL Retrieval

```csharp
// In controller
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

## Versioning Algorithm

1. **Global version**: If `ResourceVersion` is specified, it is used
2. **Content hash**: If global version is not specified, SHA256 hash of resource content is calculated
3. **Fallback**: On hash calculation error, timestamp is used

## ASP.NET Core Integration

### Service Registration

```csharp
// В Program.cs
builder.AddAstra();

// In route configuration
app.UseAstra();
```

### Using Together with asp-append-version

You can use both mechanisms simultaneously:

```html
<!-- Static files with asp-append-version -->
<link href="~/css/site.css" asp-append-version="true" />

<!-- Astra resources with automatic versioning -->
@resourceResolver.RenderHeader()
```

## Examples

### Complete Controller Example

```csharp
public class DemoController : Controller
{
    private readonly IHtmlResourceRenderer _resourceRenderer;
    
    public DemoController(IHtmlResourceRenderer resourceRenderer)
    {
        _resourceRenderer = resourceRenderer;
        
        // Register resources
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

<!-- Using Tag Helper -->
<astra-style href="demo-style"></astra-style>
<astra-script src="demo-script"></astra-script>

<!-- Or via RenderHeader() -->
@resourceResolver.RenderHeader()

<div id="demo-content">
    <h1>Versioning Demonstration</h1>
    <button id="demo-button">Click me</button>
</div>
```

## Advantages

1. **Automatic cache busting**: When resource content changes, URL is automatically updated
2. **Performance**: Hashes are calculated once during resource registration
3. **Flexibility**: Support for both global version and individual hashes
4. **Compatibility**: Works together with existing ASP.NET Core mechanisms
5. **Ease of use**: Minimal changes to existing code

## Migration

To add versioning to existing code:

1. Add `EnableVersioning: true` to configuration
2. Existing `RenderHeader()` calls will automatically receive versioning
3. Optionally add Tag Helper for more convenient usage 