# Установка шаблона AlienFruit.Astra для Visual Studio

Этот шаблон позволяет создавать новые ASP.NET MVC проекты с предустановленной библиотекой AlienFruit.Astra для динамической загрузки контента.

## 🚀 Шаблон готов!

Шаблон AlienFruit.Astra находится в папке `template/clean-template/` и включает:

- ✅ Полностью настроенный ASP.NET MVC проект
- ✅ Предустановленную библиотеку AlienFruit.Astra
- ✅ Рабочие Tag Helpers (`<view-box>` и `<view-box-link>`)
- ✅ Демонстрационную навигацию с AJAX загрузкой
- ✅ Bootstrap для красивого интерфейса

## Способы установки

### 1. Локальная установка шаблона в Visual Studio (рекомендуется)

1. **Найдите ZIP архив шаблона** в папке `template/` (файл будет создан при сборке)

2. **Скопируйте содержимое папки** `template/clean-template/` в:
   ```
   %USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\AlienFruit.Astra.Template\
   ```

3. **Перезапустите Visual Studio**

### 2. Ручная установка

1. **Создайте папку** для шаблона:
   ```
   %USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\AlienFruit.Astra.Template\
   ```

2. **Скопируйте все файлы** из `template/clean-template/` в созданную папку

3. **Перезапустите Visual Studio**

### 2. Установка через .NET CLI (альтернативный способ)

1. **Установите шаблон глобально:**
   ```bash
   dotnet new install "путь\к\папке\AlienFruit.Astra.Template"
   ```

2. **Проверьте установку:**
   ```bash
   dotnet new list
   ```

   Вы должны увидеть шаблон с именем "AlienFruit.Astra Web App"

## Использование шаблона

### В Visual Studio:
1. Откройте Visual Studio
2. Выберите "Create a new project"
3. Найдите "AlienFruit.Astra Web App" в списке шаблонов
4. Выберите шаблон и нажмите "Next"
5. Укажите имя проекта и расположение
6. Нажмите "Create"

### Через .NET CLI:
```bash
dotnet new astra -n MyAstraProject
```

## Что включает шаблон

Шаблон создает полностью настроенный ASP.NET MVC проект с:

- ✅ Предустановленной библиотекой AlienFruit.Astra
- ✅ Настроенными Tag Helpers (`<view-box>` и `<view-box-link>`)
- ✅ Правильной конфигурацией в `Program.cs`
- ✅ Готовым `_Layout.cshtml` с демонстрацией работы Astra
- ✅ Базовой навигацией с AJAX загрузкой
- ✅ Настроенными статическими файлами

## Первый запуск

1. После создания проекта запустите его (`F5` или `Ctrl+F5`)
2. Откройте браузер и перейдите по ссылкам в навигации
3. Обратите внимание, что переходы между страницами происходят без перезагрузки всей страницы

## Структура проекта

```
YourProjectName/
├── Controllers/
│   └── HomeController.cs
├── Models/
│   └── ErrorViewModel.cs
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── Error.cshtml
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   └── js/
│       └── site.js
├── appsettings.json
├── Program.cs
└── YourProjectName.csproj
```

## Следующие шаги

- Изучите документацию AlienFruit.Astra в README.md
- Добавьте свои контроллеры и представления
- Настройте внешний вид в `_Layout.cshtml`
- Добавьте дополнительные ресурсы (CSS/JS) через `@AstraEngine.RenderHeaders()`

## Удаление шаблона

### Из Visual Studio:
Удалите папку `AlienFruit.Astra.Template` из `%USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\`

### Через .NET CLI:
```bash
dotnet new uninstall AlienFruit.Astra.Template
```
