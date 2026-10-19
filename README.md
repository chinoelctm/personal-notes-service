# personal-notes.service

API del gestor de notas y documentación personal: árbol de carpetas (hasta 5 niveles), documentos con
contenido enriquecido (HTML con imágenes en Base64) y banderitas de prioridad, estadísticas del inicio y
respaldos completos, y un asistente de IA con Google Gemini (la API Key vive solo en el servidor). .NET 8 + MongoDB.

## Ejecutar

Requiere un MongoDB accesible. En local se usa el contenedor `mongodb` (usuario `root`, clave `root`,
`authSource=admin`), ya configurado en `src/PersonalNotes.Api/appsettings.json`:

```bash
docker start mongodb   # o: docker run -d --name mongodb -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=root mongo
cd src
dotnet run --project PersonalNotes.Api --launch-profile http   # http://localhost:5080/swagger
dotnet test PersonalNotes.Service.sln
```

Toda la configuración vive en `appsettings.json` (también se puede sobrescribir con variables de entorno):

| Clave | Valor por defecto | Uso |
|---|---|---|
| `ConnectionStrings:MongoUri` | `mongodb://root:root@localhost:27017/?authSource=admin` | Conexión a MongoDB |
| `ConnectionStrings:MongoDb` | `personal-notes` | Base de datos |
| `Notes:MaxFolderDepth` | `5` | Niveles máximos del árbol (mínimo 3) |
| `Notes:RecentWindowHours` | `24` | Ventana de "editados recientemente" del dashboard |
| `Gemini:ApiKey` | `""` (vacía) | API Key de Google Gemini; sin ella el asistente responde 503 |
| `Gemini:Model` | `gemini-3.8-flash` | Modelo de Gemini |
| `Gemini:BaseUrl` / `Gemini:TimeoutSeconds` | `https://generativelanguage.googleapis.com/v1beta/` / `60` | Endpoint y tiempo máximo de respuesta |
| `Cors:AllowedOrigins` | `["http://localhost:4200"]` | Orígenes del front (política `FrontCorsPolicy`) |
| `Kestrel:Limits:MaxRequestBodySize` | `52428800` (50 MB) | Documentos y respaldos con imágenes |

Al arrancar, un worker en segundo plano (`MongoIndexInitializer`) sincroniza los índices: crea los que
faltan y elimina los propios (`ix_*`/`ux_*`) que ya no se usan. Si MongoDB no responde, la API arranca
igual y reintenta con espera creciente (5 s, 10 s, … hasta 5 min); mientras tanto, los endpoints que usan
la base responden **503** y `GET /api/v1/health` informa `Degraded`.

### API Key de Gemini

