# LuxfordPTAWeb Project Plan

## 🏁 Project Overview
LuxfordPTAWeb is a Blazor-based portal designed to replace the static DocFX-powered LuxfordPTADoc site. It will serve as a dynamic, branded hub for PTA members, volunteers, and board members, integrating content, events, and membership tools.

---

## 🎯 Goals
- Replace static DocFX site with a visually engaging Blazor portal
- Integrate editable content from markdown or JSON sources
- Support super member perks and ticketing via GiveBacks, Zeffy, or Square
- Provide board tools for tracking events and volunteer roles (SignUp Genius)
- Enable secretary and treasurer workflows with cloud-based document storage (Google Cloud)

---

## 🧱 Architecture

### Solution Structure

#### LuxfordPTAWeb (Host/Server Project)
- **Purpose:** Hosts and serves the Blazor WebAssembly client app.
- **Responsibilities:**
  - Serves static files and the client app to browsers.
  - May provide server-side APIs for authentication, data access, or integrations.
  - Handles configuration, dependency injection, and startup logic.
  - Can be extended for hybrid hosting (SSR, API endpoints).
- **Data:** May store configuration, secrets, and server-side logic if needed.
- **Deployment:** This project is published and uploaded to the hosting provider (FTP or Google Cloud) as the main entry point for the site.

#### LuxfordPTAWeb.Client (Client Project)
- **Purpose:** Contains the Blazor WebAssembly app that runs in the browser.
- **Responsibilities:**
  - Implements all UI components, pages, and client-side logic.
  - Handles routing, user interactions, and calls to APIs (via HttpClient).
  - Runs entirely in the browser, providing a rich, interactive experience.
  - Communicates with the host/server for data and integrations.
- **Data:** Handles user interface state, calls APIs for dynamic data, and may cache data locally in the browser.
- **Deployment:** This project is built and its output is served by the LuxfordPTAWeb host project. It does not need to be published or uploaded separately.

---

## 🔄 Data & Content Flow

- **Content (Markdown/JSON):** Stored in Azure Blob Storage. Accessed by the client app via API calls.
- **Membership & Ticketing:** Managed via external APIs (GiveBacks, Zeffy, Square).
- **Event & Volunteer Data:** Synced via SignUp Genius API.
- **Board Documents:** Uploaded and stored in Google Cloud via server or client integration.
- **Configuration:** Stored in `appsettings.json` or Azure App Configuration, typically in the host project.

---

## 🛠 Publishing & Deployment

- **Publish:** Only the LuxfordPTAWeb (host/server) project needs to be published for deployment.
- **Upload:** The published output (usually the `wwwroot` folder and server binaries) should be uploaded to your hosting provider (FTP or Google Cloud).
- **Client Project:** LuxfordPTAWeb.Client is referenced and included automatically in the host project’s output; no separate upload is required.

---

## 🔗 Integrations

| Feature              | Platform         | Method                | Project Responsible      |
|----------------------|-----------------|-----------------------|-------------------------|
| Membership           | GiveBacks       | External link or API  | Client (via API)        |
| Ticketing            | Zeffy / Square  | Embed or redirect     | Client                  |
| Volunteer Signups    | SignUp Genius   | REST API              | Client                  |
| Document Storage     | Google Cloud    | SDK or REST API       | Host/Client             |
| Content Sync         | Azure Blob      | SDK or HTTP           | Client                  |

---

## 🧩 Component Breakdown

### Public Pages (Client)
- Home
- About PTA
- Events
- Membership
- Contact

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
- Treasurer Document Upload

---

## 🧪 Future Enhancements
- QR check-in for events
- Gamified volunteer badges
- Role-based access control
- Event cloning and templates
- Integration with school calendar

---

## 🛠 Development Notes
- Use `@rendermode` per page/component for hybrid SSR
- Avoid top-level statements for clarity and modularity
- Use `HttpClientFactory` for external API calls
- Store config in `appsettings.json` or Azure App Configuration

---

## 📦 Summary of Deployment

- **Publish and upload LuxfordPTAWeb (host/server) project only.**
- **LuxfordPTAWeb.Client is included automatically in the host’s output.**
- **Upload published files to your hosting provider (FTP or Google Cloud) as required.**
