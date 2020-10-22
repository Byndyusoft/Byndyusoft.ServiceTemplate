# Шаблон сервиса .NET Core

## Что включает?
Шаблон включает 8 проектов для полноценного запуска сервиса. Подключены swagger, jaeger, nlog. Добавлены сервисы для работы с базой, реббитом, s3 хранилищем. Апи поддерживает версионирование, хелс чек \status.

- Byndyusoft.ServiceTemplate.Domain - бизнес-логика приложения
- Byndyusoft.ServiceTemplate.Tests - юнит-тесты на бизнес логику
- Byndyusoft.ServiceTemplate.DataAccess - слой доступа к данным
- Byndyusoft.ServiceTemplate.Migrator - мигратор базы данных на основе https://github.com/fluentmigrator/fluentmigrator
- Byndyusoft.ServiceTemplate.Api - веб-апи приложение, с добавленной фоновой службой
- Byndyusoft.ServiceTemplate.Api.Client - клиент для веб-апи
- Byndyusoft.ServiceTemplate.Api.Shared - дто для веб-апи и клиента, расширение для подключения клиента в потребителе
- Byndyusoft.ServiceTemplate.IntegrationTests - интеграционные тесты на веб-апи



## Как использовать шаблон?
### установка шаблона из nuget
dotnet new --install Byndyusoft.ServiceTemplate

### создание нового сервиса из шаблона
dotnet new bsservice -n {Название сервиса}

###
проект готов к использованию

## Ручная сборка и установка пакета из репозитория
### создание нугет пакета из шаблона 
dotnet pack Byndyusoft.ServiceTemplate.csproj

### установка шаблона 
cd bin/debug

dotnet new -i Byndyusoft.ServiceTemplate1.0.0.nupkg


