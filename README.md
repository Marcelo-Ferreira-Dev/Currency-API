# Prueba Técnica API

## Versiones

- .NET 10 — SDK 10.0.401.
- Entity Framework Core SQLite 10.0.12.
- dotnet-ef 10.0.12.
- FluentValidation 12.1.1.
- BCrypt.Net-Next 4.2.0.

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

| Método | Ruta |
|---|---|
| POST | `/users` |
| GET | `/users?isActive=true` (filtro opcional) |
| GET | `/users/{id}` |
| PUT | `/users/{id}` |
| DELETE | `/users/{id}` |
| POST | `/users/{userId}/addresses` |
| GET | `/users/{userId}/addresses?isActive=true` (filtro opcional) |
| PUT | `/addresses/{id}` |
| DELETE | `/addresses/{id}` |

DELETE desactiva usuarios y direcciones con `IsActive = false`. Los listados incluyen activos e inactivos; PUT permite reactivarlos. Las monedas incluyen `IsActive`, con valor predeterminado `true`.

## Postman

Importar `postman/API.postman_collection.json`. La variable `baseUrl` usa `http://localhost:5018`.
Los requests de creación guardan `userId` y `addressId` para las demás operaciones.



## Pruebas

Desde la raíz:

```bash
dotnet test API.Tests/API.Tests.csproj
```

