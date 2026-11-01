# План: исправления реестра ресурсов и компрессии

Дата: 2026-11-01. Основан на разборе HEAD `383b810` + рабочим деревом (logger удалён).
Объём: три пункта решения пользователя (валидация `AddResource`, вывод имени из path + хранение Path,
компрессия инлайн-скриптов) + один побочный баг, найденный при проверке.

**Вне объёма: тесты и документация** (тесты пользователь приводит сам; доки — этап 4 основного плана).

---

## Диагностика: три дефекта с одним корнем

Корень: `ResourceEntry.Version` сейчас — «хеш байт, которые вернул storage», то есть **зависит от
компрессора**, при этом сравнивается при дублях с «хешем контента в момент регистрации». Отсюда:

### D1. Тавтологичная проверка в `AddResource` (пункт 1)
`HtmlResourceRenderer.cs:37-38`:

```csharp
ThrowIfContentDiffers(name, existing, () => ComputeVersionFromStorage(name));
```

`ComputeVersionFromStorage(name)` читает из storage запись под `name` — а там **контент первой
регистрации** (новая `path` никогда не читается: `resourceStorage.RegisterResource` ниже по течению,
до него early-return; сам storage first-write-wins `TryAdd`). Итого сравниваются хеши одного и того же
старого контента — проверка не может сработать никогда.

Доказано пробой на рабочем дереве: `AddResource("dup.js", load-check.js, Header)` → `AddResource("dup.js",
viewbox.js /* другой контент */, Body)` → без исключения → `RenderBodyResource` → «resource dup.js was not
registered as body resource» — исходный баг жив на embedded-пути.

Для `AddJsCodeFromTemplate` (`:110-112`) та же функция **работает**: `versionFunc` рендерит новый шаблон.

### D2. Хеш сжатого vs хеш сырого (найден при проверке)
`AddJsCodeFromTemplate`: первый add → `Version = ComputeVersionFromStorage(...)` = SHA256 **сжатых**
Nuglify байт. Дубль → сравнение с SHA256 **сырого** скрибан-рендера. При `UseCompression=true`
(дефолт) идентичная повторная регистрация бросает:

```
C1 identical template re-add (compression ON): THROWS InvalidOperationException:
   Resource 's.js' version mismatch. Expected 5c7f277b…, got …
C2 identical template re-add (compression OFF): no throw
```

### D3. Лишний I/O + CPU при регистрации (пункт 2)
Версия считается всегда, даже при `EnableVersioning=false` (проба: 1 `OpenRead`), и для Body — тоже.
Хуже того: round-trip через storage вызывает **Uglify на старте** для каждого Header-ресурса
(`OpenRead` → `CompressToStream`), хотя хеш нужен по сырому контенту.

---

## Решение

### Этап A — `AddResource`: имя выводится из path, проверка сравнивает источник

**A1. Вывод имени.** Имя = имя файла из embedded-пути. Оба существующих callsite движка уже так и
устроены (`"AlienFruit.Astra.Core.Resources.load-check.js"` → `"load-check.js"`), т.е. параметр `name`
несёт 0 информации сверх `path`:

```csharp
internal static string DeriveResourceName(string path)
{
    // manifest-путь: "Namespace.Folder.file.js" → "file.js".
    // Ограничение: имя файла с точками ("file.min.js") выведется как "min.js" —
    // для таких случаев есть перегрузка с явным именем (A2).
    var lastDot = path.LastIndexOf('.');
    var prevDot = path.LastIndexOf('.', lastDot - 1, lastDot - 1);
    return prevDot < 0 ? path : path.Substring(prevDot + 1);
}
```

**A2. Публичная сигнатура** (`IHtmlResourceRenderer` + реализация). Breaking, но библиотека и так на
мажоре 2.0.0:

```csharp
void AddResource(string path, AstraResourceLocation resourceLocation, Assembly? assembly = null);
// перегрузка-люк для коллизии имён файлов (см. риски) и имён с точками:
void AddResource(string path, string resourceName, AstraResourceLocation resourceLocation, Assembly? assembly = null);
```

**A3. Entry хранит источник.** `ResourceEntry` — без иерархии `Source` (решение принято ранее), просто
два nullable-поля для embedded-ресурсов:

```csharp
internal sealed record ResourceEntry(
    string Name,
    AstraResourceKind Kind,
    AstraResourceLocation Location,
    string Version,
    string? EmbeddedPath = null,
    Assembly? EmbeddedAssembly = null);
```

Template-ресурсы остаются с явным именем (контент генерируется, выводить имя неоткуда) — у них
`EmbeddedPath == null`, и дубли проверяются по хешу (этап B).

**A4. Логика дубля в `AddResource`** — сравнение источника, нулевой I/O, заодно чинит D1 и исходный
класс бага (loкация больше не может разойтись молча):

