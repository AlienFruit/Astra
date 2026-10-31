# Обсуждение: Анализ расхождений План vs Реализация

**Дата**: 2026-10-31
**Контекст**: Сравнение `astra-resource-architecture-plan.md` с текущей реализацией в `HtmlResourceRenderer.cs` и смежных файлах.

---

## Краткое резюме

Текущая реализация = **Частичный Этап 2**. Этап 3 (stateless init-скрипты) — **не реализован**. Этап 1 — выполнен.

Ключевой вывод: План верно выявляет корневые причины. Код решает ~70% Этапа 2, но оставляет критические архитектурные проблемы (утечка памяти, засорение URL, гонки) нерешёнными, потому что Этап 3 отсутствует.

---

## 1. Оценка адекватности плана

### Сильные решения в плане
- Единый `ResourceEntry` + один `ConcurrentDictionary` → устраняет torn write структурно
- Атомарный `TryAdd` после полного построения entry → нет окна для частичного состояния
- `ResourceSource` как closed hierarchy → контент самодостаточен внутри записи
- Ленивая версия только для Header → Body никогда не считает хеш
- Нет timestamp-fallback при ошибке хеша → предотвращает cross-node cache thrashing
- `IInitScriptRenderer` stateless → per-instance скрипты никогда не трогают синглтон-реестр
- Tag helpers → `IInitScriptRenderer` (не `IHtmlResourceRenderer`) → чёткое разделение ответственности
- Удаление manual-path `AddViewBox*` целиком → breaking change в 2.0.0, честная миграция
- Race-тесты как feature-level константа → регрессионный страж между этапами

### Риски / вопросы в плане
| Проблема | Риск | Оценка |
|----------|------|--------|
| `EnsureSameRegistration` сравнивает только Kind+Location | Разный контент с одинаковыми метаданными → silent first-write-wins | **Баг плана**: нужно сравнивать хеш контента / Source |
| `ResourceSource` в диаграмме, но закомментирован в коде Этапа 2 | Body-контент всё равно через storage round-trip | **Противоречие плана**: либо Source в entry, либо отказ от инварианта 4 |
| `resourceName` для компрессора в `RenderInline` | Протечка абстракции (renderer знает расширение) | Приемлемый компромисс для byte-compat; можно передавать MimeType явно |
| Default interface methods для совместимости | DIM добавляет сложность потребителям | Спорно: чистый major break лучше, чем трения от DIM |
| Судьба `IResourceResolver` не упомянута | Мёртвый интерфейс или новая роль? | **Пробел**: явно решить удалить/оставить/переопределить |

### Возможные упрощения
- `Lazy<string>? Version` → `string Version` (вычисляется при регистрации, lazy не даёт выгоды)
- `resourceVersionCache` instance field → static helper
- Этап 4 (доки) → вынести в `CHANGELOG.md` / `RELEASES.md`

---

## 2. Проблемы плана vs Текущая реализация (детально)

### План §12-19: Диагноз — «Факт размазан по 4 словарям, join строковым ключом, согласованность — порядок вызовов»

| Проблема плана | Решена? | Доказательства |
|----------------|---------|----------------|
| Torn write (имя есть, контента нет) | ✅ **Да** | Один `ConcurrentDictionary<string, ResourceEntry>`, `TryAdd` после полного entry (строки 50-54, 137-140) |
| First-write-wins молча (ViewBox collision) | ❌ **Нет** | `EnsureSameRegistration` закомментирован (строки 101-108). Дубликат имени → ранний `return` (строки 37-40, 117-119) |
| Неограниченный рост при data-derived Id | ❌ **Нет** | Tag helpers всё ещё регистрируют `viewbox-init-{Id}`, `viewbox-link-init-{Id}` в синглтоне |
| URL-namespace `/astra/*` засорен per-instance именами | ❌ **Нет** | Body-ресурсы пишутся в `ResourceStorage` (строки 50, 137) → отдаются через endpoint |
| `resourceHtml` — мёртвый код | ✅ **Да** | Удалено |
| Hash считается для Body, где никогда не читается | ⚠️ **Частично** | Версия считается при регистрации для ВСЕХ (строки 51, 138). `GetResourceVersion` возвращает пусто для Body, но работа уже сделана |

### Инварианты Этапа 2 (план строки 39-46)

| Инвариант | Статус | Примечания |
|-----------|--------|------------|
| 1. Один ключ → одна атомарная запись; «полурегистрированного» невыразимо типом | ✅ | Достигнут через один словарь + атомарный TryAdd |
| 2. Tag helper не мутирует синглтон: нет роста, нет гонок, нет first-write-wins | ❌ | Tag helpers вызывают `AddJsCodeFromTemplate(..., Body)` → мутируют синглтон |
| 3. Имена `viewbox-*` в одном месте (константы в спецификациях); движок и tag helper физически не могут зарегистрировать одно имя с разной локацией | ❌ | Движок регистрирует Header, tag helpers — Body под теми же префиксами |
| 4. Body-ресурсы: без `?v=`, не в URL-пространстве | ❌ | Body в storage → доступны через `/astra/{name}` (нет `?v=`, но всё равно отдаются) |

