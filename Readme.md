# LuxfordPTAWeb Project Plan

## 🏁 Project Overview
LuxfordPTAWeb is a Blazor-based portal that evolved from a static DocFX-powered site into a dynamic, interactive hub for PTA members, volunteers, and board members. The platform integrates content management, events, membership tools, and comprehensive treasurer management workflows with database-backed tracking and cloud document storage.

---

## 🎯 Goals
- **Evolved from static DocFX site to a dynamic Blazor WebAssembly portal**
- Integrate editable content from markdown or JSON sources
- Support super member perks and ticketing via GiveBacks, Zeffy, or Square
- Provide board tools for tracking events and volunteer roles (SignUp Genius)
- Enable secretary and treasurer workflows with cloud-based document storage (Google Workspace)
- **Implement comprehensive PTA treasurer management system with digital checklist tracking**

---

## 🧱 Architecture

### Solution Structure

#### LuxfordPTAWeb (Host/Server Project)
- **Purpose:** Hosts and serves the Blazor WebAssembly client app with full-stack capabilities.
- **Responsibilities:**
  - Serves static files and the client app to browsers.
  - Provides server-side APIs for authentication, data access, and treasurer management.
  - Handles configuration, dependency injection, and startup logic.
  - Manages SQL Server database operations for treasurer checklist and document tracking.
  - Integrates with Google Workspace APIs for document storage and management.
  - Implements Entity Framework Core for data persistence.
- **Data:** Stores treasurer checklist progress, user roles, financial documents metadata, and compliance tracking in SQL Server database.
- **Deployment:** This project is published and uploaded to the hosting provider (FTP or Google Cloud) as the main entry point for the site.

#### LuxfordPTAWeb.Client (Client Project)
- **Purpose:** Contains the Blazor WebAssembly app that runs in the browser.
- **Responsibilities:**
  - Implements all UI components, pages, and client-side logic.
  - Handles routing, user interactions, and calls to APIs (via HttpClient).
  - Provides treasurer dashboard and checklist management interface.
  - Runs entirely in the browser, providing a rich, interactive experience.
  - Communicates with the host/server for data and integrations.
- **Data:** Handles user interface state, calls APIs for dynamic data, and may cache data locally in the browser.
- **Deployment:** This project is built and its output is served by the LuxfordPTAWeb host project. It does not need to be published or uploaded separately.

---

## 🔄 Data & Content Flow

- **Content (Markdown/JSON):** Transitioned from static DocFX to dynamic content via Azure Blob Storage or database. Accessed by the client app via API calls.
- **Membership & Ticketing:** Managed via external APIs (GiveBacks, Zeffy, Square).
- **Event & Volunteer Data:** Synced via SignUp Genius API.
- **Board Documents:** Uploaded and stored in Google Workspace via server APIs with database metadata tracking.
- **Treasurer Documents:** Managed through Google Workspace Drive API with SQL Server tracking of completion status, deadlines, and compliance requirements.
- **Configuration:** Stored in `appsettings.json` or Azure App Configuration, typically in the host project.

---

## 💰 PTA Treasurer Management System

### Core Features
- **Digital Checklist:** Interactive monthly and seasonal task tracking based on Virginia PTA treasurer requirements
- **Document Management:** Integration with Google Workspace for storing treasurer reports, budgets, and financial documents
- **Progress Tracking:** Real-time visibility into completion status of treasurer duties with deadline reminders
- **File Upload & Organization:** Streamlined uploading of receipts, bank statements, and compliance documents
- **Role-Based Access:** Secure access for current treasurer, incoming treasurer, and board oversight
- **Database-Driven:** All checklist data, completion status, and document metadata stored in SQL Server

### Treasurer Checklist Categories
- **Monthly Tasks:** Ongoing financial record maintenance, reporting, and dues management
- **July:** Transition period activities, financial reviews, and tax filing preparations
- **August:** New fiscal year setup, budget preparation, and membership processing
- **September:** Financial review presentation and budget approval processes
- **October-March:** Continued monthly processes with budget monitoring
- **April-May:** Year-end preparation and summer budget planning
- **June:** Fiscal year closure and transition documentation

### Technical Implementation
- **Database Schema:** SQL Server with Entity Framework Core for treasurer checklist sections, items, and completion tracking
- **File Storage:** Google Workspace Drive API integration for document uploads and organization
- **User Management:** ASP.NET Core Identity with role-based permissions for treasurers and board members
- **Reporting:** Progress dashboards and completion status reports
- **Notifications:** Deadline reminders and task notifications

