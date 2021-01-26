# Шаблон сервиса .NET Core

## Что включает?
Шаблон включает 8 проектов для полноценного запуска сервиса. Подключены swagger, jaeger, nlog. Добавлены сервисы для работы с базой данных, RabbitMQ, S3-хранилищем. API поддерживает версионирование, health-check\status.

- Byndyusoft.ServiceTemplate.Domain - бизнес-логика приложения
- Byndyusoft.ServiceTemplate.Tests - юнит-тесты на бизнес-логику
- Byndyusoft.ServiceTemplate.DataAccess - слой доступа к данным
- Byndyusoft.ServiceTemplate.Migrator - мигратор базы данных на основе https://github.com/fluentmigrator/fluentmigrator
- Byndyusoft.ServiceTemplate.Api - веб-апи приложение, с добавленной фоновой службой
- Byndyusoft.ServiceTemplate.Api.Client - клиент для веб-апи
- Byndyusoft.ServiceTemplate.Api.Shared - DTO для веб-апи и клиента, расширение для подключения клиента в потребителе
- Byndyusoft.ServiceTemplate.IntegrationTests - интеграционные тесты на веб-апи



# Как использовать шаблон?
### Установка шаблона из nuget в консоли Windows:
`dotnet new --install Byndyusoft.ServiceTemplate`

![gif](https://i.imgur.com/yKPSngl.gif)

### Создание нового сервиса из шаблона (выполнять в пустой директории)
`dotnet new bsservice -n {Название сервиса}`

![gif](https://i.imgur.com/q0ivkq1.gif)

Проект готов к использованию!

![gif](https://i.imgur.com/vCcvlj1.png)

# Модификация и локальное развёртывание пакета
## Ручная сборка и установка пакета из репозитория
### создание нугет пакета из шаблона 
`dotnet pack Byndyusoft.ServiceTemplate.csproj`

### установка шаблона 
`cd bin/debug`

`dotnet new -i Byndyusoft.ServiceTemplate1.0.0.nupkg`

# Maintainers
github.maintain@byndyusoft.com
