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

## Postman

Importar `postman/API.postman_collection.json`. La variable `baseUrl` usa `http://localhost:5018`.



## Pruebas unitarias

Desde la raíz:

```bash
dotnet test API.Tests/API.Tests.csproj
```

Validación de creación, actualización y eliminación, incluyendo el límite de contraseña en bytes UTF-8.
