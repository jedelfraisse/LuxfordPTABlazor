# Luxford PTA Website — Project Map

## Repository Overview
This repository is a .NET 9 Blazor solution split into **Server**, **Client**, and **Shared** projects.  
Goal: make most public content manageable by Admin/Board members (and selected volunteers) through admin workflows.

---

## Top-Level Structure

- **`.github/`**  
  GitHub automation and workflow/config files for CI/CD and repository management.

- **`LuxfordPTAWeb/`** (Server host, API, Identity, EF Core)  
  ASP.NET Core host app that serves the Blazor app, exposes API controllers, runs auth/identity, and manages database persistence.

- **`LuxfordPTAWeb.Client/`** (Blazor UI)  
  UI project containing public pages, admin pages, layout/components, and browser-side utilities.

- **`LuxfordPTAWeb.Shared/`** (Shared contracts/domain)  
  Cross-project models, DTOs, enums, and interfaces shared between server and client.

- **`LuxfordPTAWebPublished/`**  
  Published/deployment output artifacts.

- **`OverallDev.md`**  
  Product planning notes and high-level implementation phases.

- **`LuxfordPTA_InitialSeedData.sql`**  
  Seed script for core data/bootstrap content.

- **`TINYMCE_LOCAL_SETUP.md`, `INSTALL_TINYMCE.bat`**  
  Local TinyMCE install documentation and helper script.

---

## `LuxfordPTAWeb/` (Server)

### Key Folders

- **`Components/`**  
  Server-host shell and Identity helper components for auth/account flows.

- **`Controllers/`**  
  REST API endpoints for events, school years, board positions, users, bug reports, file uploads, markdown conversion, programs, backups, and OAuth operations.

- **`Data/`**  
  EF Core `ApplicationDbContext` and database configuration root.

- **`Migrations/`**  
  EF Core schema history and migration snapshots (events, board, bugs, program cards, etc.).

- **`Services/`**  
  Business/infrastructure services (email, audit, permissions, backup, OAuth SMTP, scheduled tasks).

- **`wwwroot/`**  
  Static assets served by host (CSS, JS, images, uploaded files).

- **`Views/`**  
  Server-rendered MVC/Razor view support (minimal in this architecture).

### Major Backend Modules (files)

- **`Program.cs`**  
  App startup composition root: service registrations, middleware pipeline, authentication/authorization, and endpoint mapping.

- **`Data/ApplicationDbContext.cs`**  
  Main EF Core context for domain entities (events, school years, sponsors, board positions, program cards, bug reports, etc.).

- **`Controllers/EventsController.cs`**  
  Primary events API with filtering, school-year queries, admin operations, and event lifecycle functions.

- **`Controllers/ProgramCardsController.cs`**  
  CRUD API for program cards, slug lookup, active toggle, and admin-governed content management.

- **`Controllers/FileUploadController.cs`**  
  Handles secured image/PDF uploads to `wwwroot` folders with validation and path safety checks.

- **`Controllers/MarkdownController.cs`**  
  Converts markdown to HTML server-side (used to render rich content safely/consistently in client views).

- **`Controllers/SchoolYearsController.cs`**  
  Manages school year entities, current-year behavior, and school-year transitions.

- **`Controllers/BoardPositionsController.cs`**  
  Board role/title assignment endpoints and public/admin board listing operations.

- **`Controllers/UsersController.cs`**  
  User retrieval and admin-related user management endpoints.

- **`Controllers/BugReportsController.cs`**  
  Intake and management endpoints for bug reporting workflows.

- **`Controllers/BackupController.cs`**  
  API surface for initiating and managing backup operations.

- **`Controllers/OAuth2Controller.cs`**  
  OAuth2 integration endpoints (email/OAuth token workflows).

- **`Services/EventPermissionService.cs`**  
  Encapsulates role/rule logic around event access and modifications.

- **`Services/AuditService.cs`**  
  Tracks audit data for mutable entities and change history patterns.

- **`Services/DatabaseBackupService.cs`**, **`Services/ScheduledBackupService.cs`**  
  Backup creation and scheduled backup orchestration.

- **`Services/GoogleSmtpOAuthService.cs`**, **`Services/EmailSenderService.cs`**, **`Services/IdentityEmailSender.cs`**  
  Email delivery and authentication integration used by app and identity flows.

---

## `LuxfordPTAWeb.Client/` (Blazor UI)

### Key Folders

- **`AdminPages/`**  
  Admin-facing pages for dashboard, events, school years, programs, users, sponsors, bugs, backups, auth debugging, etc.

- **`Pages/`**  
  Public site routes: home, events, programs/program detail, sponsors, documents, givebacks, membership, privacy, and not-found.

- **`Components/`**  
  Reusable UI pieces (cookie consent, analytics initializer, TinyMCE wrapper, bug modal).

- **`Components/Admin/`**  
  Dashboard widgets and admin utility components (officers/committee chairs, event calendar summary, context header, etc.).

- **`Layout/`**  
  Main shell (`MainLayout`) and login display/navigation frame.

- **`Services/`**  
  Browser-facing services for analytics and cookie consent state handling.

- **`Code/`**  
  Shared client helper logic (notably school-year selection/state support).

- **`wwwroot/`**  
  Client static assets (page CSS and JS helpers).

### Major Client Files / Modules

- **`Program.cs`**  
  Client startup registrations and service wiring.

- **`Layout/MainLayout.razor`**  
  Global header/nav/footer frame, school-year switch UX, and shared chrome behavior.

- **`AdminPages/Dashboard.razor(.cs)`**  
  Admin landing page with quick links and widget summaries for board/event context.

- **`AdminPages/ProgramsAdmin.razor`**  
  Program CRUD UI, rich-content editing, upload integration for images/flyers, and publish/visibility controls.

