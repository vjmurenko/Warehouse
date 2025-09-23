# Система управления складом


[Техническое задание](TASK.md)


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

#### Вариант 1: Запуск с помощью Docker (рекомендуется)

1. Убедитесь, что у вас установлен Docker и Docker Compose
2. Выполните команду в корневой директории проекта:
   ```bash
   docker-compose up --build
   ```
3. После запуска приложение будет доступно по адресам:
   - Фронтенд: http://localhost:3000
   - Бэкенд API: http://localhost:8080
   - База данных PostgreSQL: localhost:5432

#### Вариант 2: Запуск без Docker

1. Настройте PostgreSQL
2. Обновите строку подключения в `appsettings.json`
3. Запустите приложение: `dotnet run --project WarehouseManagement.Web`



