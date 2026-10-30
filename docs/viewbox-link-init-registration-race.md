# Отчёт: `The resource "viewbox-link-init-{Id}" was not registered`

Дата анализа: 2026-10-30
Статус: **исправлено** — патч в `HtmlResourceRenderer`, регрессионные тесты `ResourceRegistrationRaceTests`.

## 1. Симптом

```
System.ArgumentException: The resource "viewbox-link-init-go-aitarotreader-becheating" was not registered
   at AlienFruit.Astra.Core.HtmlResourceRenderer.RenderBodyResource(String name)
   at AlienFruit.Astra.ViewBoxLink.ViewBoxLink.Process(TagHelperContext context, TagHelperOutput output)
   ... Features_TarotDivinations_Views_Spread.cshtml:line 484
```

## 2. Источник исключения

Сообщение бросает единственное место — `HtmlResourceRenderer.GetResourceContent`:

```csharp
if (resourceStorage.Contains(resourceName) == false)
    throw new ArgumentException($"The resource \"{resourceName}\" was not registered");
```

Путь вызова: `ViewBoxLink.Process` для одного и того же имени `viewbox-link-init-{Id}` сначала вызывает
`AddJsCodeFromTemplate(..., AstraResourceLocation.Body)`, сразу следом `RenderBodyResource(...)`.
Имя без `.js` — значит ресурс регистрировал tag helper, а не `AstraEngine.AddViewBoxLink`
(тот использует `viewbox-link-init-{Id}.js`, location=Header).

## 3. Корневая причина

Состояние живёт в **двух независимых** `ConcurrentDictionary` синглтонов
(`IHtmlResourceRenderer` и `IResourceStorage` зарегистрированы как `AddSingleton`, `AstraRegistration.cs`):

- `resourceScripts` — реестр имён + location (Header/Body);
- `ResourceStorage.resources` — фактическое содержимое.

Инвариант «имя видимо в реестре ⇒ контент есть в хранилище» не соблюдался: все четыре метода
`Add*` использовали `TryAdd` в реестре **как guard**, и только внутри этой ветки писали в хранилище:

```csharp
if (resourceScripts.TryAdd(resourceName, resourceLocation))   // ← метка ставится ПЕРВОЙ
{
    ... GetManifestResourceStream / Template.Parse / Render ... // ← окно гонки (мс)
    resourceStorage.RegisterResource(resourceName, scriptContent); // ← контент ПОСЛЕДНИМ
}
```

Отсюда два сбоя:

### Сценарий A — гонка двух concurrent-запросов на один `Id`

Первый рендер ссылки после старта процесса. Поток B видит `TryAdd = false`, пропускает
запись в хранилище — но сразу идёт в `RenderBodyResource`, который по реестру считает
ресурс Body-зарегистрированным, а хранилище ещё пустое → исключение.

Типичный производитель: страница `Spread.cshtml` рендерит много `view-box-link`; параллельные
запросы (обычный + AJAX-паршалка того же блока, двойной клик, несколько пользователей) —
после рестарта/deploy счётчик пуст и проигравший падает.

### Сценарий B — отравление ключа (sticky)

Любое исключение после `TryAdd` (отсутствующий/битый embedded-шаблон, ошибка Scriban, IO) —
метка в реестре остаётся навсегда, контента в хранилище нет. Все последующие рендеры этого
`Id` падают с тем же сообщением **до перезапуска процесса**.

## 4. Воспроизведение (до патча)

Временный проект со связкой реальных `ResourceStorage` + `HtmlResourceRenderer`, имя
`viewbox-link-init-go-aitarotreader-becheating`, `Parallel.For(0, 400)`:

```
[concurrency] errors=399
   The resource "viewbox-link-init-go-aitarotreader-becheating" was not registered
[poison] first=There is no resource with path: ...Missing.template.js
[poison] second=The resource "viewbox-link-init-go-aitarotreader-becheating" was not registered
```

## 5. Патч

Файл: `src/AlienFruit.Astra/Core/HtmlResourceRenderer.cs` (все четыре метода:
`AddStylesheetResource`, `AddScriptResource`, `AddJsCode`, `AddJsCodeFromTemplate`).

