# AlienFruit.Astra

<img src="design/logo.png" alt="AlienFruit.Astra Logo" height="64">

[![NuGet version](https://badge.fury.io/nu/AlienFruit.Astra.svg)](https://badge.fury.io/nu/AlienFruit.Astra)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/alienfruit/AlienFruit.Astra/workflows/CI/badge.svg)](https://github.com/alienfruit/AlienFruit.Astra/actions)

AlienFruit.Astra is a powerful .NET library designed to enhance ASP.NET applications (both MVC and Razor Pages) by enabling dynamic content loading and providing a seamless user experience with smooth navigation between pages without full page refreshes.

## Why Choose AlienFruit.Astra?

While modern frameworks like Blazor offer full client-side capabilities, AlienFruit.Astra provides a unique, lightweight alternative for developers who want to:

*   **Enhance Existing ASP.NET Apps:** Seamlessly integrate smooth AJAX navigation into traditional server-rendered MVC or Razor Pages applications with minimal code changes.
*   **Avoid SPA Complexity:** Achieve a modern, responsive user experience without the overhead and learning curve of a full Single Page Application (SPA) framework.
*   **Preserve Architecture:** Keep your existing controllers, views, and business logic intact, allowing for incremental improvements to user experience.

**What you'll gain:**
- ⚡ **Faster Page Loads:** Eliminate full page refreshes for quicker, more fluid user interactions.
- 🎯 **Improved User Experience:** Deliver smooth transitions, dynamic content updates, and better responsiveness across all devices.
- 🏗️ **Maintained Architecture:** Continue leveraging familiar ASP.NET patterns and infrastructure.
- 📱 **Mobile-Friendly by Design:** Enhance mobile user experience with efficient content loading.
- 🔧 **Easy Maintenance:** Benefit from simplified integration and ongoing management.

## Features

*   **Dynamic Content Loading:** Load partial views or content blocks dynamically without reloading the entire page, optimizing performance and perceived speed.
*   **AJAX Navigation:** Enable smooth, asynchronous navigation between pages, enhancing user experience and reducing server load.
*   **Nested ViewBoxes:** Construct complex, hierarchical UI structures with multiple independent dynamic content areas for modular and flexible layouts.
*   **JavaScript API:** Gain programmatic control over view-boxes from client-side code, allowing for advanced interactions and custom behaviors.
*   **Resource Management:** Automatically manage JavaScript and CSS resources, preventing duplicates, ensuring proper loading order, and optimizing caching.
*   **ViewBox Integration:** Facilitate the integration of isolated and reusable UI components within dynamic content areas.
*   **Highly Customizable:** Configure the library extensively to perfectly match your application's specific requirements and design patterns.

## Quick Start

Get AlienFruit.Astra up and running in your ASP.NET MVC or Razor Pages application with these minimal steps:

### 1. Installation

```bash
Install-Package AlienFruit.Astra
# Or using .NET CLI
dotnet add package AlienFruit.Astra
```

### 2. Register Services (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAstra(); // Register Astra services
```

### 3. Add Middleware (Program.cs)

```csharp
var app = builder.Build();
app.UseAstra(); // Add Astra middleware to the request pipeline
```

### 4. Configure View Files (_ViewImports.cshtml & _ViewStart.cshtml)

**_ViewImports.cshtml:**

```html
@addTagHelper *, AlienFruit.Astra
```

**_ViewStart.cshtml:**

```html
@using AlienFruit.Astra.Core
@inject AstraEngine AstraEngine
@{
    @AstraEngine.IncompleteLoadCheck()
    Layout = AstraEngine.RouteLayout(Context, "_Layout");
}
```

### 5. Add ViewBox to _Layout.cshtml

Wrap your main content (`@RenderBody()`) with a `<view-box>` tag:

```html
<view-box id="main-view-box" class="pb-3" role="main">
    @RenderBody()
</view-box>
```

### 6. Add Dynamic Links

Use `<view-box-link>` for AJAX navigation:

```html
<view-box-link view-box-id="main-view-box" uri="/Home">
    Home
</view-box-link>
```

### 7. Render Astra Headers (_Layout.cshtml)

Include `@AstraEngine.RenderHeaders()` in your `<head>` section:

```html
<!DOCTYPE html>
<html>
<head>
    @AstraEngine.RenderHeaders()
</head>
<body>
    <!-- ... -->
</body>
</html>
```

## Demo

See how AlienFruit.Astra transforms a traditional ASP.NET application into a modern, responsive app with smooth navigation:

[![AlienFruit.Astra Demo](https://img.youtube.com/vi/your-video-id/0.jpg)](https://www.youtube.com/watch?v=your-video-id)

## Advanced Usage

Explore the full capabilities of AlienFruit.Astra, including nested view-boxes, JavaScript API, resource management, and detailed configuration options.

### Nested ViewBoxes

AlienFruit.Astra supports hierarchical view-box structures, allowing you to create nested dynamic content areas. This enables complex UI layouts where different parts of the page can be updated independently.

```html
<!-- Parent view-box in _Layout.cshtml -->
<view-box id="main-view-box1" class="pb-3" role="main">
    @RenderBody()
</view-box>
```

```html
<!-- Child view-box in a page view (e.g., DynamicContent/Index.cshtml) -->
<view-box id="dynamic-content-box"
    parent-view-box-id="main-view-box1"
    class="pb-3"
    role="main"
    changing-browser-address-enable="false">
    <div class="alert alert-info">
        <h4>Select a tab to load content</h4>
    </div>
</view-box>
```

### JavaScript API

AlienFruit.Astra provides a JavaScript API for programmatic control over view-boxes:

```javascript
// Load content into the main view-box
ViewBoxRegistry.sendRequest('main-view-box1', '/Home/About');

// Dynamically load tab content
ViewBoxRegistry.sendRequest('dynamic-content-box', '/DynamicContent/Tab1');

// Cancel the current request
ViewBoxRegistry.abortCurrentRequest('main-view-box1');
```

### ViewBox Link Groups

Create groups of navigation elements that share a common visual state:

```html
<view-box-link-group id="main-nav-group"
    view-box-id="main-view-box1"
    default-class-name="nav-item"
    selected-class-name="nav-item active"
    uri-to-activete="@(["/Home", "/About", "/Contact"])">
    <!-- Links here -->
</view-box-link-group>
```

## Configuration

AlienFruit.Astra provides a flexible configuration system for optimizing resource handling and performance.

**Recommended configuration for most projects:**

```json
{
  "AstraConfiguration": {
    "EnableVersioning": true,
    "UseCompression": true,
    "CacheMaxAge": 86400
  }
}
```

Or programmatically:

```csharp
builder.AddAstra(options =>
{
    options.EnableVersioning = true;
    options.UseCompression = true;
    options.CacheMaxAge = 86400; // 24 hours
});
```

## Comparison with Other Solutions

| Feature | AlienFruit.Astra | Blazor Server | Blazor WebAssembly | Full SPA (React/Angular) |
|----------------|------------------|---------------|-------------------|---------------------------------|
| **Code Changes Required** | Minimal | Significant | Significant | Complete Rewrite |
| **Learning Curve** | Low | Medium | High | High |
| **Application Size** | Small | Medium | Large | Large |
| **SEO-Friendly** | ✅ Excellent | ✅ Excellent | ⚠️ Requires Setup | ⚠️ Requires Setup |
| **Performance** | ✅ Fast Loads | ⚠️ Connection Dependent | ✅ Fast After Load | ✅ Fast After Load |
| **Mobile Support** | ✅ Excellent | ✅ Excellent | ✅ Excellent | ✅ Excellent |

## Use Cases

- **Corporate Applications:** Incrementally enhance existing MVC applications without a full rewrite.
- **Dashboards:** Create dynamic dashboards with independent content areas.
- **Content Management Systems:** Improve navigation and user experience without changing the core architecture.
- **E-commerce Platforms:** Speed up catalog navigation and the checkout process.

## Community & Support

- 📖 [Documentation](https://github.com/alienfruit/AlienFruit.Astra/wiki)
- 🐛 [Report an Issue](https://github.com/alienfruit/AlienFruit.Astra/issues)
- 💬 [Discussions](https://github.com/alienfruit/AlienFruit.Astra/discussions)
- 📧 [Contact Us](mailto:support@alienfruit.dev)

## Contributing

We welcome contributions to AlienFruit.Astra! If you have suggestions for improvements, new features, or bug fixes, please open an issue or submit a pull request on our [GitHub repository](https://github.com/alienfruit/AlienFruit.Astra).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.