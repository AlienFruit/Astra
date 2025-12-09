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

1.  **Register services** in your `Program.cs`:

```csharp
    var builder = WebApplication.CreateBuilder(args);
    // ... service registrations ...

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

**Что делает эта директива:** Регистрирует все Tag Helpers из сборки AlienFruit.Astra, что позволяет использовать специальные HTML-теги `<view-box>` и `<view-box-link>` в ваших Razor представлениях для создания динамических компонентов интерфейса.

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

**Что делают эти директивы:**
- `@using AlienFruit.Astra.Core` - импортирует пространство имен для доступа к классам Astra
- `@inject AstraEngine AstraEngine` - внедряет экземпляр AstraEngine в представление для управления динамической загрузкой
- `@AstraEngine.IncompleteLoadCheck()` - добавляет JavaScript код для защиты от неполной загрузки ресурсов при восстановлении страницы из истории браузера
- `Layout = AstraEngine.RouteLayout(Context, "_Layout")` - динамически определяет layout страницы через AstraEngine вместо статического присваивания

**Важно:** Строка `@AstraEngine.IncompleteLoadCheck()` обеспечивает механизм защиты от неполной загрузки ресурсов. Эта функция добавляет JavaScript код, который проверяет наличие специального мета-тега после загрузки DOM. Это необходимо на тот случай, когда страница восстанавливается из истории при открыти браузера.

### 4. ViewBox Example

Тег `<view-box>` представляет собой контейнер для динамического контента страницы. Он предназначен для размещения содержимого, которое может загружаться асинхронно без полной перезагрузки страницы:

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

| Параметр | Тип | Обязательный | Описание | Значение по умолчанию |
|-----------|-----|-------------|----------|---------------------|
| `id` | `string` | ✅ Да | Уникальный идентификатор view-box контейнера. Используется для связи с view-box-link элементами и для JavaScript взаимодействия | - |
| `connection-error-message-resource` | `Resource` | ❌ Нет | Ресурс, содержащий сообщение об ошибке соединения, которое будет отображаться при проблемах с загрузкой. Поддерживаются `EmbeddedResource` и `InMemoryResource` типы ресурсов | Встроенный ресурс с сообщением "Failed to connect to server. Please check your internet connection and try refreshing the page." |
| `on-start-loading-js-function` | `string` | ❌ Нет | Имя JavaScript функции, которая будет вызвана при начале загрузки контента | `null` |
| `on-timeout-after-start-loading-js-function` | `string` | ❌ Нет | Имя JavaScript функции, которая будет вызвана при истечении времени ожидания после начала загрузки. Можно использовать для отображения индикатора загрузки | `null` |
| `on-finish-loading-js-function` | `string` | ❌ Нет | Имя JavaScript функции, которая будет вызвана после успешного завершения загрузки контента. Можно использовать для выполнения дополнительных действий после загрузки | `null` |
| `on-scripts-executed-js-function` | `string` | ❌ Нет | Имя JavaScript функции, которая будет вызвана после выполнения всех скриптов загруженной страницы | `null` |
| `start-loading-event-delay` | `int` | ❌ Нет | Задержка в миллисекундах перед вызовом события начала загрузки | `100` |
| `class` | `string` | ❌ Нет | CSS классы для стилизации контейнера | `null` |
| `style` | `string` | ❌ Нет | Inline CSS стили для контейнера | `null` |
| `role` | `string` | ❌ Нет | ARIA роль для обеспечения доступности (accessibility) | `null` |
| `tag-name` | `string` | ❌ Нет | Имя HTML тега, который будет использоваться вместо `<main>` | `"main"` |
| `changing-browser-address-enable` | `bool` | ❌ Нет | Включает/отключает изменение URL адреса браузера при навигации | `true` |
| `parent-view-box-id` | `string` | ❌ Нет | ID родительского view-box контейнера для создания иерархии | `null` |

**Подробное описание connection-error-message-resource:**

Параметр `connection-error-message-resource` определяет ресурс, содержащий HTML-сообщение об ошибке, которое будет отображаться в view-box контейнере при проблемах с подключением к серверу (например, при потере интернет-соединения или недоступности сервера).

**Поддерживаемые типы ресурсов:**

1. **`EmbeddedResource`** - для использования встроенных ресурсов из сборки:
```csharp
connection-error-message-resource="@(new EmbeddedResource(
    name: "CustomError.html",
    path: "MyProject.Views.Shared.CustomError.html",
    assembly: typeof(MyController).Assembly))"
```

**Параметры EmbeddedResource:**
- `name` - уникальное имя ресурса для идентификации
- `path` - полный путь к встроенному ресурсу в формате `Namespace.Folder.FileName.Extension` (manifest resource name)
- `assembly` - сборка, содержащая встроенный ресурс (обычно `Assembly.GetExecutingAssembly()` или `typeof(SomeClass).Assembly`)

2. **`InMemoryResource`** - для создания ресурса непосредственно в коде:
```csharp
connection-error-message-resource="@(new InMemoryResource(
    "connection-error",
    "<div class='alert alert-danger'>Не удалось подключиться к серверу. Проверьте подключение к интернету.</div>"))"
