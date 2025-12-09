# Настройка автоматической публикации NuGet пакета через GitHub Actions

Этот документ описывает процесс настройки автоматической публикации NuGet пакета AlienFruit.Astra на nuget.org при помощи GitHub Actions.

## Предварительные требования

1. **Аккаунт на nuget.org** - вам понадобится API ключ для публикации пакетов
2. **GitHub репозиторий** - проект должен быть размещен на GitHub
3. **Git теги** - для управления версиями пакетов

## Шаг 1: Получение NuGet API ключа

1. Перейдите на [nuget.org](https://www.nuget.org/)
2. Войдите в свой аккаунт (или создайте новый)
3. Перейдите в настройки аккаунта → "API Keys"
4. Создайте новый API ключ с правами на публикацию пакетов
5. Скопируйте сгенерированный ключ (он понадобится на следующем шаге)

## Шаг 2: Настройка секретов в GitHub

1. Откройте ваш GitHub репозиторий
2. Перейдите в **Settings** → **Secrets and variables** → **Actions**
3. Нажмите **New repository secret**
4. Создайте секрет с именем `NUGET_API_KEY`
5. Вставьте ваш NuGet API ключ в поле **Secret**

## Шаг 3: Создание GitHub Actions workflow

Создайте файл `.github/workflows/publish-nuget.yml` в корне вашего репозитория со следующим содержимым:

```yaml
name: Publish NuGet Package

on:
  push:
    tags:
      - 'v*.*.*'  # Триггер на создание тега, начинающегося с 'v' (например, v1.0.0)
  workflow_dispatch:  # Позволяет запускать workflow вручную

jobs:
  publish:
    runs-on: ubuntu-latest

    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0  # Полная история для GitVersion

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: |
          7.0.x
          8.0.x
          9.0.x
          10.0.x

    - name: Restore dependencies
      run: dotnet restore src/AlienFruit.Astra.slnx

    - name: Build project
      run: dotnet build src/AlienFruit.Astra.slnx --configuration Release --no-restore

    - name: Run tests (если есть)
      run: dotnet test src/AlienFruit.Astra.slnx --configuration Release --no-build --verbosity normal

    - name: Pack NuGet package
      run: dotnet pack src/AlienFruit.Astra/AlienFruit.Astra.csproj --configuration Release --no-build --output nupkgs

    - name: Publish to NuGet
      run: dotnet nuget push "nupkgs/*.nupkg" --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

## Шаг 4: Настройка GitVersion (рекомендуется)

Ваш проект уже использует GitVersion.MsBuild. Убедитесь, что у вас есть файл `GitVersion.yml` в корне репозитория:

```yaml
mode: ContinuousDelivery
branches:
  main:
    regex: ^main$
    tag: ''
    increment: Minor
    prevent-increment-of-merged-branch-version: true
    track-merge-target: false
    tracks-release-branches: false
    is-release-branch: true
  develop:
    regex: ^develop$
    tag: alpha
    increment: Minor
    prevent-increment-of-merged-branch-version: false
    track-merge-target: true
    tracks-release-branches: true
    is-release-branch: false
  pull-request:
    tag: PullRequest
  feature:
    tag: useBranchName
    increment: Inherit
    prevent-increment-of-merged-branch-version: false
    track-merge-target: true
  hotfix:
    tag: beta
    increment: Patch
    prevent-increment-of-merged-branch-version: false
    track-merge-target: true
```

## Шаг 5: Создание релиза

### Вариант 1: Использование Git тегов (рекомендуется)

1. Создайте Git тег для новой версии:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. GitHub Actions автоматически запустит workflow и опубликует пакет

### Вариант 2: Ручной запуск workflow

1. Перейдите на вкладку **Actions** в вашем GitHub репозитории
2. Выберите workflow **Publish NuGet Package**
3. Нажмите **Run workflow**

## Шаг 6: Проверка публикации

1. Дождитесь завершения GitHub Actions workflow
2. Проверьте статус на [nuget.org/packages/AlienFruit.Astra](https://www.nuget.org/packages/AlienFruit.Astra)
3. Убедитесь, что новая версия пакета появилась в списке

## Дополнительные настройки

### Автоматическое создание GitHub Release

Добавьте в workflow создание GitHub релиза:

```yaml
- name: Create GitHub Release
  if: startsWith(github.ref, 'refs/tags/v')
  uses: actions/create-release@v1
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  with:
    tag_name: ${{ github.ref }}
    release_name: Release ${{ github.ref }}
    body: |
      ## What's Changed

      ### NuGet Package
      - Published version ${{ env.PACKAGE_VERSION }} to NuGet.org

      Full changelog: https://github.com/${{ github.repository }}/compare/${{ github.event.before }}...${{ github.ref }}
    draft: false
    prerelease: false
```

### Настройка условий публикации

Вы можете настроить workflow для публикации только из определенных веток:

```yaml
on:
  push:
    branches:
      - main
      - master
    tags:
      - 'v*.*.*'
  pull_request:
    branches:
      - main
      - master
```

### Добавление проверки качества кода

Добавьте шаги для проверки качества кода перед публикацией:

```yaml
- name: Run code analysis
  run: dotnet build src/AlienFruit.Astra.slnx --configuration Release /p:RunCodeAnalysis=true

- name: Run tests with coverage
  run: dotnet test src/AlienFruit.Astra.slnx --configuration Release --collect:"XPlat Code Coverage"
```

## Устранение неполадок

### Проблема: "API key not valid"

- Убедитесь, что секрет `NUGET_API_KEY` правильно настроен в GitHub
- Проверьте срок действия API ключа на nuget.org

### Проблема: "Package already exists"

- GitVersion генерирует уникальные версии для каждого коммита
- Используйте `skip-duplicate` флаг в команде push

### Проблема: "Build fails"

- Убедитесь, что все зависимости указаны в .csproj файле
- Проверьте версию .NET SDK в workflow

### Проблема: "GitVersion fails"

- Убедитесь, что у вас есть хотя бы один Git тег
- Проверьте конфигурацию GitVersion.yml

## Полезные ссылки

- [Документация GitHub Actions](https://docs.github.com/en/actions)
- [Документация NuGet](https://docs.microsoft.com/en-us/nuget/)
- [GitVersion документация](https://gitversion.net/docs/)
- [Примеры GitHub Actions workflows](https://github.com/actions/starter-workflows)

## Поддержка

Если у вас возникли проблемы с настройкой, создайте issue в репозитории проекта или обратитесь в [GitHub Discussions](https://github.com/alienfruit/AlienFruit.Astra/discussions).
