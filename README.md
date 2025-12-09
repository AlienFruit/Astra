# AlienFruit.Astra

<img src="design/logo.png" alt="AlienFruit.Astra Logo" height="64">

AlienFruit.Astra is a powerful .NET library designed to enhance ASP.NET MVC applications by enabling dynamic content loading and providing a seamless user experience with smooth navigation between pages without full page refreshes.

## Features

*   **Dynamic Content Loading:** Load partial views or content blocks dynamically without reloading the entire page.
*   **AJAX Navigation:** Navigate between pages using AJAX, improving performance and user experience.
*   **Resource Management:** Efficiently manage JavaScript and CSS resources, preventing duplicates and ensuring proper loading.
*   **ViewBox Integration:** Integrate with the ViewBox pattern for isolated and reusable UI components.
*   **Highly Customizable:** Easily configure the library to fit your application's specific needs.

## Installation

Install AlienFruit.Astra via NuGet Package Manager Console:

```bash
Install-Package AlienFruit.Astra
```

Or using the .NET CLI:

```bash
dotnet add package AlienFruit.Astra
```

## Usage

### Basic Setup

### 1.  **Register services** in your `Program.cs`:

```csharp
    var builder = WebApplication.CreateBuilder(args);
    // ... service registrations ...

    // Register Astra services
    builder.AddAstra();
```

### 2.  **Add the Astra middleware** to your application's request pipeline:

```csharp
    var app = builder.Build();
    // ... other middleware configuration ...
    app.UseAstra();
```

### 3. Configure View Files

Add tag helpers and AstraEngine injection to your view files:

In `_ViewImports.cshtml`:

```html
@addTagHelper *, AlienFruit.Astra
```

**What this directive does:** Registers all Tag Helpers from the AlienFruit.Astra assembly, allowing you to use special HTML tags `<view-box>` and `<view-box-link>` in your Razor views to create dynamic UI components.

---

In `_ViewStart.cshtml`:

```html
@using AlienFruit.Astra.Core
@inject AstraEngine AstraEngine
@{
    @AstraEngine.IncompleteLoadCheck()
    Layout = AstraEngine.RouteLayout(Context, "_Layout");
}
```

**What these directives do:**
- `@using AlienFruit.Astra.Core` - imports the namespace for accessing Astra classes
- `@inject AstraEngine AstraEngine` - injects an AstraEngine instance into the view for managing dynamic loading
- `@AstraEngine.IncompleteLoadCheck()` - adds JavaScript code to protect against incomplete resource loading when restoring pages from browser history
- `Layout = AstraEngine.RouteLayout(Context, "_Layout")` - dynamically determines the page layout through AstraEngine instead of static assignment

**Important:** The `@AstraEngine.IncompleteLoadCheck()` line provides protection against incomplete resource loading. This function adds JavaScript code that checks for the presence of a special meta tag after DOM loading. This is necessary in case a page is restored from history when opening the browser.

### 4. Add ViewBox to _Layout.cshtml

The `<view-box>` tag is a container for dynamic page content. It is designed to hold content that can be loaded asynchronously without a full page reload:

```html
<view-box id="main-view-box1"
    class="pb-3"
    role="main"
    on-timeout-after-start-loading-js-function="onTimeoutAfterStartLoading"
    on-finish-loading-js-function="onFinishLoading">
    @RenderBody()
</view-box>
```

**Параметры view-box:**