- **`Pages/Programs.razor`**  
  Public program listing page (cards, links, flyer CTA).

- **`Pages/ProgramDetail.razor`**  
  Program detail route loading by slug and rendering markdown-derived HTML content.

- **`Pages/Home.razor(.cs)`**  
  Homepage composition, board member display, and event snapshot integration.

- **`Pages/Events.razor`**  
  Event browsing/filtering experience by school year and category metadata.

- **`Components/TinyMCEEditor.razor`**  
  Reusable wrapper for TinyMCE in Blazor forms.

- **`wwwroot/js/scrollToEditForm.js`**  
  UI helper script for admin edit form UX.

### Notable Client Artifacts

- **`Layout/MainLayout.razor.backup`**, **`Layout/MainLayout.razor.new`**  
  Alternate/backup layout files retained in source; likely temporary development artifacts.

---

## `LuxfordPTAWeb.Shared/` (Domain + Contracts)

### Key Folders

- **`Models/`**  
  Core domain entities shared across layers (Event, ProgramCard, SchoolYear, Sponsor, BoardPosition, BugReport, etc.).

- **`DTOs/`**  
  Request/response contracts for API operations (event create/update/copy, board assignment, dashboard summaries).

- **`Enums/`**  
  Shared enums for statuses, permissions, role types, and consent levels.

- **`Interfaces/`**  
  Cross-cutting contracts (e.g., auditable entity semantics).

- **`Services/`**  
  Shared service contracts/utilities consumed by both app layers (e.g., cookie consent abstraction, DTO projection helpers).

- **`Configuration/`**  
  Strongly typed configuration models (OAuth2, analytics options).

### Major Shared Files

- **`Models/Event.cs`**  
  Rich event aggregate root containing schedule, coordinator, status, sponsorship, audit, and multi-day support.

- **`Models/ProgramCard.cs`**  
  Program content model for public cards/details and admin editing metadata.

- **`Models/ApplicationUser.cs`**  
  Extended identity user profile used by auth and board assignment features.

- **`Models/SchoolYear.cs`**  
  School-year boundary/context model driving site-wide filtering and visibility.

- **`DTOs/CreateEventDTO.cs`, `UpdateEventDTO.cs`, `EventDashboardSummaryDTO.cs`**  
  Main event API contracts for creation, updates, and admin dashboard aggregation.

- **`Enums/EventStatus.cs`, `SchoolYearStatus.cs`**  
  Key state enums used across server/client business logic.

---

## TODOs, Unfinished Methods, and Commented-Out Code

### Confirmed TODO / Placeholder Items

- **`LuxfordPTAWeb/Controllers/SchoolYearsController.cs`** (line ~225)  
  `// TODO: Add year-end transition logic here` — school-year rollover automation not yet implemented.

- **`LuxfordPTAWeb.Client/AdminPages/UsersAdmin.razor`** (line ~65)  
  `<!-- TODO: Add role management -->` — role editing UI is not implemented.

- **`LuxfordPTAWeb.Client/AdminPages/UsersAdmin.razor`** (line ~110)  
  `// TODO: Implement user editing` — edit action currently stubbed with alert only.

- **`LuxfordPTAWeb.Client/Pages/Documents.razor.cs`** (line ~7)  
  `// TODO: Add properties and methods for document management with EF` — documents module backend integration pending.

- **`LuxfordPTAWeb.Client/Pages/Givebacks.razor`** (line ~242)  
  `// TODO: Add properties and methods for Givebacks integration with EF` — Givebacks persistence/integration pending.

- **`LuxfordPTAWeb.Client/AdminPages/SponsorsAdmin.razor`**  
  Explicit **“Coming Soon”** note indicates sponsor admin page is placeholder-level.

### Commented/Feature-Gated UI Blocks

- **`LuxfordPTAWeb.Client/Layout/MainLayout.razor`** (line ~92)  
  `@if (1==2)` block disables a Documents nav link in top navigation.

- **`LuxfordPTAWeb.Client/Pages/Documents.razor`** (line ~114)  
  Another `@if (1==2)` block gating additional document section content.

### “Coming Soon” Content Markers

- **`LuxfordPTAWeb.Client/Pages/Documents.razor`**  
  Multiple `(Coming Soon)` labels indicate partial/placeholder document resources and workflows.

- **`LuxfordPTAWeb.Client/Pages/EventCategory.razor`**  
  Contains `Coming Soon!` badge, indicating unfinished sub-feature in event category UX.

- **`LuxfordPTAWeb.Client/AdminPages/UsersAdmin.razor`**  
  Alert-based “coming soon” behavior for user edit path confirms incomplete functionality.

### SQL Script Follow-Ups

- **`LuxfordPTA_InitialSeedData.sql`** (multiple locations)  
  Contains `TODO` post-run operational reminders; script execution has manual follow-up steps.

---

## Quick Capability Snapshot by Product Area

1. **Events**: Strongly implemented (domain, API, admin pages, widgets, summaries, categories).  
2. **Get Involved**: Public page present; deeper admin/content tooling appears lighter.  
3. **Sponsors**: Public page exists; admin management currently placeholder/partial.  
4. **Documents**: Public shell exists but many marked “Coming Soon”; EF-backed management pending.  
5. **Givebacks**: Public page exists; EF/integration TODO indicates incomplete backend support.

---

## Notes for Cleanup / Hardening

- Consider removing or archiving `*.backup` / `*.new` files from source.  
- Replace `@if (1==2)` with explicit feature flags/config toggles.  
- Convert “Coming Soon” placeholders into tracked issues and implementation milestones.  
- Add centralized error display (instead of JS alerts) on admin pages for better diagnostics and UX.
