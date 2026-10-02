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
| POST | `/currencies` |
| GET | `/currencies?isActive=true` (filtro opcional) |
| POST | `/currency/convert` |

DELETE desactiva usuarios y direcciones con `IsActive = false`. Los listados incluyen activos e inactivos; PUT permite reactivarlos. Las monedas incluyen `IsActive`, con valor predeterminado `true`.

Monedas: código y nombre obligatorios, código único y tasa positiva. `isActive` es opcional al crear y vale `true` por defecto. Los códigos se guardan en mayúsculas.

Conversión: importe positivo y monedas existentes y activas. Fórmula: `amount * from.RateToBase / to.RateToBase`, usando `decimal`. Moneda inexistente devuelve `404`, inactiva `409` y desbordamiento `400`.

`RateToBase` expresa el valor de una unidad en la moneda base; los ejemplos usan PYG con tasa 1 y USD con tasa 6000. Las tasas son de prueba y no se consultan cotizaciones externas.

## Postman

Importar `postman/API.postman_collection.json`. La variable `baseUrl` usa `http://localhost:5018`.
Los requests de creación guardan `userId` y `addressId` para las demás operaciones.

## Pruebas

Desde la raíz:

```bash
dotnet test API.Tests/API.Tests.csproj
```