```csharp
public void AddResource(string path, AstraResourceLocation resourceLocation, Assembly? assembly = null)
    => AddResource(path, DeriveResourceName(path), resourceLocation, assembly);

public void AddResource(string path, string resourceName, AstraResourceLocation resourceLocation, Assembly? assembly = null)
{
    if (entries.TryGetValue(resourceName, out var existing))
    {
        // Идемпотентный no-op: тот же источник, та же локация.
        if (existing.EmbeddedPath == path && existing.EmbeddedAssembly == (assembly ?? Assembly.GetExecutingAssembly())
            && existing.Location == resourceLocation)
            return;

        throw new InvalidOperationException(
            $"Resource '{resourceName}' is already registered from '{existing.EmbeddedPath}' " +
            $"({existing.Location}); cannot re-register from '{path}' ({resourceLocation}). " +
            "Use AddResource(path, resourceName, ...) to disambiguate.");
    }
    // …валидация расширения, чтение raw (этап B), RegisterResource в storage, TryAdd entry — как сейчас,
    //    но version считается из raw-потока assembly, не через storage.
}
```

Убрать закомментированный блок старого `EnsureSameRegistration` (`HtmlResourceRenderer.cs:96-103`) — его
роль теперь выполняет код выше, он строже (включая контент-путь) и без мёртвых веток.

**A5. Callsite'ы** (`AstraEngine.cs:29-30`):

```csharp
htmlResourceRenderer.AddResource(IncompleteLoadCheckScriptPath, AstraResourceLocation.Body);
htmlResourceRenderer.AddResource(ViewBoxSpecification.JsResourcePath, AstraResourceLocation.Header);
```

---

### Этап B — `Version` = SHA256 **сырого** контента, всегда (чинит D1·D2·D3)

Инвариант: `entry.Version` определяется только контентом ресурса и конфигурацией пользователя,
но не компрессором. Тогда сравнение дубля честно в оба конца.

1. **Embedded**: читать `assembly.GetManifestResourceStream(path)` напрямую, SHA256 по сырым байтам.
   Побочная польза: **отсутствие existence-валидации сейчас** — вчерашняя `ComputeVersionFromStorage`
   глотала ошибку в `catch`; теперь отсутствующий embedded-файл падает сразу при регистрации
   (fail-fast на старте вместо 500 в эндпоинте). Исключение — `ArgumentException`
   `$"Embedded resource '{path}' not found in assembly '{assembly.GetName().Name}'"`.
2. **Template**: `Version = ComputeVersion(scriptContent)` — SHA256 по строке скрибан-рендера (одна
   хеш-функция для обоих путей). Дубль сравнивает свежий рендер с этим же значением → D2 устранён:
   идентичный add — no-op при любой конфигурации компрессии; изменённый контент — по-прежнему `throw`.
3. **Storage остаётся хранилищем сырого** (как и был: `RegisterResource(name, content)` кладёт
   `InMemoryResource`, сжатие происходит в `OpenRead` при отдаче) — **менять `ResourceStorage` не нужно**.
4. `ComputeVersionFromStorage` удаляется целиком; `GetResourceVersion` не трогать.

Стоимость на регистрацию: один поток + SHA256 (было: поток + Uglify + SHA256 при `UseCompression=true`).
При `EnableVersioning=false` хеш всё ещё считается (нужен для проверки дублей) — принято осознанно,
это один проход по своим embedded-файлам на старте.

**Побочный эффект (в release notes, не в этот план):** на `UseCompression=true` значение `?v=`
сменится с хеша сжатого на хеш сырого — разовый cache-bust всех Header-URL при апгрейде. На демо
(`UseCompression: false`) вывод побайтово идентичен.

---

### Этап C — компрессия инлайн-скриптов в одном месте (пункт 5)

Решение: **не возвращать** пер-инстансные скрипты в storage/реестр (реанимирует D-класс коллизий,
рост реестра и `/astra/*`-утечку). Вместо этого единственный владелец компрессии инлайнов —
`InitScriptRenderer`, через уже существующий `IResourceCompressor`:

```csharp
internal sealed class InitScriptRenderer(IResourceCompressor? compressor = null) : IInitScriptRenderer
{
    public string Render<T>(T specification) where T : ISpecification
    {
        var template = GetOrParseTemplate(specification.InitScriptTemplatePath);
        var result = template.Render(specification, x => x.Name)
            ?? throw new InvalidOperationException("Result script cannot be null");
        return compressor?.CompressToString(new InMemoryResource("inline-init-script.js", result)) ?? result;
    }
    // …_templateCache без изменений — парсинг уже кэшируется (это было заблуждение в разборе:
    //    старый путь через RenderTemplateToString парсил каждый раз, новый — один раз на процесс)
}
```

- **Почему кэш не нужен:** кэшировать сжатый результат = ключ по контенту спецификации → unbounded
  рост (ровно та проблема, что убрали этапом 3 основного плана). Старый путь минифицировал на каждый
  запрос — новый не дороже, чем был, и дешевле из-за кэша разобранных `Template`.
- **Выбор компрессора уже делает DI**: `AddAstra` регистрирует Nuglify при `UseCompression=true`,
  иначе `StubCompressor` (`AstraRegistration.cs:38-44`); `AddSingleton<IInitScriptRenderer,
  InitScriptRenderer>()` пробросит параметр автоматически (MS DI внедряет даже опциональные ctors).
  Значит никакого нового флага в `AstraConfiguration` — **единый смысл `UseCompression` восстанавливается:
  сжато всё, что выводит Astra** (header-файлы, body startup, инлайн-init).