| Parameter | Type | Required | Description | Default Value |
|-----------|------|----------|-------------|---------------|
| `id` | `string` | ✅ Yes | Unique identifier for the view-box container. Used to link with view-box-link elements and for JavaScript interaction | - |
| `connection-error-message-resource` | `Resource` | ❌ No | Resource containing a connection error message that will be displayed when loading problems occur. Supports `EmbeddedResource` and `InMemoryResource` resource types | Built-in resource with message "Failed to connect to server. Please check your internet connection and try refreshing the page." |
| `on-start-loading-js-function` | `string` | ❌ No | Name of the JavaScript function that will be called when content loading starts | `null` |
| `on-timeout-after-start-loading-js-function` | `string` | ❌ No | Name of the JavaScript function that will be called when the timeout expires after loading starts. Can be used to display a loading indicator | `null` |
| `on-finish-loading-js-function` | `string` | ❌ No | Name of the JavaScript function that will be called after successful content loading completion. Can be used to perform additional actions after loading | `null` |
| `on-scripts-executed-js-function` | `string` | ❌ No | Name of the JavaScript function that will be called after all scripts of the loaded page have been executed | `null` |
| `start-loading-event-delay` | `int` | ❌ No | Delay in milliseconds before calling the loading start event | `100` |
| `class` | `string` | ❌ No | CSS classes for container styling | `null` |
| `style` | `string` | ❌ No | Inline CSS styles for the container | `null` |
| `role` | `string` | ❌ No | ARIA role for accessibility | `null` |
| `tag-name` | `string` | ❌ No | HTML tag name to use instead of `<main>` | `"main"` |
| `changing-browser-address-enable` | `bool` | ❌ No | Enables/disables browser URL address change during navigation | `true` |
| `parent-view-box-id` | `string` | ❌ No | ID of the parent view-box container for creating hierarchy | `null` |

**Detailed description of the connection-error-message-resource parameter:**

The `connection-error-message-resource` parameter defines a resource containing an HTML error message that will be displayed in the view-box container when connection problems occur (for example, when internet connection is lost or the server is unavailable).

**Supported resource types:**

1. **`EmbeddedResource`** - for using embedded resources from the assembly:
```csharp
connection-error-message-resource="@(new EmbeddedResource(
    name: "CustomError.html",
    path: "MyProject.Views.Shared.CustomError.html",
    assembly: typeof(MyController).Assembly))"
```

**EmbeddedResource parameters:**
- `name` - unique resource name for identification
- `path` - full path to the embedded resource in the format `Namespace.Folder.FileName.Extension` (manifest resource name)
- `assembly` - assembly containing the embedded resource (usually `Assembly.GetExecutingAssembly()` or `typeof(SomeClass).Assembly`)
---

2. **`InMemoryResource`** - for creating a resource directly in code:
```csharp
connection-error-message-resource="@(new InMemoryResource(
    "connection-error",
    "<div class='alert alert-danger'>Failed to connect to server. Please check your internet connection.</div>"))"
```
---
3. **Custom Resource implementation** - for advanced scenarios, you can create your own class inheriting from the abstract `Resource` class:

```csharp
public class DatabaseResource(string name, string query, IDbConnection connection) : Resource(name)
{
    public override Stream GetStream()
    {
        // Get HTML from database by query
        var html = GetHtmlFromDatabase(query, connection);
        var bytes = Encoding.UTF8.GetBytes(html);
        return new MemoryStream(bytes);
    }
}
```
---
**Usage recommendations:**
- Use `InMemoryResource` for simple text messages
- Use `EmbeddedResource` for complex HTML templates with styling
- HTML content should be valid and secure
- It is recommended to use CSS classes for styling error messages

**What @RenderBody() does:** This Razor method displays the content of a specific page inside the view-box container for standard display of page content using ASP.NET MVC means. For example, when first opening a page by link in the browser or during a full page reload.

**Detailed explanation of @RenderBody():**
- In a standard ASP.NET MVC application, `@RenderBody()` is used in layout files to display page content
- In the context of AlienFruit.Astra, this method is placed inside the `<view-box>` container
- When a user navigates via `<view-box-link>`, the target page content is loaded via AJAX request and displayed inside the view-box instead of `@RenderBody()`
- This allows updating only part of the page (view-box content), preserving navigation, header, footer, and other static elements
- This way, smooth SPA-like navigation is achieved without reloading the entire page

### 5. Add Dynamic Links to _Layout.cshtml (for example, for menu items)

To create a dynamic link that loads content without a full page refresh, use the `<view-box-link>` tag:

```html
<view-box-link id="home-link"
    selected-class-name="nav-link text-dark active"
    default-class-name="nav-link"
    view-box-id="main-view-box1"
    uri="/Home">
    Home
</view-box-link>
```

**view-box-link parameters:**

