# AlienFruit.Astra

<img src="design/logo.png" alt="AlienFruit.Astra Logo" height="64">

[![NuGet version](https://badge.fury.io/nu/AlienFruit.Astra.svg)](https://badge.fury.io/nu/AlienFruit.Astra)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/alienfruit/AlienFruit.Astra/workflows/CI/badge.svg)](https://github.com/alienfruit/AlienFruit.Astra/actions)

AlienFruit.Astra - это мощная библиотека .NET, предназначенная для улучшения ASP.NET приложений (MVC и Razor Pages) путем включения динамической загрузки контента и обеспечения бесшовного пользовательского опыта с плавной навигацией между страницами без полной перезагрузки.

## Почему стоит выбрать AlienFruit.Astra?

Хотя современные фреймворки, такие как Blazor, предлагают полноценные возможности на стороне клиента, AlienFruit.Astra предоставляет уникальную, легковесную альтернативу для разработчиков, которые хотят:

- **Улучшить существующие ASP.NET приложения**: Бесшовно интегрировать плавную AJAX-навигацию в традиционные серверные MVC или Razor Pages приложения с минимальными изменениями кода.
- **Избежать сложности SPA**: Достичь современного, отзывчивого пользовательского опыта без накладных расходов и кривой обучения полноценного фреймворка Single Page Application (SPA).
- **Сохранить архитектуру**: Сохранить существующие контроллеры, представления и бизнес-логику в целости, позволяя постепенно улучшать пользовательский опыт.

**Что вы получите:**
- ⚡ **Более быстрая загрузка страниц**: Устраните полную перезагрузку страниц для более быстрых и плавных взаимодействий с пользователем.
- 🎯 **Улучшенный пользовательский опыт**: Обеспечьте плавные переходы, динамические обновления контента и лучшую отзывчивость на всех устройствах.
- 🏗️ **Сохранение архитектуры**: Продолжайте использовать знакомые паттерны и инфраструктуру ASP.NET.
- 📱 **Мобильная ориентированность**: Улучшите мобильный пользовательский опыт за счет эффективной загрузки контента.
- 🔧 **Простое обслуживание**: Выгодите от упрощенной интеграции и текущего управления.

## Возможности

*   **Динамическая загрузка контента**: Загружайте частичные представления или блоки контента динамически без перезагрузки всей страницы, оптимизируя производительность и воспринимаемую скорость.
*   **AJAX-навигация**: Включите плавную, асинхронную навигацию между страницами, улучшая пользовательский опыт и снижая нагрузку на сервер.
*   **Вложенные ViewBoxes**: Создавайте сложные, иерархические структуры пользовательского интерфейса с несколькими независимыми областями динамического контента для модульных и гибких макетов.
*   **JavaScript API**: Получите программный контроль над view-boxes со стороны клиентского кода, позволяя создавать расширенные взаимодействия и пользовательское поведение.
*   **Управление ресурсами**: Автоматически управляйте ресурсами JavaScript и CSS, предотвращая дубликаты, обеспечивая правильный порядок загрузки и оптимизируя кэширование.
*   **Интеграция ViewBox**: Облегчите интеграцию изолированных и переиспользуемых компонентов пользовательского интерфейса в области динамического контента.
*   **Высокая настраиваемость**: Настройте библиотеку в соответствии с конкретными требованиями и паттернами дизайна вашего приложения.

## Быстрый старт

Запустите AlienFruit.Astra в вашем ASP.NET MVC или Razor Pages приложении с помощью этих минимальных шагов:

### 1. Установка

```bash
Install-Package AlienFruit.Astra
# Или используя .NET CLI
dotnet add package AlienFruit.Astra
```

### 2. Регистрация служб (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAstra(); // Регистрация служб Astra
```

### 3. Добавление middleware (Program.cs)

```csharp
var app = builder.Build();
app.UseAstra(); // Добавление middleware Astra в конвейер запросов
```

### 4. Настройка файлов представлений (_ViewImports.cshtml & _ViewStart.cshtml)

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

### 5. Добавление ViewBox в _Layout.cshtml

Оберните ваш основной контент (`@RenderBody()`) тегом `<view-box>`:

```html
<view-box id="main-view-box" class="pb-3" role="main">
    @RenderBody()
</view-box>
```

### 6. Добавление динамических ссылок

Используйте `<view-box-link>` для AJAX-навигации:

```html
<view-box-link view-box-id="main-view-box" uri="/Home">
    Home
</view-box-link>
```

### 7. Отображение заголовков Astra (_Layout.cshtml)

Включите `@AstraEngine.RenderHeaders()` в ваш `<head>` раздел:

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

## Демонстрация

Посмотрите, как AlienFruit.Astra преобразует традиционное ASP.NET приложение в современное, отзывчивое приложение с плавной навигацией:

[![Демонстрация AlienFruit.Astra](https://img.youtube.com/vi/your-video-id/0.jpg)](https://www.youtube.com/watch?v=your-video-id)

## Расширенное использование

Изучите все возможности AlienFruit.Astra, включая вложенные view-boxes, JavaScript API, управление ресурсами и подробные опции конфигурации.

### Вложенные ViewBoxes

AlienFruit.Astra поддерживает иерархические структуры view-box, позволяя создавать вложенные области динамического контента. Это позволяет создавать сложные макеты пользовательского интерфейса, где разные части страницы могут обновляться независимо.

```html
<!-- Родительский view-box в _Layout.cshtml -->
<view-box id="main-view-box1" class="pb-3" role="main">
    @RenderBody()
</view-box>
```

```html
<!-- Дочерний view-box в представлении страницы (например, DynamicContent/Index.cshtml) -->
<view-box id="dynamic-content-box"
    parent-view-box-id="main-view-box1"
    class="pb-3"
    role="main"
    changing-browser-address-enable="false">
    <div class="alert alert-info">
        <h4>Выберите вкладку для загрузки контента</h4>
    </div>
</view-box>
```

### JavaScript API

AlienFruit.Astra предоставляет JavaScript API для программного управления view-boxes:

```javascript
// Загрузка контента в основной view-box
ViewBoxRegistry.sendRequest('main-view-box1', '/Home/About');

// Динамическая загрузка контента вкладки
ViewBoxRegistry.sendRequest('dynamic-content-box', '/DynamicContent/Tab1');

// Отмена текущего запроса
ViewBoxRegistry.abortCurrentRequest('main-view-box1');
```

### Группы ссылок ViewBox

Создавайте группы навигационных элементов, которые разделяют общее визуальное состояние:

```html
<view-box-link-group id="main-nav-group"
    view-box-id="main-view-box1"
    default-class-name="nav-item"
    selected-class-name="nav-item active"
    uri-to-activete="@(["/Home", "/About", "/Contact"])">
    <!-- Ссылки здесь -->
</view-box-link-group>
```

## Конфигурация

AlienFruit.Astra предоставляет гибкую систему конфигурации для оптимизации обработки ресурсов и производительности:

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

Или программно:

```csharp
builder.AddAstra(options =>
{
    options.EnableVersioning = true;
    options.UseCompression = true;
    options.CacheMaxAge = 86400; // 24 часа
});
```

## Сравнение с другими решениями

| Характеристика | AlienFruit.Astra | Blazor Server | Blazor WebAssembly | Полноценный SPA (React/Angular) |
|----------------|------------------|---------------|-------------------|---------------------------------|
| **Требуемые изменения кода** | Минимальные | Значительные | Значительные | Полная переработка |
| **Кривая обучения** | Низкая | Средняя | Высокая | Высокая |
| **Размер приложения** | Маленький | Средний | Большой | Большой |
| **SEO-дружественность** | ✅ Отличная | ✅ Отличная | ⚠️ Требует настройки | ⚠️ Требует настройки |
| **Производительность** | ✅ Быстрая загрузка | ⚠️ Зависит от соединения | ✅ Быстрая после загрузки | ✅ Быстрая после загрузки |
| **Поддержка мобильных** | ✅ Отличная | ✅ Отличная | ✅ Отличная | ✅ Отличная |

## Примеры использования

- **Корпоративные приложения**: Постепенное улучшение существующих MVC приложений без полной переработки.
- **Панели управления**: Создание динамических панелей с независимыми областями контента.
- **Системы управления контентом**: Улучшение навигации и пользовательского опыта без изменения архитектуры.
- **E-commerce платформы**: Ускорение навигации по каталогу и процессу оформления заказа.

## Сообщество и поддержка

- 📖 [Документация](https://github.com/alienfruit/AlienFruit.Astra/wiki)
- 🐛 [Сообщить о проблеме](https://github.com/alienfruit/AlienFruit.Astra/issues)
- 💬 [Обсуждения](https://github.com/alienfruit/AlienFruit.Astra/discussions)
- 📧 [Связаться с нами](mailto:support@alienfruit.dev)

## Вклад

Мы приветствуем вклад в AlienFruit.Astra! Если у вас есть предложения по улучшению, новые функции или исправления ошибок, пожалуйста, откройте issue или отправьте pull request в нашем [репозитории GitHub](https://github.com/alienfruit/AlienFruit.Astra).

## Лицензия

Этот проект лицензирован под лицензией MIT - см. файл [LICENSE](LICENSE) для деталей.