- Параметр nullable-по-умолчанию `= null` — чтобы `new InitScriptRenderer()` в существующих тестах
  компилился без правок (поведение: без минификации, как сейчас).
- `ViewBox` продолжает инжектить `IResourceCompressor` для `ConnectionErrorMessage` — это сжатие
  пользовательского HTML-ресурса, к инлайнам отношения не имеет; не трогать.
- Незначительный whitespace-дрейф против старого пути: `RenderBodyResource` оборачивал сжатый скрипт
  в `<script>\n…\n</script>`, helper'ы делают `<script>{script}</script>` — документировать в golden-тестах
    (вне объёма этого плана).

---

### Этап D — санитайз того, что уже решено иначе (код, не доки)

- `ResourceEntry.cs:22-26` — удалить закомментированную иерархию `ResourceSource`.
- `ResourceEntry.cs:35-38` — XML-doc врёт: `<param name="Source">` и `Lazy{T}` описывают поля, которых
  нет; переписать под реальную сигнатуру (после этапа A — с `EmbeddedPath/EmbeddedAssembly`).
- `HtmlResourceRenderer.cs:4` — удалить висячий `using Microsoft.Extensions.Logging;` (параметра уже нет).
- `IHtmlResourceRenderer.cs:27-28` — `<see cref="AddScriptResource"/>` ссылается на удалённый метод
  (сломанный cref; `GenerateDocumentationFile` выключен, потому не видно). Поправить текст скоупа под
  новый `AddResource(path, …)`.

---

## Порядок и проверки

A → B → C → D (A и B сцеплены: B даёт `ComputeVersion`-хелпер, который A использует в комментарии
про raw-чтение; C и D независимы от A/B).

Прогнать после каждого этапа (одноразовый probe-проект вне репозитория, как делали; временная папка):

| # | Сценарий | Ожидание до | Ожидание после |
|---|---|---|---|
| 1 | `AddResource(load-check.js, Header)` → `AddResource(viewbox.js → тот же вывод-имя, Body)` | silent → later crash | `InvalidOperationException` при втором add |
| 2 | идентичный двойной `AddResource` | no-op | no-op (без изменения) |
| 3 | `UseCompression=true`: двойной **идентичный** `AddJsCodeFromTemplate` | throws (D2) | no-op |
| 4 | двойной `AddJsCodeFromTemplate` с изменённой спецификацией | throws | throws (не регрессировать) |
| 5 | счётчик `OpenRead` в storage при регистрации | ≥1 (сжатие на старте) | 0 (raw-чтение мимо storage) |
| 6 | `AddResource` с отсутствующим embedded-путём | silent empty version | throws на старте |
| 7 | `InitScriptRenderer(new NuglifyResourceCompressor()).Render(spec)` | raw | сжатый JS; с `StubCompressor`/null — raw, байт-в-байт как сейчас |
| 8 | демо-смоук | `/home` 200, `/astra/viewbox.js` 200, `/astra/viewbox-*` 404, `/astra/load-check.js` 200 | **без изменений**; HTML главной побайтово идентичен (UseCompression=false) |
| 9 | `dotnet build -c Release` net7–net10 + solution | succeeded | succeeded (тесты — красные, вне объёма) |

## Риски и оговорки

| Риск | Механизм / гашение |
|---|---|
| Вывод имени ломается на именах файлов с точками (`a.min.js` → `min.js`) и при коллизии одинаковых имён из разных папок | перегрузка `AddResource(path, resourceName, …)`; сообщение об ошибке прямо говорит про неё |
| Breaking публичного API (`AddResource` без `name`) | библиотека на мажоре 2.0.0 (уже ломаем `AddViewBox*`); в docs/`Release` notes — этап 4 основного плана |
| Fail-fast на отсутствующем embedded-пути меняет поведение существующих mock-тестов, передающих выдуманные пути (`"Test.Path.Css"`) | это осознанный сдвиг (ошибка на старте лучше 500 в эндпоинте); миграция тестов — на стороне пользователя |
| Nuglify на кривом пользовательском template-выводе | Uglify не бросает, возвращает `code` + список ошибок — поведение не ХУЖЕ старого пути (тот сжимал то же самое) |
| Изменение значений `?v=` при `UseCompression=true` | разовый cache-bust на апгрейде, нормален для мажора; в release notes |

## Не трогать (осознанно)

- Возврат init-скриптов в `IResourceStorage` / реестр — отверждён (коллизии different-content, рост, URL-утечка).
- `IResourceStorage` — контракт и реализация без изменений (хранит сырое, сжимает при отдаче — так и было).
- Body startup-ресурсы в `/astra/*` (`load-check.js` → 200) — принято как неопасное; инвариант 4
  основного плана понизить до «не гарантируется для startup-ресурсов» при правке docs (позже).
- Тесты, `README-Versioning.md`, остальная документация, `CHANGELOG`.