| Parameter | Type | Required | Description | Default Value |
|-----------|------|----------|-------------|---------------|
| `id` | `string` | ✅ Yes | Unique identifier for the view-box-link element | - |
| `uri` | `string` | ✅ Yes | URI address of the page to navigate to | - |
| `view-box-id` | `string` | ✅ Yes | ID of the view-box container where content will be loaded | - |
| `style` | `string` | ❌ No | Inline CSS styles for the element | `null` |
| `default-class-name` | `string` | ❌ No | CSS classes applied by default | `null` |
| `selected-class-name` | `string` | ❌ No | CSS classes applied when the link matches the current URL | `null` |
| `tag-name` | `string` | ❌ No | HTML tag name to use instead of `<a>` | `"a"` |
| `on-click-js-function` | `string` | ❌ No | Name of the JavaScript function to be called when the link is clicked | `null` |
| `scroll-up` | `bool` | ❌ No | Determines whether the page should scroll up after content loading | `true` |

### 6. Resource Management

**Resource Management** - is an important part of AlienFruit.Astra that provides automatic connection and management of JavaScript and CSS resources. This is necessary for the correct operation of dynamic content loading and preventing resource conflicts.

**Why is this needed:**
- **Automatic resource connection:** Astra automatically connects the necessary JS/CSS files to the HTML page header
- **Prevention of duplication:** The system tracks already connected resources and avoids their re-inclusion
- **Version management:** Resources receive version tags for correct browser caching
- **Location separation:** Resources can be placed in `<head>` (Header) or before the closing `</body>` (Body)
- **AJAX navigation support:** When dynamically loading pages, resources are connected automatically without reloading

**Как использовать:**

```html
@using AlienFruit.Astra.Core
@inject AstraEngine AstraEngine

<!DOCTYPE html>
<html>
<head>
    <!-- ... other head elements ... -->
    @AstraEngine.RenderHeaders()
</head>
<body>
    <!-- ... body content ... -->
</body>
</html>
```

**What `@AstraEngine.RenderHeaders()` does:**
- Connects the main JavaScript file `viewbox.js` for view-box component operation
- Generates HTML `<link>` tags for CSS resources
- Generates HTML `<script>` tags for JavaScript resources
- Adds a special meta tag `<meta id="load-check">` to protect against incomplete resource loading
- Considers resource versioning for correct caching

## 7. Configuration

AlienFruit.Astra provides a flexible configuration system for optimizing resource handling and performance. Most users will find the following configuration suitable:

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

**Или программно:**

```csharp
builder.AddAstra(options =>
{
    options.EnableVersioning = true;
    options.UseCompression = true;
    options.CacheMaxAge = 86400; // 24 hours
});
```

**All available configuration parameters:**

| Parameter | Type | Description | Default Value | Recommendation |
|-----------|------|-------------|---------------|---------------|
| `UseCompression` | `bool` | Enables resource compression (GZIP) to reduce the size of transmitted data | `true` | Keep `true` for production |
| `ResourcesRoute` | `string` | Route path for serving resources (CSS/JS files) | `"astra"` | Use the default value |
| `EnableVersioning` | `bool` | Enables resource versioning for correct browser caching | `false` | Set `true` for production |
| `ResourceVersion` | `string?` | Fixed resource version (if not specified, content hash is used) | `null` | Leave `null` for automatic versioning |
| `CacheMaxAge` | `int` | Resource caching time in seconds (HTTP Cache-Control max-age) | `31536000` (1 year) | `86400` (24 hours) for development, `31536000` for production |

**Parameter explanations:**
- **`EnableVersioning = true`** - prevents caching issues when updating resources
- **`UseCompression = true`** - reduces the size of transmitted data
- **`CacheMaxAge = 86400`** - balance between performance and content freshness (24 hours)

## Contributing

We welcome contributions to AlienFruit.Astra! If you have suggestions for improvements, new features, or bug fixes, please open an issue or submit a pull request on our [GitHub repository](https://github.com/alienfruit/AlienFruit.Astra).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact & Support

For questions, support, or general discussions, please visit our [GitHub Discussions](https://github.com/alienfruit/AlienFruit.Astra/discussions) or open an issue on the [issue tracker](https://github.com/alienfruit/AlienFruit.Astra/issues).