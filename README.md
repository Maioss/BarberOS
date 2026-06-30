# BarberOS

Sistema de gestión de barberías multi-sede. Backend en .NET 10 con arquitectura hexagonal estricta y persistencia real en PostgreSQL vía Entity Framework Core. Frontend en React + TypeScript consumiendo la API de extremo a extremo.

## Qué resuelve

Una barbería con varias sedes necesita un solo sistema donde: los clientes reserven turno online, los barberos vean su agenda diaria y marquen turnos como atendidos, y los administradores supervisen ingresos, servicios y equipo desde un panel central. BarberOS cubre los cuatro roles del negocio (Super administrador, Administrador, Barbero, Cliente) con una sola base de datos y una sola API.

## Stack

| Capa | Tecnología |
|---|---|
| Backend | .NET 10, ASP.NET Core (Controllers, no Minimal API) |
| Persistencia | Entity Framework Core 10 + Npgsql, PostgreSQL 16 |
| Autenticación | JWT Bearer + BCrypt |
| Validación | FluentValidation |
| Documentación API | Swagger / OpenAPI |
| Frontend | Vite + React 18/19 + TypeScript estricto |
| Routing | React Router |
| HTTP | Axios con interceptor JWT |
| Estilos | Tailwind CSS (sistema visual propio) |
| Gráficos | Recharts |
| Almacenamiento de imágenes | Supabase Storage |

## Arquitectura del backend

El backend sigue **arquitectura hexagonal** (puertos y adaptadores) con cuatro proyectos físicamente separados:

```
BarberOS.Domain/          Entidades y reglas de negocio puras. Cero dependencias externas.
BarberOS.Application/     Casos de uso, DTOs, puertos (interfaces), validadores.
BarberOS.Infrastructure/  EF Core, repositorios, mappers, seguridad, seeding.
BarberOS.Api/             Controllers, middleware, configuración de la app.
```

La regla que ordena todo: las dependencias apuntan hacia el centro. `Domain` no conoce a nadie. `Application` solo conoce a `Domain`. `Infrastructure` implementa los puertos que `Application` define. `Api` orquesta, pero no contiene lógica de negocio.

**Por qué importa esta separación:** el dominio (`Domain`) no tiene una sola referencia a Entity Framework, a ASP.NET, ni a ningún framework externo. Las entidades (`User`, `Barbershop`, `Barber`, `Service`, `Appointment`, `Payment`) son clases de C# puro con constructores controlados y propiedades de solo lectura hacia afuera (`private set`), de forma que ninguna regla de negocio puede saltarse accidentalmente. Si mañana se quisiera cambiar PostgreSQL por SQL Server, o ASP.NET por otro framework web, el dominio entero permanece intacto.

Cada módulo de negocio (Auth, Barbershops, Users, Barbers, Services, Appointments, Payments, Metrics) sigue el mismo patrón interno en `Application`: `DTOs/`, `UseCases/`, `Validators/`. Los casos de uso reciben sus dependencias (repositorios, servicios) por inyección de constructor, nunca instancian nada directamente — eso es lo que permite testear cada caso de uso de forma aislada y lo que hace que `Program.cs` sea el único lugar donde se decide qué implementación concreta corresponde a cada interfaz.

**Persistencia.** Cada entidad de dominio tiene un `DbModel` paralelo en `Infrastructure` con las anotaciones de Entity Framework — el dominio nunca ve un atributo `[Required]` ni un `DbSet`. Un `Mapper` estático traduce en ambas direcciones: dominio → DbModel al guardar, DbModel → dominio al leer. Las migraciones de base de datos se gestionan con `dotnet ef` y se aplican automáticamente al arrancar la aplicación.

## Decisiones de diseño relevantes

- **Modelo unificado de barberías.** Una sede y una barbería principal son la misma entidad (`Barbershop`), diferenciadas por `IsMain` y `ParentId`. Evita duplicar lógica entre "negocio" y "sucursal".
- **Snapshots en las reservas.** Cuando un cliente reserva, el nombre, precio y duración de cada servicio se copian dentro de la reserva (`AppointmentService`). Si el precio del servicio cambia después, las reservas ya hechas no se alteran.
- **Soft delete como estándar.** Ninguna entidad con historial (barberos, servicios, citas) se borra físicamente — se marca inactiva. Preserva la integridad de reportes y métricas históricas.
- **Cálculo de disponibilidad en tiempo real.** Los horarios libres de un barbero se calculan combinando su jornada laboral, su horario de almuerzo, y sus citas confirmadas existentes — no es una tabla de slots precalculada.
- **Respuesta uniforme.** Todo endpoint devuelve la misma forma (`{ success, data, message, errors }`), lo que permite que el frontend tenga un único punto de manejo de respuestas y errores en su cliente HTTP.

## Arquitectura del frontend

SPA en React con TypeScript estricto. Estructura por responsabilidad:

