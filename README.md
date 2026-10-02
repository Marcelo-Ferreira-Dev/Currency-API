# Prueba Técnica API

## Versiones

- .NET 10 — SDK 10.0.401.
- Entity Framework Core SQLite 10.0.12.
- dotnet-ef 10.0.12.

## Ejecutar

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
cd API
dotnet restore
dotnet build
dotnet ef database update
dotnet run --launch-profile http
```

## Implementación

- Implementado: entidades User, Address y Currency, DbContext, conexión SQLite y migración inicial.