Инвариант: **контент обязан попасть в хранилище раньше, чем имя станет видимым в реестре**.
Guard через `ContainsKey`, публикация имени — последней операцией:

```csharp
if (resourceScripts.ContainsKey(resourceName))
{
    return;
}

// ...формирование scriptContent...

resourceStorage.RegisterResource(resourceName, scriptContent); // сначала контент
CalculateResourceHash(resourceName, scriptContent);
resourceScripts.TryAdd(resourceName, resourceLocation);        // потом метка
```

Последствия:

- проигравший в гонке либо видит полный хранилище-key (`ContainsKey` → ранний выход), либо
  заходит в ветку и пересоздаёт тот же контент (`ResourceStorage.RegisterResource` — идемпотентный
  `TryAdd`) — падения невозможны;
- исключение в шаблоне больше не оставляет метку → повторная регистрация на следующем запросе
  работает (сценарий B устранён);
- двойная запись контента при гонке безвредна: `RegisterResource` идемпотентен по имени.

## 6. Регрессионные тесты

Файл: `src/AlienFruit.Astra.Tests/UnitTests/ResourceRegistrationRaceTests.cs` — реальные
`ResourceStorage`/`HtmlResourceRenderer` (без моков), 32 потока с `Barrier` (детерминированный
старт в одну точку, без таймингов):

| Тест | Проверяет |
|---|---|
| `AddJsCodeFromTemplateAndRenderBodyResource_ConcurrentSameResourceName_ShouldNeverThrow` | сценарий A на уровне renderer (последовательность из `ViewBoxLink.Process`) |
| `ViewBoxLinkProcess_ConcurrentSameId_ShouldNeverThrow` | сценарий A end-to-end через tag helper — ровно стек из отчёта об ошибке |
| `AddJsCodeFromTemplate_FailedRegistration_ShouldNotPoisonResourceName` | сценарий B: после неудачной регистрации имя не отравлено |
| `AddJsCode_ConcurrentSameResourceName_StorageAndRegistryStayConsistent` | то же для перегрузки без шаблона |

Доказательство red/green:

```
# до патча
Failed! - Failed: 4, Passed: 0, Total: 4
  ArgumentException: The resource "viewbox-link-init-go-aitarotreader-becheating" was not registered

# после патча (10 прогонов подряд)
Passed! - Failed: 0, Passed: 4 × 10

# полный набор
Passed! - Failed: 0, Passed: 129, Total: 129
```

## 7. Попутно исправлено

`src/AlienFruit.Astra.Tests/AlienFruit.Astra.Tests.csproj`: вложенный консольный проект
`TestTagHelper/` — его `obj/**/*AssemblyAttributes.cs` подхватывался компиляционным glob'ом
тестового проекта и ломал сборку (`CS0579: Duplicate 'TargetFrameworkAttribute'`).
Добавлено исключение `TestTagHelper\**` для `Compile/EmbeddedResource/None`. Без этого ни один
тест не запускался.

## 8. Известные смежные проблемы (вне объёма патча)

- `ResourceStorage` — синглтон, словарь никогда не очищается: первый отрендеренный спецификатор
  имени выигрывает навсегда; при пересечении `Id` между представлениями второй получает init-скрипт
  первого + неограниченный рост памяти.
- `ViewBox` (`viewbox-init-{Id}.js`) и `ViewBoxLinkGroup` (`viewbox-link-group-init--{Id}`)
  используют ту же последовательность add-then-render через исправленный renderer — гонка устранена
  и там; но двойное подчёркивание `--` в имени группы — отдельная несогласованность.
- `AstraEngine.AddViewBoxLink` регистрирует `viewbox-link-init-{Id}.js` (Header), tag helper —
  `viewbox-link-init-{Id}` (Body): вызов обоих даёт два init-скрипта на ссылку.

## 9. Изменённые файлы

| Файл | Изменение |
|---|---|
| `src/AlienFruit.Astra/Core/HtmlResourceRenderer.cs` | патч: public-after-storage во всех `Add*` |
| `src/AlienFruit.Astra.Tests/UnitTests/ResourceRegistrationRaceTests.cs` | +4 регрессионных теста |
| `src/AlienFruit.Astra.Tests/AlienFruit.Astra.Tests.csproj` | фикс CS0579 (исключение `TestTagHelper\**`) |
