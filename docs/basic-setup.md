# Basic Setup

This section details the essential steps to integrate AlienFruit.Astra into your ASP.NET application.

## 1. Register Services (Program.cs)

To enable AlienFruit.Astra's functionality, you need to register its services in your application's `Program.cs` file. This makes Astra's components available for dependency injection throughout your application.

```csharp
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    // ... other service registrations ...
    builder.Services.AddAstra(); // Register Astra services

    var app = builder.Build();
```

## 2. Add Middleware (Program.cs)

After building your application, add the Astra middleware to the request pipeline in `Program.cs`. This middleware intercepts requests and enables dynamic content loading capabilities.

```csharp
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // ... other middleware ...
    app.UseAstra(); // Add Astra middleware to the request pipeline

    app.Run();
```

## 3. Configure View Files

To utilize Astra's Tag Helpers and engine features within your Razor views, specific directives need to be added to your `_ViewImports.cshtml` and `_ViewStart.cshtml` files.

### _ViewImports.cshtml

Add the following directive to `_ViewImports.cshtml` to register all Tag Helpers from the `AlienFruit.Astra` assembly. This allows you to use special HTML tags like `<view-box>` and `<view-box-link>` in your Razor views.

```html
@addTagHelper *, AlienFruit.Astra
```

### _ViewStart.cshtml

Modify `_ViewStart.cshtml` to inject `AstraEngine` and handle layout routing and resource loading checks. This ensures proper functionality for dynamic content and browser history management.

```html
@using AlienFruit.Astra.Core
@inject AstraEngine AstraEngine
@{
    @AstraEngine.IncompleteLoadCheck()
    Layout = AstraEngine.RouteLayout(Context, "_Layout");
}
```

**What these directives do:**
- `@using AlienFruit.Astra.Core` - imports the namespace for accessing Astra classes.
- `@inject AstraEngine AstraEngine` - injects an AstraEngine instance into the view for managing dynamic loading.
- `@AstraEngine.IncompleteLoadCheck()` - adds JavaScript code to protect against incomplete resource loading when restoring pages from browser history.
- `Layout = AstraEngine.RouteLayout(Context, "_Layout")` - dynamically determines the page layout through AstraEngine instead of static assignment.

**Important:** The `@AstraEngine.IncompleteLoadCheck()` line provides protection against incomplete resource loading. This function adds JavaScript code that checks for the presence of a special meta tag after DOM loading. This is necessary in case a page is restored from history when opening the browser.
