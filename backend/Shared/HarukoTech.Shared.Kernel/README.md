# HarukoTech.SharedKernel

Базовые типы для сервисов HarukoTech: `Error`, `ErrorExtensions`, `Failure`, `DomainException`.

## Установка

1. Создать GitHub PAT (classic) с правом `read:packages`.
2. Добавить источник в личный NuGet.Config (токен НЕ коммитить):
   ```
   dotnet nuget add source "https://nuget.pkg.github.com/Haruko-Tao/index.json" --name github --username <ник> --password <PAT> --configfile "$env:APPDATA\NuGet\NuGet.Config"
   ```
3. В проекте: `<PackageReference Include="HarukoTech.SharedKernel" />`, версия — в `Directory.Packages.props`.

## Версии (SemVer: MAJOR.MINOR.PATCH)

- PATCH (0.1.0 → 0.1.1) — исправление, публичный API не меняется.
- MINOR (0.1 → 0.2) — новое, обратно совместимое.
- MAJOR (1.x → 2.0) — ломающее изменение (удалён/переименован тип, изменена сигнатура).
- 0.x — API ещё не стабилен.
- Пред-релиз `X.Y.Z-ci.N` — тестовые сборки CI.
- Опубликованную версию изменить нельзя (повторный push → 409) — только новая.

## Выпуск версии

1. Поднять `<Version>` в `HarukoTech.Shared.Kernel.csproj`.
2. Merge в `main`.
3. GitHub → Actions → «Publish HarukoTech.SharedKernel» → Run workflow.
4. Поставить тег: `git tag -a vX.Y.Z -m "..."` → `git push origin vX.Y.Z`.