```

3. **Собственная реализация Resource** - для продвинутых сценариев можно создать собственный класс, наследующий абстрактный класс `Resource`:

```csharp
public class DatabaseResource(string name, string query, IDbConnection connection) : Resource(name)
{
    public override Stream GetStream()
    {
        // Получить HTML из базы данных по query
        var html = GetHtmlFromDatabase(query, connection);
        var bytes = Encoding.UTF8.GetBytes(html);
        return new MemoryStream(bytes);
    }
}
```

**Рекомендации по использованию:**
- Используйте `InMemoryResource` для простых текстовых сообщений
- Используйте `EmbeddedResource` для сложных HTML-шаблонов с стилизацией
- HTML-содержимое должно быть валидным и безопасным
- Рекомендуется использовать CSS-классы для стилизации сообщений об ошибках

**Что делает @RenderBody():** Этот метод Razor отображает содержимое конкретной страницы внутри view-box контейнера для стандартного отображения содержимого страницы средствами ASP.NET MVC. Например, при первом открытии страницы по ссылке в браузере или при полной перезагрузке страницы.

**Подробное объяснение @RenderBody():**
- В стандартном ASP.NET MVC приложении `@RenderBody()` используется в layout файлах для отображения содержимого страниц
- В контексте AlienFruit.Astra этот метод размещается внутри `<view-box>` контейнера
- Когда пользователь переходит по `<view-box-link>`, содержимое целевой страницы загружается AJAX-запросом и отображается внутри view-box вместо `@RenderBody()`
- Это позволяет обновлять только часть страницы (содержимое view-box), сохраняя навигацию, header, footer и другие статические элементы
- Таким образом достигается плавная SPA-подобная навигация без перезагрузки всей страницы

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

**Параметры view-box-link:**

| Параметр | Тип | Обязательный | Описание | Значение по умолчанию |
|-----------|-----|-------------|----------|---------------------|
| `id` | `string` | ✅ Да | Уникальный идентификатор view-box-link элемента | - |
| `uri` | `string` | ✅ Да | URI адрес страницы, на которую будет осуществлена навигация | - |
| `view-box-id` | `string` | ✅ Да | ID view-box контейнера, в который будет загружен контент | - |
| `style` | `string` | ❌ Нет | Inline CSS стили для элемента | `null` |
| `default-class-name` | `string` | ❌ Нет | CSS классы, применяемые по умолчанию | `null` |
| `selected-class-name` | `string` | ❌ Нет | CSS классы, применяемые когда ссылка соответствует текущему URL | `null` |
| `tag-name` | `string` | ❌ Нет | Имя HTML тега, который будет использоваться вместо `<a>` | `"a"` |
| `on-click-js-function` | `string` | ❌ Нет | Имя JavaScript функции, которая будет вызвана при клике на ссылку | `null` |
| `scroll-up` | `bool` | ❌ Нет | Определяет, должна ли страница прокручиваться вверх после загрузки контента | `true` |

### 6. Resource Management

**Управление ресурсами** - это важная часть AlienFruit.Astra, которая обеспечивает автоматическое подключение и управление JavaScript и CSS ресурсами. Это необходимо для корректной работы динамической загрузки контента и предотвращения конфликтов ресурсов.

**Для чего это нужно:**
- **Автоматическое подключение ресурсов:** Astra автоматически подключает необходимые JS/CSS файлы в заголовок HTML страницы
- **Предотвращение дублирования:** Система отслеживает уже подключенные ресурсы и избегает их повторного включения
- **Управление версиями:** Ресурсы получают версионные метки для корректного кэширования браузером
- **Разделение по расположению:** Ресурсы могут быть размещены в `<head>` (Header) или перед закрывающим `</body>` (Body)
- **Поддержка AJAX навигации:** При динамической загрузке страниц ресурсы подключаются автоматически без перезагрузки

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

**Что делает `@AstraEngine.RenderHeaders()`:**
- Подключает основной JavaScript файл `viewbox.js` для работы view-box компонентов
- Генерирует HTML теги `<link>` для CSS ресурсов
- Генерирует HTML теги `<script>` для JavaScript ресурсов
- Добавляет специальный мета-тег `<meta id="load-check">` для защиты от неполной загрузки ресурсов
- Учитывает версионирование ресурсов для корректного кэширования

## 7. Configuration

AlienFruit.Astra предоставляет гибкую систему конфигурации для оптимизации работы с ресурсами и производительности. Большинству пользователей подойдет следующая конфигурация:

**Рекомендуемая конфигурация для большинства проектов:**

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
    options.CacheMaxAge = 86400; // 24 часа
});
```

**Все доступные параметры конфигурации:**

| Параметр | Тип | Описание | Значение по умолчанию | Рекомендация |
|-----------|-----|----------|---------------------|-------------|
| `UseCompression` | `bool` | Включает сжатие ресурсов (GZIP) для уменьшения размера передаваемых данных | `true` | Оставить `true` для production |
| `ResourcesRoute` | `string` | Путь маршрута для обслуживания ресурсов (CSS/JS файлы) | `"astra"` | Использовать значение по умолчанию |
| `EnableVersioning` | `bool` | Включает версионирование ресурсов для корректного кэширования браузером | `false` | Установить `true` для production |
| `ResourceVersion` | `string?` | Фиксированная версия ресурсов (если не указана, используется хэш контента) | `null` | Оставить `null` для автоматического версионирования |
| `CacheMaxAge` | `int` | Время кэширования ресурсов в секундах (HTTP Cache-Control max-age) | `31536000` (1 год) | `86400` (24 часа) для development, `31536000` для production |

**Пояснения к параметрам:**
- **`EnableVersioning = true`** - предотвращает проблемы кэширования при обновлении ресурсов
- **`UseCompression = true`** - уменьшает размер передаваемых данных
- **`CacheMaxAge = 86400`** - баланс между производительностью и свежестью контента (24 часа)

## Contributing

We welcome contributions to AlienFruit.Astra! If you have suggestions for improvements, new features, or bug fixes, please open an issue or submit a pull request on our [GitHub repository](https://github.com/alienfruit/AlienFruit.Astra).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact & Support

For questions, support, or general discussions, please visit our [GitHub Discussions](https://github.com/alienfruit/AlienFruit.Astra/discussions) or open an issue on the [issue tracker](https://github.com/alienfruit/AlienFruit.Astra/issues).