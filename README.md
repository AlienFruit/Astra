# AlienFruit.Astra

![AlienFruit.Astra Logo](design/logo.png)

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

1.  **Register services** in your `Program.cs`:

```csharp
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    // Register Astra services
    builder.AddAstra();
```

2.  **Add the Astra middleware** to your application's request pipeline:

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

In `_ViewStart.cshtml`:
```html
@using AlienFruit.Astra.Core
@inject AstraEngine AstraEngine
@{
    Layout = AstraEngine.RouteLayout(Context, "_Layout");
}
```

### 4. ViewBox Example

For isolated and reusable components, use the `<view-box>` tag. This is the main container that will hold your dynamic content:

```html
<view-box id="main-view-box1"
    class="pb-3"
    role="main"
    on-timeout-after-start-loading-js-function="onTimeoutAfterStartLoading"
    on-finish-loading-js-function="onFinishLoading">
    @RenderBody()
</view-box>
```

### 5. Dynamic Link Example

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

### 6. Resource Management

To include registered resources in your HTML head, use the AstraEngine in your layout:

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

## 7. Configuration

You can configure AlienFruit.Astra through `appsettings.json`:

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

## Contributing

We welcome contributions to AlienFruit.Astra! If you have suggestions for improvements, new features, or bug fixes, please open an issue or submit a pull request on our [GitHub repository](https://github.com/alienfruit/AlienFruit.Astra).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact & Support

For questions, support, or general discussions, please visit our [GitHub Discussions](https://github.com/alienfruit/AlienFruit.Astra/discussions) or open an issue on the [issue tracker](https://github.com/alienfruit/AlienFruit.Astra/issues).