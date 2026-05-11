Analyze the backend codebase and generate a capability report.  
List all data models, API endpoints, utilities, and workflows.  
Explain what each one does and how complete it is.  
Identify any missing pieces or unused code.


# Backend Capability Report

## Scope
This report covers the backend in `LuxfordPTAWeb/` plus shared backend contracts in `LuxfordPTAWeb.Shared/` (models/DTOs/enums/services used by APIs).

---

## Architecture Summary
- **Host/API:** ASP.NET Core (.NET 9) in `LuxfordPTAWeb`.
- **Persistence:** EF Core + SQL Server via `ApplicationDbContext`.
- **Auth:** ASP.NET Core Identity with roles (`Admin`, `BoardMember`, `Volunteer`).
- **API Style:** Controller-based REST endpoints under `api/*`.
- **Cross-cutting services:** audit, permissions, backup, email, OAuth2 SMTP.

---

## Data Models (Domain + Identity)

> Source: `LuxfordPTAWeb.Shared/Models/*`, `ApplicationDbContext` registrations.

| Model | Purpose | Completeness |
|---|---|---|
| `ApplicationUser` | Identity user with PTA-specific profile fields (name, metadata used by board/event assignment). | **High** |
| `SchoolYear` | Defines school-year boundaries/status and relationships to events/board/sponsors. | **High** |
| `Event` | Core event aggregate (status, coordinator, timing, approvals, copy lineage, markdown/content fields, sponsorship links). | **High** |
| `EventDay` | Multi-day event child entity with per-day schedule details. | **High** |
| `EventCat` | Event category taxonomy and display/permission behavior. | **High** |
| `EventCatSub` | Subcategory taxonomy tied to parent categories and ordering. | **High** |
| `Sponsor` | Sponsor profile/entity for display and event associations. | **High** |
| `SponsorLevel` | Sponsorship tier metadata (including amount). | **Medium** (see precision note below) |
| `SponsorAssignment` *(declared in `SponsorLevel.cs`)* | Assigns sponsor + level to school year/event context. | **High** |
| `EventMainSponsor` | Join table for primary sponsors on events. | **High** |
| `EventOtherSponsor` | Join table for secondary sponsors on events. | **High** |
| `BoardPositionTitle` | Master list of board/committee roles and sort/election metadata. | **High** |
| `BoardPosition` | School-year role assignment to users (public + admin board views). | **High** |
| `ProgramCard` | Program content entity (title, route, markdown body, media links, active/display flags). | **High** |
| `BugReport` | Bug intake and resolution tracking entity. | **High** |
| `GoogleOAuthToken` | Stores OAuth token payload for Gmail/OAuth email flow. | **Medium** (feature partially enabled in runtime config) |

### EF Core Relationship/Mapping Notes
- `ApplicationDbContext` configures many key relationships and enum conversions (events, board, sponsorship joins).
- Useful indexes exist for `Event.Slug`, `(SchoolYearId, Status)`, and unique `(EventId, DayNumber)` for `EventDay`.

---

## API Endpoints Inventory

> Base route pattern is `api/[controller]`.

### `BackupController`
- Backup management endpoints (not fully enumerated in this report extraction, but tied to backup services).
- **Completeness:** **Medium** (functional service exists; depends on hosting/storage policy).

### `BoardPositionsController`
- `GET`, board listing/public variants, plus assignment/override write endpoints.
- Protected writes for Admin/BoardMember roles.
- **Completeness:** **High**

### `BugReportsController`
- `GET /api/bugreports`
- `GET /api/bugreports/count`
- `GET /api/bugreports/all`
- `POST /api/bugreports`
- `PUT /api/bugreports/{id}` *(Admin)*
- `PUT /api/bugreports/{id}/resolve` *(Admin)*
- **Completeness:** **High**

### `EventCatController`
- Full category management + ordering:
  - `GET`, `GET {id}`
  - `POST`, `PUT {id}`, `DELETE {id}` *(Admin/BoardMember)*
  - `POST {id}/move-up`, `POST {id}/move-down`
- **Completeness:** **High**

### `EventCatSubController`
- Full subcategory management + ordering:
  - `GET`, `GET {id}`, `GET by-category/{categoryId}`
  - `POST`, `PUT {id}`, `DELETE {id}`
  - `POST {id}/move-up`, `POST {id}/move-down`
- **Completeness:** **High**

### `EventDayController` *(Admin/BoardMember protected controller)*
- Multi-day CRUD/copy endpoints:
  - `GET`, `GET {dayId}`
  - `POST`, `PUT {dayId}`, `DELETE {dayId}`
  - `POST {dayId}/copy`
- **Completeness:** **High**

### `EventsController`
- Public/event listing + rich admin/event lifecycle API:
  - `GET`, `GET {id}`, `GET by-slug/{slug}`
  - dashboard summaries (`dashboard-summary`, `dashboard-summary/{schoolYearId}`)
  - `POST`, `PUT {id}`, `DELETE {id}`
  - copy flow (`POST {id}/copy`, `GET available-for-copy`)
  - public filters (`upcoming`, `by-type/{eventcatid}`, `by-category/{slug}`, `by-school-year/{schoolYearId}`)
  - coordinator view (`GET {id}/coordinator`)
  - admin-all listing (`GET all-admin`)
  - approval flow (`POST {id}/approve`)
- **Completeness:** **High**

### `FileUploadController` *(Admin/BoardMember protected controller)*
- `POST upload-image`
- `POST upload-pdf`
- `DELETE delete`
- Validates file types/sizes and writes to `wwwroot` paths.
- **Completeness:** **High**

### `MarkdownController`
- `POST to-html` for markdown-to-HTML conversion.
- Used by Program detail rendering workflow.
- **Completeness:** **High**