1. Entrar a [Google AI Studio](https://aistudio.google.com/app/apikey) con una cuenta de Google y presionar
   **Create API key** (el plan gratuito no pide tarjeta).
2. Configurarla **sin subirla al repo** (el `appsettings.json` versionado la deja vacía):
   - En desarrollo, con *user secrets* (se guardan fuera del repo; el proyecto ya tiene `UserSecretsId`):
     `dotnet user-secrets set "Gemini:ApiKey" "<tu-clave>" --project src/PersonalNotes.Api`
   - O pegarla en `Gemini:ApiKey` de `appsettings.json` solo en tu copia local (sin hacer commit de ese cambio).
   - En Render, en la variable de entorno `Gemini__ApiKey`.
3. Verificar con `GET /api/v1/assistant/status` → `{ "configured": true, ... }`.

## Arquitectura

```
src/
├── PersonalNotes.Api             Minimal APIs (Endpoints/), Models/ (requests HTTP), GlobalExceptionHandler,
│                                 Extensions/ (DI), Workers/MongoIndexInitializer
├── PersonalNotes.Application     Commands/ y Queries/ (MediatR), validadores (FluentValidation), Behaviors/,
│                                 Services/ (árbol de carpetas, estadísticas del dashboard, prompt del asistente),
│                                 DTOs/, Helpers/
├── PersonalNotes.Infrastructure  Mongo: DbContext + índices, repositorios, opciones, salud de la base;
│                                 GeminiAssistant (cliente HTTP de Gemini)
└── PersonalNotes.Domain          Entidades (Folder, Document), value objects (Priority, DocumentStamp),
                                  excepciones, contratos de repositorio y servicios
tests/PersonalNotes.Tests         Dominio + servicios de cálculo de Application
```

Dependencias: `Api → Application → Domain` e `Infrastructure → Domain` (la Api referencia Infrastructure
solo para registrar sus servicios).

## Modelo en Mongo

| Colección | Contenido |
|---|---|
| `folders` | Carpetas: `name`, `normalizedName`, `parentFolderId` (null = raíz), `enabled`, `createdOn`, `updatedOn` |
| `documents` | Documentos: `folderId`, `title`, `content` (HTML), `priority` (`None`/`Low`/`Medium`/`High`), `enabled`, `createdOn`, `updatedOn` |

Los Id son `ObjectId` y viajan como texto de 24 caracteres. Las fechas de auditoría se asignan en el
backend y siempre en UTC (`TimeHelper.GetUtcNow()`); el front las muestra en la hora local.

### Índices

Cada consulta de los repositorios se validó con `explain()`: todas usan índice (sin `COLLSCAN`) y
ninguna ordena en memoria (sin etapa `SORT`).

| Colección | Índice | Claves | Consultas que resuelve |
|---|---|---|---|
| `folders` | `ux_folders_parent_name` | `parentFolderId, normalizedName` (único, parcial `enabled: true`) | Nombre duplicado entre carpetas hermanas activas (también en la raíz, `parentFolderId: null`) |
| `folders` | `ix_folders_active_name` | `enabled, normalizedName` | Carpetas activas ordenadas por nombre; todas para el respaldo (mismo orden) |
| `documents` | `ix_documents_active_updated` | `enabled, updatedOn ↓` | Documentos activos por última edición; proyección liviana para estadísticas; respaldo |
| `documents` | `ix_documents_folder_updated` | `folderId, enabled, updatedOn ↓` | Documentos de una carpeta por última edición; borrado en cascada (`folderId $in`) |

`GetById` y la desactivación de carpetas (`_id $in`) usan el índice `_id_`.

## Reglas de negocio

- **Árbol**: una carpeta raíz es el nivel 1; se permiten hasta `Notes:MaxFolderDepth` niveles (5 por
  defecto, mínimo 3). Mover una carpeta cuenta la altura de todo su subárbol y no se puede mover dentro de
  sí misma ni de sus subcarpetas (400).
- **Nombres**: no puede haber dos carpetas activas con el mismo nombre en la misma ubicación (409; el índice
  único lo garantiza también ante altas simultáneas).
- **Borrado lógico**: eliminar una carpeta desactiva (`enabled: false`) la carpeta, todas sus subcarpetas
  y sus documentos; eliminar un documento lo desactiva. Los respaldos incluyen los elementos desactivados.
- **Documentos**: título vacío se publica como "Sin título" (máx. 200 caracteres); el contenido admite
  hasta 12 millones de caracteres (las imágenes van en Base64 dentro del HTML). Solo se crean o mueven a
  carpetas activas.
- **Fechas**: `createdOn` y `updatedOn` en UTC; `updatedOn` siempre tiene valor (al crear es igual a
  `createdOn`) porque ordena los listados por última edición.
- **Estadísticas del dashboard** (`DashboardCalculator`): total de notas y carpetas, desglose por prioridad
  y editadas en las últimas `Notes:RecentWindowHours` horas.
- **Asistente sobre tus notas** (`NotesKnowledgeBuilder`): en cada pregunta la API arma una base de conocimiento con el
  catálogo de todos los documentos (título, carpeta, prioridad, fecha) y el contenido en texto plano de los más
  relevantes para la pregunta (coincidencia de palabras sin tildes, el título pesa más; sin coincidencias van los
  editados más recientemente), con un tope de 40.000 caracteres. Gemini responde citando los documentos y la API
  devuelve las fuentes. `searchNotes: false` desactiva la búsqueda.
- **Respaldo**: `POST /backup` valida todo el archivo (Id repetidos, padres inexistentes, ciclos,
  profundidad, documentos huérfanos) antes de reemplazar los datos.

## Endpoints (`/api/v1`)

| Método | Ruta | Respuesta |
|---|---|---|
| GET    | `/health` | `{ status, database }`: 200 `Healthy` o 503 `Degraded` si MongoDB no responde |
| GET    | `/folders` | Carpetas activas (lista plana): `id, name, parentFolderId, depth, path[], documentCount, createdOn, updatedOn` |
| POST   | `/folders` | 201 `{ id }`, body `{ "name": "Sprints Q3", "parentFolderId": "…" \| null }` |
| PUT    | `/folders/{id}` | 204, renombra y/o mueve (400 ciclo/profundidad, 409 nombre repetido) |
| DELETE | `/folders/{id}` | 204, desactiva la carpeta, sus subcarpetas y sus documentos |
| GET    | `/folders/{id}/documents` | Documentos activos de la carpeta, el editado más recientemente primero |
| GET    | `/documents` | Todos los documentos activos con contenido, por última edición |
| GET    | `/documents/{id}` | Documento activo |
| POST   | `/documents` | 201 `{ id, updatedOn }`, body `{ "folderId", "title", "content", "priority": "High" }` |
| PUT    | `/documents/{id}` | 200 `{ id, updatedOn }` (publica la nueva versión; otro `folderId` lo mueve) |
| DELETE | `/documents/{id}` | 204, lo desactiva |
| GET    | `/dashboard` | `{ totalDocuments, totalFolders, byPriority: { high, medium, low, none }, recentlyEdited, recentWindowHours, lastEditedOn }` |
| GET    | `/assistant/status` | `{ configured, provider, model }` (nunca devuelve la clave) |
| POST   | `/assistant/ask` | `{ answer, model, sources: [{ id, title, path }] }`, body `{ question, searchNotes: true, context: { title, text, documentId } | null, history: [{ role: User|Assistant, text }] }`. 503 sin clave, 429 límite, 502 error de Gemini |
| GET    | `/backup` | Respaldo completo `{ version, exportedOn, folders[], documents[] }` |
| POST   | `/backup` | 200 `{ folders, documents }`, reemplaza todo por el respaldo enviado |

Errores: `ProblemDetails` (400 validación/regla con `errors` por campo, 404, 409, 429/502/503 del asistente,
503 si la base no está disponible, 500). Los enums viajan como texto (`"High"`) y las fechas en ISO 8601 UTC.

## Despliegue (gratis): Render + MongoDB Atlas

1. **MongoDB Atlas** (base de datos):
   - Crear un cluster **M0 (Free)**.
   - *Database Access*: crear un usuario con contraseña.
   - *Network Access*: agregar `0.0.0.0/0` (Render no tiene IP fija).
   - *Connect → Drivers*: copiar la cadena `mongodb+srv://usuario:clave@cluster.xxxxx.mongodb.net/...`.
2. **Render** (API): **New → Blueprint** y elegir este repo. El `render.yaml` crea un Web Service gratuito
   con el `Dockerfile` (multi-stage `sdk:8.0` → `aspnet:8.0`, puerto 8080) y `healthCheckPath: /api/v1/health`.
   Render pedirá los valores de `ConnectionStrings__MongoUri` (cadena de Atlas) y `Gemini__ApiKey`.
3. Probar `https://personal-notes-service-hgni.onrender.com/api/v1/health`.

Las variables de entorno de Render reemplazan a `appsettings.json` (`PORT=8080`,
`ConnectionStrings__MongoUri`, `ConnectionStrings__MongoDb=personal-notes`, `Gemini__ApiKey`,
`Cors__AllowedOrigins__0=https://personal-notes-front.onrender.com`). Si el front queda en otra URL,
cambiar `Cors__AllowedOrigins__0` en el dashboard de Render.

El plan gratuito de Render apaga la API tras 15 minutos sin tráfico; la primera llamada después tarda
cerca de un minuto (el front muestra mientras tanto su copia local). Atlas M0 no tiene respaldos
automáticos: usar *Exportar Respaldo* en la app.

## Front

[personal-notes-front](https://github.com/chinoelctm/personal-notes-front) consume esta API. Los orígenes
permitidos se configuran en `Cors:AllowedOrigins` de `appsettings.json` (por defecto `http://localhost:4200`).

## Limitaciones conocidas

- `POST /backup` y el borrado en cascada no son transaccionales (Mongo standalone no soporta
  transacciones); el respaldo se valida completo antes de escribir para no dejar datos a medias.