```
src/api/          Funciones tipadas que llaman a cada endpoint del backend.
src/auth/         Contexto de autenticación, decodificación de JWT, rutas protegidas.
src/components/   Componentes reutilizables (UI base + específicos de dominio).
src/layouts/      Estructuras de página compartidas (pública, autenticada).
src/pages/        Una página por ruta.
src/lib/          Utilidades (formato de moneda/fecha, integración con Supabase).
```

El manejo de estado es deliberadamente simple: `useState` y `useContext`, sin librerías de estado global. Las llamadas a la API son `async/await` con manejo de errores en `try/catch`, y cada respuesta del backend está modelada con una interfaz TypeScript explícita — no se usa `any` en ningún punto del proyecto.

## Roles del sistema

| Rol | Qué puede hacer |
|---|---|
| **Cliente** | Explorar barberías, reservar turno, ver y cancelar sus propias reservas, editar su perfil |
| **Barbero** | Ver su agenda del día y fechas pasadas, marcar citas como completadas o canceladas, ver su saldo acumulado |
| **Administrador** | Gestionar barberos y servicios de su barbería, ver métricas e ingresos, supervisar todas las citas |
| **Super administrador** | Todo lo anterior, además de crear barberías nuevas y usuarios de cualquier rol en el sistema |

## Cómo levantar el proyecto localmente

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 18 o superior
- [Docker](https://www.docker.com/) (para PostgreSQL)
- Cuenta de [Supabase](https://supabase.com/) (gratuita) si se quiere usar la subida de fotos de perfil

### 1. Clonar el repositorio

```bash
git clone <url-del-repo>
cd BarberOS
```

### 2. Levantar la base de datos

Desde la raíz del repo:

```bash
docker compose up -d
```

Esto levanta PostgreSQL 16 en el puerto `5432` con la base `barberos` ya creada. Verificar que el contenedor está saludable:

```bash
docker compose ps
```

### 3. Arrancar el backend

Desde la raíz del repo:

```bash
dotnet build
dotnet run --project BarberOS.Api
```

Al arrancar por primera vez, la aplicación aplica las migraciones de base de datos automáticamente y siembra los datos iniciales (usuarios, barberías, servicios de demostración).

El backend queda disponible en `http://localhost:5126` (revisar la consola al arrancar para confirmar el puerto exacto — puede variar según el entorno). La documentación interactiva de la API está en `http://localhost:5126/swagger`.

### 4. Configurar y arrancar el frontend

```bash
cd frontend
npm install
```

Crear un archivo `.env` en `frontend/` (usar `.env.example` como referencia) con:

```
VITE_API_URL=http://localhost:5126
VITE_SUPABASE_URL=<url-del-proyecto-supabase>
VITE_SUPABASE_ANON_KEY=<anon-key-del-proyecto-supabase>
```

Las dos variables de Supabase solo son necesarias si se quiere probar la subida de fotos de perfil — el resto de la aplicación funciona sin ellas.

Levantar el frontend:

```bash
npm run dev
```

La aplicación queda disponible en `http://localhost:5173`.

### 5. Iniciar sesión

El sistema viene con usuarios de demostración para cada rol, listos para usar sin necesidad de registrar cuentas nuevas:

| Rol | Correo | Contraseña |
|---|---|---|
| Super administrador | `samin@barberos.com` | `Pitch2026!` |
| Administrador | `admin.pitch@barberos.com` | `Pitch2026!` |
| Barbero | `barbero.pitch@barberos.com` | `Pitch2026!` |
| Cliente | `cliente.pitch@barberos.com` | `Pitch2026!` |

Cada cuenta tiene datos precargados (citas, pagos, historial) para que las pantallas de métricas, agenda y reservas se vean completas desde el primer ingreso.

## Estructura del repositorio

```
BarberOS/
├── BarberOS.slnx                  Solución (.NET 10, formato XML)
├── docker-compose.yml             Definición del contenedor de PostgreSQL
├── BarberOS.Domain/               Entidades y reglas de negocio puras
├── BarberOS.Application/          Casos de uso, DTOs, puertos
├── BarberOS.Infrastructure/       EF Core, repositorios, seeding, seguridad
├── BarberOS.Api/                  Controllers y configuración de la API
└── frontend/                      Aplicación React
    ├── src/
    └── .env.example
```

## Endpoints principales

La documentación completa e interactiva de todos los endpoints está disponible en Swagger una vez el backend está corriendo (`/swagger`). Los módulos expuestos son: autenticación, barberías, usuarios, barberos, servicios, citas, pagos y métricas.

## Notas

- El proyecto usa **soft delete** en todas las entidades con historial — nada se borra físicamente de la base de datos.
- Los enums (`Role`, `AppointmentStatus`, `PaymentMethod`, etc.) se serializan como texto legible (`"Confirmed"`, no `1`) en todas las respuestas de la API.
- Las fechas y horas de las citas representan la hora local del negocio, no UTC — solo los timestamps de auditoría (creación, actualización) están en UTC.
