# Eventos.Web

Aplicación ASP.NET Core MVC de eventos e inscripciones con Entity Framework Core **Code First**. La base de datos `EventosDB` se crea a partir de las clases de C# mediante migraciones: no hay scripts SQL ni procedimientos almacenados, y todo el acceso a datos se hace con LINQ.

- Las entidades están en `Models/` y se configuran solo con Data Annotations (sin Fluent API).
- `Data/EventosContext.cs` tiene un `DbSet` por entidad y no sobrescribe `OnModelCreating`.
- Todas las consultas LINQ y los `SaveChangesAsync` están en `Repositorios/`; los controladores no usan el contexto.
- Eliminación lógica (`Activo = false`), patrón Post/Redirect/Get y mensajes con `TempData`.

## Requisitos

- .NET SDK 10 o superior (el proyecto usa `net10.0`)
- SQL Server Express en `.\SQLEXPRESS` con autenticación de Windows
- Visual Studio 2026

## Ejecutar

### Visual Studio

1. Abrir `Eventos.slnx`.
2. En la Consola del Administrador de paquetes, con `Eventos.Web` como proyecto predeterminado:

   ```
   Update-Database
   ```

3. Ejecutar con el perfil `http` (http://localhost:5059).

### Terminal

```bash
cd Eventos.Web
dotnet tool restore
dotnet build
dotnet ef database update
dotnet run --launch-profile http
```

`dotnet-ef` está en el manifiesto local (`Eventos.Web/dotnet-tools.json`). Si se prefiere instalarlo de forma global: `dotnet tool install --global dotnet-ef`.

Cadena de conexión (`Eventos.Web/appsettings.json`):

```
Server=.\SQLEXPRESS;Database=EventosDB;Integrated Security=True;TrustServerCertificate=True
```

## Datos iniciales

Al iniciar, `Program.cs` llama a `DatosIniciales.CargarAsync`, que inserta 3 categorías, 4 eventos y 5 asistentes, solo si cada tabla está vacía (`AnyAsync`, `AddRange` y `SaveChangesAsync`). La carga se hace únicamente cuando no hay migraciones pendientes; si falta aplicar alguna, se registra un aviso y no se inserta nada.

Las fechas de los eventos se calculan desde el día de la carga: uno ya pasó (para probar que no admite inscripciones) y la mentoría tiene aforo 4 (para probar el aforo completo).

## Migraciones

| Migración | Cambio |
|---|---|
| `InitialCreate` | Tablas `Categorias`, `Eventos`, `Asistentes` e `Inscripciones` con sus índices únicos y llaves foráneas |
| `AgregarModalidadEvento` | Columna `Eventos.Modalidad` (obligatoria, máx. 20: Presencial o Virtual) |
| `AgregarTelefonoAsistente` | Columna `Asistentes.Telefono` (opcional, máx. 9) |

En `AgregarModalidadEvento` el único cambio hecho a mano fue el valor por defecto: EF Core generó `defaultValue: ""` y se cambió a `defaultValue: "Presencial"` antes de aplicarla, para que los eventos ya registrados la conserven.

Comandos utilizados (desde `Eventos.Web`):

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update

dotnet ef migrations add AgregarModalidadEvento
dotnet ef database update

dotnet ef migrations add AgregarTelefonoAsistente
dotnet ef database update
```

En Visual Studio son `Add-Migration <Nombre>` y `Update-Database`. Están también en `scripts/comandos.txt`.

Verificación en SQL Server:

```sql
SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId;
```

Debe devolver tres filas cuyos `MigrationId` coinciden con los archivos de `Eventos.Web/Migrations`.

## Estructura

```
Lab11/
├── Eventos.slnx
├── scripts/comandos.txt
└── Eventos.Web/
    ├── Models/          Categoria, Evento, Asistente, Inscripcion
    ├── Data/            EventosContext, DatosIniciales
    ├── Migrations/      las tres migraciones y EventosContextModelSnapshot
    ├── Repositorios/    EventoRepositorio, AsistenteRepositorio, InscripcionRepositorio
    ├── ViewModels/      modelos de los listados
    ├── Controllers/     Eventos, Asistentes, Inscripciones
    └── Views/
```

## Reglas implementadas

- Eventos: el listado muestra los eventos activos con su categoría y modalidad, permite filtrar por categoría y los ordena por fecha. Para cada evento muestra los inscritos confirmados y los cupos disponibles (`Aforo` menos confirmadas). Al registrar un evento, la fecha debe ser futura. Al editarlo, el aforo no puede quedar por debajo de las inscripciones confirmadas. Eliminar un evento lo marca como inactivo y cancela sus inscripciones confirmadas.
- Asistentes: el listado muestra los activos ordenados por nombres, con búsqueda por nombre o DNI. Al registrar, si el DNI o el correo ya existen se muestra el error en el formulario. El DNI debe tener 8 dígitos y el teléfono, si se ingresa, entre 7 y 9.
- Inscripciones: no se inscribe en un evento que ya pasó, ni a un asistente que ya tiene una inscripción confirmada en ese evento, ni en un evento cuyas confirmadas ya alcanzaron el aforo. Se guardan con estado `Confirmada`. El listado se filtra por estado y permite cancelar las confirmadas (pasan a `Cancelada` y liberan el cupo).
- Las validaciones de formato están en los Data Annotations de las entidades; las que consultan la base de datos se hacen en los controladores con `ModelState.AddModelError`.
