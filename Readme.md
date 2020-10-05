# Шаблон сервиса .NET Core

## Что включает?
Шаблон включает 8 проектов для полноценного запуска сервиса. Подключены swagger, jaeger, nlog. Добавлены сервисы для работы с базой, реббитом, s3 хранилищем. Апи поддерживает версионирование, хелс чек \status.

## Как использовать шаблон?
### создание нугет пакета из шаблона
dotnet pack Byndyusoft.ServiceTemplate.csproj

### установка шаблона
cd bin/debug

dotnet new -i Byndyusoft.ServiceTemplate1.0.0.nupkg

### создание нового сервиса из шаблона
dotnet new bsservice -n {Название сервиса}