# Система управления складом

## Проект рефакторинга архитектуры

Этот проект демонстрирует рефакторинг архитектуры приложения управления складом:

### ✅ **Что было сделано:**

1. **Убран CQRS для справочных сущностей** 
   - `Resource` (ресурсы)
   - `UnitOfMeasure` (единицы измерения)  
   - `Client` (клиенты)

2. **Создан сервисный слой для справочников**
   - `IResourceService` / `ResourceService`
   - `IUnitOfMeasureService` / `UnitOfMeasureService`
   - `IClientService` / `ClientService`

3. **Создан обобщенный репозиторий**
   - `INamedEntityRepository<T>` / `NamedEntityRepository<T>`
   - Методы для архивирования, проверки уникальности имени

4. **Оставлен CQRS только для документов и баланса**
   - `ReceiptDocument` (документы поступления)
   - `ShipmentDocument` (документы отгрузки)
   - `Balance` (баланс на складе)

5. **Исправлены доменные модели**
   - Правильные конструкторы для EF Core
   - Корректная инициализация свойств

6. **Настроена архитектура**
   - Clean Architecture с разделением на слои
   - DDD принципы
   - SOLID принципы

### 🏗️ **Структура проекта:**

```
├── WarehouseManagement.Domain/          # Доменный слой
│   ├── Aggregates/                      # Агрегаты
│   ├── Common/                          # Базовые классы
│   └── ValueObjects/                    # Объекты-значения
├── WarehouseManagement.Application/     # Слой приложения
│   ├── Features/                        # CQRS для документов
│   │   ├── ReceiptDocuments/           # Документы поступления
│   │   ├── ShipmentDocuments/          # Документы отгрузки
│   │   └── BalanceQueries/             # Запросы баланса
│   ├── Services/                        # Сервисы для справочников
│   └── Common/                          # Репозитории
├── WarehouseManagement.Infrastructure/  # Слой инфраструктуры
│   └── Data/                           # Entity Framework
├── WarehouseManagement.Web/            # Web API
│   └── Controllers/                    # REST API контроллеры
└── WarehouseManagement.Tests/          # Тесты
```

### 🎯 **Архитектурные принципы:**

- **Простые справочники** → обычный сервисный подход
- **Сложные операции с документами** → CQRS + Event Sourcing ready
- **Обобщенные репозитории** для устранения дублирования
- **Clean Architecture** с четким разделением ответственности

### 🚀 **Как запустить:**

1. Настройте PostgreSQL
2. Обновите строку подключения в `appsettings.json`
3. Запустите приложение: `dotnet run --project WarehouseManagement.Web`

### 📋 **API Endpoints:**

**Справочники (Services):**
- `GET /api/Resources` - получить все ресурсы
- `POST /api/Resources` - создать ресурс
- `PUT /api/Resources/{id}` - обновить ресурс
- `POST /api/Resources/{id}/archive` - архивировать

**Документы (CQRS):**
- `POST /api/ReceiptDocuments` - создать документ поступления
- `POST /api/ShipmentDocuments` - создать документ отгрузки
- `POST /api/ShipmentDocuments/{id}/sign` - подписать отгрузку

**Баланс:**
- `GET /api/Balance` - получить остатки на складе

### ✨ **Ключевые особенности:**

- 🔄 **Гибридная архитектура**: сервисы для простых операций, CQRS для сложных
- 📦 **DDD подход**: правильное моделирование предметной области  
- 🏛️ **Clean Architecture**: четкое разделение слоев
- 🛡️ **SOLID принципы**: расширяемый и поддерживаемый код
- 🗄️ **Entity Framework**: автоматическое создание БД
- 🎯 **PostgreSQL**: промышленная база данных