### Конкретные пункты Этапа 2

| Пункт плана | Реализация |
|-------------|------------|
| `ResourceEntry` с `Source` (EmbeddedSource \| CodeSource) | ❌ **Отсутствует** — `ResourceEntry` хранит только `Version` (string). Контент через `ResourceStorage` round-trip (`ResolveContent` строки 148-158) |
| Схлопнуть словари → один `ConcurrentDictionary` | ✅ Сделано |
| Все `Add*` конструируют entry целиком, один `TryAdd` | ✅ Сделано |
| Дисциплина «storage перед реестром» стала ненужной | ✅ Сделано (атомарный entry) |
| Повторный add с другим содержимым → `InvalidOperationException` | ❌ **Не сделано** — закомментировано, тихий `return` (first-write-wins) |
| Идентичная повторная регистрация → идемпотентный no-op | ✅ Работает (ранний return при существующем ключе) |
| `ResourceStorage` прокси только для Header | ❌ **Нарушено** — `AddJsCodeFromTemplate` для Body пишет в storage (строка 137) |

### Этап 3: Главный сдвиг (stateless per-instance скрипты)

| Пункт плана | Реализация |
|-------------|------------|
| `IInitScriptRenderer` интерфейс + stateless имплементация | ❌ **Не существует** |
| Tag helpers используют `RenderInline` вместо `AddJsCodeFromTemplate` + `RenderBodyResource` | ❌ **Не сделано** — все 3 helper'а используют старый путь |
| `/astra/viewbox-link-init-{Id}` → 404 | ❌ **Не сделано** — отдаёт 200 |
| Нулевой рост памяти по уникальным Id | ❌ **Не сделано** — реестр растёт |
| Staleness устранён (нет общего состояния) | ❌ **Не сделано** — общее состояние через синглтон есть |

---

## 3. Что текущий код делает проще (оправдано)

| Подход плана | Текущий код | Вердикт |
|--------------|-------------|---------|
| `Lazy<string>? Version` — расчёт при первом чтении Header | `string Version` — расчёт сразу при `Add*` регистрации | ✅ **Правильное упрощение**: Header всегда рендерится после регистрации; lazy добавляет накладные расходы без выгоды |
| `ResourceSource` hierarchy в entry | Контент через `ResourceStorage` | ❌ **Не оправдано**: Инвариант 4 плана («Body не в URL space») требует Source в entry для гарантии типом. Текущий подход утечка зависимости от storage. |

---

## 4. Нерешённые проблемы плана (по приоритету)

| # | Проблема | Серьёзность | Блокируется |
|---|----------|-------------|-------------|
| 1 | First-write-wins при конфликте контента (отключён `EnsureSameRegistration`) | 🔴 Критично | Завершение Этапа 2 |
| 2 | Tag helpers мутируют синглтон-реестр (per-instance имена регистрируются глобально) | 🔴 Критично | Реализация Этапа 3 |
| 3 | Body-ресурсы отдаются через `/astra/{name}` (засорение URL namespace) | 🔴 Критично | Этап 3 (stateless путь обходит storage) |
| 4 | `ResourceSource` нет в `ResourceEntry` — контент не самодостаточен | 🟡 Средне | Завершение Этапа 2 |
| 5 | Hash считается для Body зря | 🟢 Низко | Тривиальный фикс: пропуск расчёта версии для Body location |

---

### Твоя интуиция про `Lazy` верна — план перестраховался. Версия вычисляется при регистрации, `Lazy` не нужен. `string Version` — достаточно.

**Но остальные 4 проблемы — реальные, и план их решает правильно.** Этап 3 (stateless init scripts) — единственный способ убрать per-instance имена из синглтона. Без него проблемы 2 и 3 нерешаемы.

---

## 5. Пункты для детального обсуждения

### 5.1 `EnsureSameRegistration` — что сравнивать?
- **Текст плана**: «different content, kind or location»
- **Текущий закомментированный код**: сравнивает только `Kind` + `Location`
- **Нужно**: Сравнивать хеш контента (или равенство `Source`). Варианты:
  - Хранить `Source` в entry → сравнивать `Source` напрямую
  - Считать хеш при регистрации → сравнивать хеш
  - Для `CodeSource`: сравнивать строковый контент
  - Для `EmbeddedSource`: сравнивать path+assembly