### `OAuth2Controller` *(Admin protected)*
- `GET authorize`
- `GET callback`
- `GET status`
- `POST revoke`
- **Completeness:** **Medium** (OAuth runtime setup in `Program.cs` is currently commented/disabled).

### `ProgramCardsController`
- `GET /api/programcards`
- `GET /api/programcards/slug/{slug}`
- `GET /api/programcards/{id}`
- `POST /api/programcards` *(Admin/BoardMember)*
- `PUT /api/programcards/{id}` *(Admin/BoardMember)*
- `DELETE /api/programcards/{id}` *(Admin)*
- `PATCH /api/programcards/{id}/toggle-active` *(Admin/BoardMember)*
- **Completeness:** **High**

### `SchoolYearsController`
- `GET`, `GET current`, `GET last`, `GET next`, `GET {id}`
- `POST`, `PUT {id}`, `DELETE {id}` *(Admin/BoardMember)*
- `POST {id}/transition` *(Admin/BoardMember)*
- **Completeness:** **Medium-High** (transition endpoint exists; year-end automation TODO remains)

### `UsersController`
- `GET /api/users`
- `GET /api/users/coordinators` *(Admin/BoardMember)*
- `POST /api/users` *(Admin/BoardMember)*
- **Completeness:** **Medium** (core retrieval exists; broader role/user admin workflows are still partial in UI)

---

## Backend Utilities / Services

| Service | Purpose | Completeness |
|---|---|---|
| `AuditService` | Tracks entity change metadata and audit history workflows. | **High** |
| `EventPermissionService` | Centralizes category/role-based event edit permissions. | **High** |
| `DatabaseBackupService` | Creates database backup artifacts and backup operations. | **Medium-High** |
| `ScheduledBackupService` | Hosted background scheduler for periodic backups. | **Medium-High** |
| `EmailSenderService` | Application-level outbound email sending. | **Medium** |
| `IdentityEmailSender` | Identity email adapter implementation. | **High** |
| `GoogleSmtpOAuthService` | OAuth-based SMTP token and Gmail auth workflow service. | **Medium** (depends on OAuth setup status) |

---

## Core Workflows (Backend)

### 1) Authentication + Role Authorization
- Identity-based auth with role checks on protected endpoints (`Admin`, `BoardMember`, `Trusted`, etc.).
- **Status:** **High**, but external Google OAuth login is currently disabled in startup config.

### 2) Event Lifecycle Management
- Create/update/delete events, category/subcategory assignment, copy from templates, coordinator assignment, approval, and school-year filtering.
- **Status:** **High** (most complete workflow in backend).

### 3) Multi-Day Event Workflow
- Event day CRUD and copy endpoint for complex event schedules.
- **Status:** **High**.

### 4) Board Position Assignment Workflow
- Manage board titles and per-year user assignments with admin overrides/public retrieval.
- **Status:** **High**.

### 5) Program Content Workflow
- Program CRUD + active toggling + slug retrieval + markdown rendering + media uploads.
- **Status:** **High**.

### 6) File Upload Workflow
- Authorized upload pipeline for images/PDFs with extension/size validation and safe path handling.
- **Status:** **High**.

### 7) School Year Management + Transition
- CRUD and current/next/last retrieval plus transition endpoint.
- **Status:** **Medium-High** (automation/transition logic still has TODO).

### 8) Bug Reporting Workflow
- Submit, list, count, and resolve bug reports with role restrictions.
- **Status:** **High**.

### 9) Backup Workflow
- API + services + hosted scheduler for backup operations.
- **Status:** **Medium-High** (depends on ops/storage validation in target environment).

### 10) OAuth2 Email Token Workflow
- Authorization/callback/status/revoke endpoints + OAuth service support.
- **Status:** **Medium** (wiring exists, but startup external auth config is presently commented out).

---

## Missing Pieces / Risks / Potentially Unused Code

## 1) Incomplete / TODO-marked backend logic
- `SchoolYearsController`: `// TODO: Add year-end transition logic here`.
- Impact: transition endpoint exists but full rollover automation may be incomplete.

## 2) OAuth feature mismatch
- `OAuth2Controller` and `GoogleSmtpOAuthService` exist, but Google external auth block in `Program.cs` is commented as disabled.
- Impact: partial capability; admin OAuth endpoints may not align with login/runtime expectations.

## 3) CORS policy defined but likely not applied
- `builder.Services.AddCors(...)` exists in `Program.cs`, but `app.UseCors(...)` is not present in pipeline.
- Impact: if cross-origin client usage is needed, policy won’t be enforced/applied.

## 4) Seed helper likely unused in runtime startup
- `ApplicationDbContext.SeedBoardPositionTitlesAsync(...)` exists; no direct invocation found in startup path shown.
- Impact: board title seed consistency may depend on manual script/migrations.

## 5) Data model registration mismatch risk
- `DbSet<SponsorAssignments>` is registered and `SponsorAssignment` model exists (inside `SponsorLevel.cs`), but this nested location is atypical and easy to overlook.
- Impact: maintainability/readability risk, not necessarily runtime failure.

## 6) Sensitive defaults in seed routine
- `SeedRolesAndAdminUser` hardcodes admin email and password in startup logic.
- Impact: security/ops risk; should be moved to secure configuration/secrets.

---

## Completeness Snapshot

- **Strong / Production-like:** Events, Event Days, Categories/Subcategories, Board Positions, Program Cards, Bug Reports, Markdown conversion, file uploads.
- **Partially complete / operational debt:** School-year transition automation, OAuth integration wiring parity, user/admin role-management depth, backup operational validation.
- **Maintainability concerns:** hardcoded seed credentials, potential unused seed helper, CORS middleware omission.

