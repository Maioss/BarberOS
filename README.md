# BarberOS

Sistema de gestión de barberías multi-sede. Backend .NET 10 con arquitectura hexagonal + PostgreSQL. Frontend React + TypeScript.

## Stack

**Backend:** .NET 10 · ASP.NET Core · EF Core + PostgreSQL · JWT + BCrypt · FluentValidation · Swagger  
**Frontend:** Vite · React 19 · TypeScript estricto · Tailwind CSS · Axios · Recharts

## Levantar el proyecto

**Requisitos:** .NET 10 SDK · Node.js 18+ · Docker

```bash
# 1. Base de datos
docker compose up -d

# 2. Secreto de firma JWT (solo la primera vez)
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)" --project BarberOS.Api

# 3. Backend
dotnet run --project BarberOS.Api
# → http://localhost:5126  |  /swagger para la documentación

# 4. Frontend
cd frontend
npm install
# Crear frontend/.env con:  VITE_API_URL=http://localhost:5126
npm run dev
# → http://localhost:5173
```

Al arrancar por primera vez el backend aplica las migraciones y siembra los datos de demo automáticamente.

## Credenciales de demo

| Rol | Correo | Contraseña |
|---|---|---|
| Super administrador | `samin@barberos.com` | `Pitch2026!` |
| Administrador | `admin.pitch@barberos.com` | `Pitch2026!` |
| Barbero | `barbero.pitch@barberos.com` | `Pitch2026!` |
| Cliente | `cliente.pitch@barberos.com` | `Pitch2026!` |

## Arquitectura

```
BarberOS.Domain/         Entidades y reglas de negocio — cero dependencias externas
BarberOS.Application/    Casos de uso, DTOs, puertos (interfaces)
BarberOS.Infrastructure/ EF Core, repositorios, mappers, seeding
BarberOS.Api/            Controllers, middleware, configuración
frontend/                SPA React
```

Las dependencias apuntan hacia el centro: `Domain` no conoce a nadie, `Application` solo conoce a `Domain`, `Infrastructure` implementa los puertos, `Api` orquesta.

## Roles

| Rol | Acceso |
|---|---|
| **Cliente** | Explorar barberías, reservar, ver y cancelar sus citas, editar perfil |
| **Barbero** | Agenda diaria, marcar citas como completadas/canceladas, ver saldo |
| **Administrador** | Gestionar barberos y servicios, métricas, supervisar citas |
| **Super administrador** | Todo lo anterior + crear barberías y usuarios de cualquier rol |
