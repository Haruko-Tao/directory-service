# Directory Service

REST API справочника организационной структуры: подразделения (дерево любой глубины), локации и должности.

## Стек

- .NET 10, ASP.NET Core Web API, OpenAPI + Scalar
- PostgreSQL 17, EF Core (запись, миграции) + Dapper (чтение, CTE), расширения `ltree`, `pg_trgm`
- FluentValidation, CSharpFunctionalExtensions (Result pattern), Scrutor (регистрация handler-ов)
- Serilog + Seq, health checks
- xUnit, Testcontainers, Respawn — интеграционные тесты на реальной БД
- Анализаторы: Roslynator, SonarAnalyzer, Meziantou, AsyncFixer; Central Package Management

## Архитектура

```
backend/
├── Backend.sln
├── DirectoryService/
│   ├── src/
│   │   ├── DirectoryService.Domain                  — сущности, value objects, доменные правила
│   │   ├── DirectoryService.Core                    — команды/запросы, handler-ы, валидаторы
│   │   ├── DirectoryService.Infrastructure.Postgres — EF Core, миграции, репозитории, Dapper
│   │   ├── DirectoryService.Contracts               — DTO запросов и ответов
│   │   └── DirectoryService.Web                     — контроллеры, middleware, DI
│   └── IntegrationTests
└── Shared/                                          — общие библиотеки для сервисов
    ├── HarukoTech.Shared.Kernel     — Error, Failure, DomainException (NuGet-пакет HarukoTech.SharedKernel)
    ├── HarukoTech.Shared.Core       — ICommand/IQuery, handler-контракты, ITransactionManager, PagedResult
    └── HarukoTech.Shared.Framework  — Envelope, Result → HTTP, ExceptionMiddleware
```

Clean Architecture: зависимости направлены внутрь (Web → Core → Domain), CQRS-разделение команд и запросов.

## Возможности

- **Подразделения**: создание, изменение, мягкое удаление (soft delete + фоновая очистка), привязка локаций и должностей.
- **Дерево**: корни, дети, предки, поиск по названию — на `ltree` (материализованный путь) с GiST/GIN-индексами.
- **Перенос поддерева** (`PUT /departments/{id}/parent`): проверка циклов, массовое обновление путей одним SQL,
  защита от гонок через `SELECT ... FOR UPDATE` в порядке id (без deadlock), конфликт → `409`.
- **Локации**: список с фильтрами, сортировкой и пагинацией, топ локаций по числу подразделений.
- Единый формат ответа (`Envelope`) и ошибок, валидация входа → `400`, не найдено → `404`.

## Запуск

Требуется .NET 10 SDK и Docker.

```bash
# PostgreSQL (localhost:5433) и Seq (http://localhost:5341)
docker compose -f backend/DirectoryService/docker-compose.yml up -d

# API: http://localhost:5292, документация — /scalar, проверка — /health
cd backend/DirectoryService/src/DirectoryService.Web
dotnet run --launch-profile http
```

Пакет `HarukoTech.SharedKernel` берётся из GitHub Packages — для `restore` нужен PAT с `read:packages`,
см. [README пакета](backend/Shared/HarukoTech.Shared.Kernel/README.md).

## Тесты

```bash
dotnet test backend/Backend.sln
```

Интеграционные тесты сами поднимают PostgreSQL в контейнере (Testcontainers) и очищают БД перед каждым тестом (Respawn).

