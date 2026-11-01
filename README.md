# 03637_Proyecto1

UNED — Proyecto 1 del curso 03637 Gestión de Sitios Web.
Incremento funcional de la plataforma web de la Asociación de Emprendedoras.

## Estructura

```
src/Emprendedoras.Web/     Aplicación web (ASP.NET Core Razor Pages + Identity + SQLite)
tests/Emprendedoras.Tests/ Pruebas (xUnit)
Emprendedoras.slnx         Solución
global.json                Versión del SDK de .NET
```

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (versión 10.0.403 o superior)

## Ejecutar

```bash
dotnet run --project src/Emprendedoras.Web
```

La base de datos SQLite (`app.db`) se crea automáticamente al iniciar y no se versiona.

## Probar

```bash
dotnet test
```

## Migraciones de base de datos (desarrollo)

La herramienta `dotnet-ef` está declarada como herramienta local en `dotnet-tools.json`:

```bash
dotnet tool restore
dotnet dotnet-ef migrations add NombreDeLaMigracion --project src/Emprendedoras.Web --output-dir Data/Migrations
```

## Datos

Todos los datos del proyecto son sintéticos. No se incluyen credenciales ni datos personales reales.