### 5.2 `ResourceSource` в `ResourceEntry` — да или нет?
- **Диаграмма плана**: Да (строка 28: `Source` в entry)
- **Текст Этапа 2** (строки 89-91): Да (closed hierarchy)
- **Реальный код Этапа 2**: Закомментировано
- **Решение нужно**: Если нет `Source` в entry, как гарантировать инвариант 4 (Body не в URL space)? Сейчас только по соглашению (tag helpers используют Body). Движок может случайно зарегистрировать Body.

### 5.3 Дизайн Этапа 3 `IInitScriptRenderer`
```csharp
public interface IInitScriptRenderer
{
    IHtmlContent RenderInline<T>(string resourceName, string templatePath, T specification) where T : class;
}
```
- `resourceName` используется только для компрессора (расширение → минификация)
- Альтернатива: передавать `string extension` или `MimeType` явно → чище абстракция
- Имплементация: Scriban parse + render → обернуть в `<script>` → сжать → вернуть `HtmlString`
- Никаких реестров, storage, состояния

### 5.4 Стратегия миграции Tag Helpers
| Helper | Текущий конструктор | Целевой конструктор |
|--------|---------------------|-------------------|
| `ViewBox` | `IHtmlResourceRenderer`, `IResourceCompressor` | `IInitScriptRenderer` |
| `ViewBoxLink` | `IHtmlResourceRenderer`, `IHttpContextAccessor` | `IInitScriptRenderer`, `IHttpContextAccessor` |
| `ViewBoxLinkGroup` | `IHtmlResourceRenderer`, `IHttpContextAccessor` | `IInitScriptRenderer`, `IHttpContextAccessor` |

- `ViewBox` также использует `IResourceCompressor` для сообщения об ошибке — оставить или перенести?
- DI регистрация: `AddSingleton<IInitScriptRenderer, InitScriptRenderer>()`

### 5.5 Breaking Changes для 2.0.0
- Удалить `AstraEngine.AddViewBox`, `AddViewBoxLink`, `GetViewBoxLinkClass` → СДЕЛАНО (по git-истории)
- `AddJsCodeFromTemplate(..., Body)` / `RenderBodyResource` → оставить, но XML-doc ограничить область (план строки 141-142)
- Tag helpers теперь требуют DI (нет ручного `new`) → тестовые хелперы нужно поправить через `Activator`

### 5.6 `IResourceResolver` — удалить или переопределить?
- Сейчас в `Abstractions/`, не используется в новой архитектуре
- План молчит
- Варианты: удалить, или переопределить как «resolve resource by name для кастомных сценариев»

### 5.7 Стратегия тестов
- `ResourceRegistrationRaceTests` — должны остаться неизменными между этапами (план строка 190)
- Новые тесты нужны:
  - `AddJsCode_SameNameDifferentContent_Throws`
  - `GetResourceUrl_ConcurrentSameHeaderResource_ComputesHashOnce`
  - `SingletonRegistry_DoesNotGrow_OverManyGeneratedIds`
  - Golden output тесты для каждого tag helper
  - `InitScript_IsNotRegisteredGlobally` для каждого helper'а

---

## 6. Предлагаемые следующие шаги

1. **Доделать Этап 2 как положено**
   - Включить `EnsureSameRegistration` с сравнением контента
   - Добавить `ResourceSource` в `ResourceEntry` (discriminated union)
   - Сделать `ResolveContent` читающим из `entry.Source`
   - Пропускать расчёт версии для `Body` location

2. **Реализовать Этап 3**
   - Добавить `IInitScriptRenderer` в `Abstractions/`
   - Добавить `InitScriptRenderer` в `Core/`
   - Перевести 3 tag helper'а на инжект `IInitScriptRenderer`
   - Зарегистрировать в DI
   - Проверить: реестр перестаёт расти, `/astra/viewbox-*` → 404, HTML вывод побайтово идентичен

3. **Починить тесты** (компиляция + прохождение)

4. **Этап 4**: Доки, release notes, CHANGELOG

---

## 7. Открытые вопросы для обсуждения

1. **Сравнение контента в `EnsureSameRegistration`**: Хранить `Source` в entry vs считать хеш при регистрации?
2. **Иерархия `ResourceSource`**: Стоит ли сложности для Body-пути, или оставить storage round-trip по соглашению?
3. **Сигнатура `IInitScriptRenderer.RenderInline`**: Оставить `resourceName` для компрессора или передавать расширение явно?
4. **Сжатие ошибки в `ViewBox`**: Сейчас использует `IResourceCompressor`, инжектённый в tag helper. Перенести в `InitScriptRenderer` или оставить отдельно?
5. **Default interface methods vs чистый major break**: Что вызовет меньше трений в экосистеме?
6. **`IResourceResolver`**: Удалить или переопределить?

---

*Этот документ служит текущим логом обсуждения. Добавляй комментарии, решения и обновления по мере итераций.*