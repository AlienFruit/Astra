# ViewBox Tag

The `<view-box>` tag is a fundamental component of AlienFruit.Astra, serving as a container for dynamic page content. It is designed to hold content that can be loaded asynchronously without requiring a full page reload, providing a seamless user experience.

## Basic Usage

Wrap your main content (`@RenderBody()`) with a `<view-box>` tag in your `_Layout.cshtml`. This sets up the primary dynamic content area for your application.

```html
<view-box id="main-view-box1"
    class="pb-3"
    role="main"
    on-timeout-after-start-loading-js-function="onTimeoutAfterStartLoading"
    on-finish-loading-js-function="onFinishLoading">
    @RenderBody()
</view-box>
```

**What `@RenderBody()` does:** This Razor method displays the content of a specific page inside the view-box container for standard display of page content using ASP.NET MVC means. For example, when first opening a page by link in the browser or during a full page reload. In the context of AlienFruit.Astra, when a user navigates via `<view-box-link>`, the target page content is loaded via AJAX request and displayed inside the view-box instead of `@RenderBody()`, allowing updating only part of the page.

## ViewBox Parameters

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

## Nested ViewBoxes

AlienFruit.Astra supports hierarchical view-box structures, allowing you to create nested dynamic content areas. This enables complex UI layouts where different parts of the page can be updated independently.

**How nested view-boxes work:**
- Child view-boxes can be placed inside parent view-boxes
- Each view-box maintains its own loading state and content
- Navigation can target specific view-boxes within the hierarchy
- Parent-child relationships are established using the `parent-view-box-id` parameter

**Example of nested view-boxes:**

```html
<!-- Parent view-box in _Layout.cshtml -->
<view-box id="main-view-box1" class="pb-3" role="main">
    @RenderBody()
</view-box>
```

```html
<!-- Child view-box in a page view (like DynamicContent/Index.cshtml) -->
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

**Benefits of nested view-boxes:**
- **Modular UI:** Create complex interfaces with multiple independent content areas
- **Selective Updates:** Update specific sections without affecting others
- **Tab Interfaces:** Perfect for tabbed content, dashboards, and multi-panel layouts
- **Independent Loading:** Each view-box can show its own loading states and error messages

**Using view-box-link with nested structures:**
```html
<view-box-link id="tab1-link"
    default-class-name="btn btn-primary"
    selected-class-name="btn btn-primary active"
    view-box-id="dynamic-content-box"
    uri="/DynamicContent/Tab1">
    Tab 1
</view-box-link>
```

**Important notes:**
- The `parent-view-box-id` parameter must reference an existing view-box ID
- Child view-boxes inherit the error handling and resource management of their parent
- JavaScript functions (on-finish-loading, etc.) work independently for each view-box
- Browser address changes can be controlled per view-box using `changing-browser-address-enable`