---

## 🛠 Publishing & Deployment

- **Publish:** Only the LuxfordPTAWeb (host/server) project needs to be published for deployment.
- **Upload:** The published output (usually the `wwwroot` folder and server binaries) should be uploaded to your hosting provider (FTP or Google Cloud).
- **Client Project:** LuxfordPTAWeb.Client is referenced and included automatically in the host project's output; no separate upload is required.
- **Database:** SQL Server database with Entity Framework migrations for treasurer management features.

---

## 🔗 Integrations

| Feature              | Platform            | Method                | Project Responsible      |
|----------------------|--------------------|-----------------------|-------------------------|
| Membership           | GiveBacks          | External link or API  | Client (via API)        |
| Ticketing            | Zeffy / Square     | Embed or redirect     | Client                  |
| Volunteer Signups    | SignUp Genius      | REST API              | Client                  |
| **Document Storage** | **Google Workspace** | **Drive API**       | **Host (via API)**      |
| **Treasurer Files**  | **Google Drive**   | **REST API + SDK**    | **Host (via API)**      |
| Content Sync         | Azure Blob         | SDK or HTTP           | Client                  |
| **Database**         | **SQL Server**     | **Entity Framework**  | **Host**                |

---

## 🧩 Component Breakdown

### Public Pages (Client)
- Home
- About PTA
- Events
- Membership
- Contact
- **Documents (Public treasurer reports and meeting minutes)**

### Member Portal (Client)
- Dashboard
- Event RSVP
- Super Member Perks
- Ticket Access

### Admin Tools (Client)
- Content Editor (Markdown/JSON)
- Event Manager
- Volunteer Tracker
- Secretary Notes
- **Treasurer Dashboard**
- **Treasurer Checklist Management**
- **Financial Document Upload**

### Treasurer-Specific Features (Client)
- **Monthly Checklist Interface**
- **Document Upload & Organization**
- **Progress Tracking Dashboard**
- **Deadline & Reminder Management**
- **Transition Planning Tools**
- **Compliance Status Monitoring**

---

## 🧪 Future Enhancements
- QR check-in for events
- Gamified volunteer badges
- Role-based access control
- Event cloning and templates
- Integration with school calendar
- **Automated budget analysis and amendment tracking**
- **Integration with MemberHub for dues processing**
- **Financial report generation and distribution**
- **Mobile app for treasurer checklist access**

---

## 🛠 Development Notes
- **Transitioned from static DocFX to dynamic Blazor WebAssembly architecture**
- Use `@rendermode` per page/component for hybrid SSR
- Avoid top-level statements for clarity and modularity
- Use `HttpClientFactory` for external API calls
- Store config in `appsettings.json` or Azure App Configuration
- **Implement Entity Framework Core for treasurer data management**
- **Use Google Workspace APIs for secure document storage**
- **Design role-based authorization for treasurer features**
- **SQL Server database for persistent data storage**

---

## 📦 Summary of Deployment

- **Publish and upload LuxfordPTAWeb (host/server) project only.**
- **LuxfordPTAWeb.Client is included automatically in the host's output.**
- **Upload published files to your hosting provider (FTP or Google Cloud) as required.**
- **Configure database connection strings and Google Workspace API credentials.**
- **Run Entity Framework migrations for treasurer management features.**


# 🐉 PTA Dragon Personas by School Year

| School Year | Reflections Theme         | Dragon Name         | Persona Description                                      |
|-------------|---------------------------|----------------------|----------------------------------------------------------|
| 2024–2025   | Accepting Imperfection     | Crinkle the Courageous 🐉 | Embraces flaws, celebrates growth, wears a patchwork cloak of “oops” moments. |
| 2025–2026   | I Belong!                  | Kinari the Welcomer 🐉 | Builds community, celebrates inclusion, connects families and voices. |
| 2026–2027   | *(TBD)*                    | Sage the Storykeeper 🐉 | Honors culture, diversity, and shared traditions (placeholder until theme is announced). |
| 2027–2028   | *(TBD)*                    | Nova the Innovator 🐉 | Champions creativity, tech, and student-led exploration (theme to be adapted). |
| 2028–2029   | *(TBD)*                    | Echo the Historian 🐉 | Archives PTA lore, preserves student art, and narrates the journey (theme to be adapted). |