# 🎪 Luxford PTA Event Management System - Development Plan

## 📋 Project Overview
Building a comprehensive event management system for the Luxford PTA that handles event creation, volunteer coordination, and public display of events with integration points for SignUpGenius, Givebacks membership, and Google Cloud services. Newsletters are out of scope; this plan focuses on events only.

## 🎯 Project Phases

### 📊 **Phase 1: Foundation & Basic CRUD** ⏱️ *Week 1-2*
**Status: 🟢 85% Complete**  

#### 1.1 Enhanced Event Model & Database
- [x] **Event Model Enhancement** *(COMPLETED)*
  - [x] Add EventCoordinatorId (FK to ApplicationUser)
  - [x] **UPDATED** EventStatus enum (Planning, SubmittedForApproval, Active, InProgress, WrapUp, Completed, Cancelled)
  - [x] Add timing fields (SetupStartTime, EventStartTime, EventEndTime, CleanupEndTime)
  - [x] Add capacity fields (MaxAttendees, EstimatedAttendees)
  - [x] Add volunteer flags (RequiresVolunteers, RequiresSetup, RequiresCleanup)
  - [x] Add Notes (internal) and PublicInstructions fields
  - [x] Add WeatherBackupPlan field
  - [x] Add ExcelImportId for volunteer tracking
  - [x] Add approval tracking fields (ApprovedBy, ApprovedDate, ApprovalNotes)
  - [x] Multi-day event support (IsMultiDay flag, EventDays collection)
  - [x] Add audit tracking fields (CreatedBy, CreatedOn, LastEditedBy, LastEditedOn, ChangeNotes)

#### 1.1.1 **Multi-Day Event System** *(COMPLETED)*
- [x] **EventDay Model** *(COMPLETED)*
  - [x] EventDayId (PK) - using Id as primary key
  - [x] EventId (FK to Event)
  - [x] DayNumber (1, 2, 3, etc.)
  - [x] Date
  - [x] DayTitle (e.g., "Kick-off Day", "Fire Safety Demo", "Fire Truck Visit")
  - [x] Description (day-specific activities)
  - [x] Location (can be different per day)
  - [x] StartTime, EndTime
  - [x] IsActive (can disable specific days)
  - [x] SpecialInstructions
  - [x] MaxAttendees (day-specific capacity)
  - [x] EstimatedAttendees (day-specific estimates)
  - [x] WeatherBackupPlan (day-specific backup plans)

- [x] **Multi-Day Event UI Components** *(COMPLETED)*
  - [x] Multi-day toggle in EventsCreate.razor
  - [x] Day management interface (add/edit/delete days)
  - [x] Primary day + additional days interface with time management
  - [x] All-day event toggle per day
  - [x] Day-specific fields (title, description, location, instructions)
  - [x] Multi-day event display in admin tables with day count badges
  - [x] Date range display for multi-day events
  - [ ] Calendar view showing all days of multi-day events *(Future Phase)*
  - [ ] Public display showing event overview + individual days *(Future Phase)*
  - [ ] Day-specific volunteer coordination *(Future Phase)*

#### 1.2 Basic Event CRUD Operations
- [x] **EventsController Updates** *(COMPLETED)*
  - [x] POST /api/events (Create) - Auto-submit for approval, support multi-day
  - [x] PUT /api/events/{id} (Update) - Support multi-day updates via audit service
  - [x] GET /api/events/{id} (Single event details) - Include EventDays
  - [x] Event ownership validation (coordinators can only edit their events)
  - [x] Approval workflow endpoints (approve, reject, request changes)
  - [x] Multi-day specific validation and management

#### 1.2.1 **EventDay CRUD Operations** *(COMPLETED)*
- [x] **EventDayController** *(COMPLETED)*
  - [x] POST /api/events/{eventId}/days (Add day to event)
  - [x] PUT /api/events/{eventId}/days/{dayId} (Update day)
  - [x] DELETE /api/events/{eventId}/days/{dayId} (Remove day)
  - [x] GET /api/events/{eventId}/days (Get all days for event)
  - [x] GET /api/events/{eventId}/days/{dayId} (Get specific day)
  - [x] POST /api/events/{eventId}/days/{dayId}/copy (Copy day to another event)
  - [x] Permission controls (Admin, BoardMember, Event Coordinator)
  - [x] Day number validation and auto-assignment

#### 1.3 Enhanced Create/Edit Forms
- [x] **EventsCreate.razor (ENHANCED)** *(COMPLETED)*
  - [x] Enhanced primary event day interface with date/time management
  - [x] Multi-day event toggle and day management interface
  - [x] All-day event toggle with time range fallbacks
  - [x] Day-specific fields (title, description, location, special instructions)
  - [x] Add/remove additional days with validation
  - [x] Event summary panel showing total days, date range, status
  - [x] Form validation for required fields
  - [x] Better category/subcategory selection with filtering
  - [ ] **NEW** Event type templates (recurring events like Fire Prevention Week) *(Phase 2)*
  - [ ] **NEW** Smart defaults based on event category/subcategory *(Phase 2)*
  - [x] **NEW** Copy to New Event functionality that creates an editable draft from an existing event *(Phase 2)*
  - [ ] **NEW** Bulk event creation for series (e.g., weekly reading programs) *(Phase 2)*
  
- [x] **EventsEdit.razor (ENHANCED)** *(COMPLETED)*
  - [x] Basic event editing with pre-populated data
  - [x] Multi-day management interface for existing events
  - [x] Access control (coordinators vs admins)
  - [x] Approval status display and actions
  - [x] Event history/audit trail display
  - [x] Day-specific editing for multi-day events

#### 1.3.1 **Enhanced Admin Category Management** *(COMPLETED)*
- [x] **EventCategoryAdmin.razor (ENHANCED)** *(COMPLETED)*
  - [x] Better subcategory management with display ordering
  - [x] Category and subcategory CRUD operations
  - [x] Active/inactive status management
  - [ ] Category templates and defaults *(Phase 2)*
  - [ ] Bulk category operations *(Phase 2)*
  - [ ] Category usage analytics *(Phase 2)*

- [ ] **Event Template System** *(PLANNED - Phase 2)*
  - [x] EventTemplate model for recurring events
  - [ ] Template categories (Annual Events, Weekly Programs, etc.)
  - [ ] Pre-filled templates for common events
  - [ ] Template sharing between school years

#### 1.4 File Storage Setup
- [x] **Local File Storage** *(COMPLETED)*
  - [x] Create `wwwroot/events/{eventId}/` folder structure
  - [x] Image upload functionality for event photos
  - [x] Document upload for forms/flyers
  - [x] File management API endpoints
  - [ ] **NEW** Day-specific file uploads for multi-day events *(Phase 2)*

#### 1.5 Event Template/Instance System (ENHANCED)
- [x] **Event Copy & Template Relationships** *(COMPLETED)*
  - [x] Add `Slug` property to Event model for URL generation
  - [x] Add `SourceEventId` (FK to source event) to Event model
  - [x] Add `CopyGeneration` int to Event model
  - [x] Helper methods for copy relationships

- [x] **Enhanced Copy Functionality** *(COMPLETED)*
  - [x] API endpoint: `POST /api/events/{id}/copy` with multi-day support
  - [x] API endpoint: `GET /api/events/available-for-copy`
  - [x] Multi-day event copying (copy all days with date offset)
  - [x] Preserve relationships between original and copies
  - [x] "Copy to New Event" in EventsCreate.razor *(Phase 2)*
  - [x] "Copy to New Event" button in EventsAdmin.razor *(Phase 2)*
  - [ ] Template-based copying for recurring annual events including stations and other copyable components *(Phase 2)*

- [x] **Smart Event Resolution (ENHANCED)** *(COMPLETED)*
  - [x] `GET /api/events/by-slug/{slug}` - Smart event instance resolution
  - [x] Priority logic: Active/InProgress → Most Recent Completed → Planning
  - [x] Include all related instances in response
  - [x] Query parameters: `?instance=2024`, `?showAll=true`
  - [x] Multi-day event aggregation in responses

- [ ] **EventDetail.razor (ENHANCED)** *(PLANNED - Phase 2)*
  - [ ] Individual event page: `/events/{slug}`
  - [ ] Show primary event with full details
  - [ ] Multi-day event timeline view
  - [ ] Day-specific registration/volunteer signup links
  - [ ] List other instances/copies with key metrics
  - [ ] Template information and "based on" relationships

#### 1.6 School Year Navigation System (ENHANCED)
- [x] **Dynamic School Year Selector** *(COMPLETED)*
  - [x] Replace static calendar download with year navigation
  - [x] API endpoint: `GET /api/events/school-years`
  - [x] API endpoint: `GET /api/events/by-school-year/{year}`
  - [x] Previous/Current/Next year navigation buttons
  - [x] Multi-day event filtering by school year

- [x] **Enhanced Events.razor** *(COMPLETED)*
  - [x] School year selector in header
  - [x] Dynamic calendar download links
  - [x] Filter all events by selected school year
  - [x] Planning year visibility (draft/pending events)
  - [x] Multi-day event display options

#### 1.7 Approval Workflow System (ENHANCED)
- [x] **Enhanced EventStatus Enum** *(COMPLETED)*
  - [x] Planning (0) - Initial creation state
  - [x] SubmittedForApproval (1) - Submitted to board/principal
  - [x] Active (2) - Approved and public
  - [x] InProgress (3) - Event currently happening
  - [x] WrapUp (4) - Event finished, collecting feedback/metrics
  - [x] Completed (5) - Event fully closed out
  - [x] Cancelled (6) - Event cancelled or not approved

- [x] **Approval Workflow API (ENHANCED)** *(COMPLETED)*
  - [x] `POST /api/events/{id}/submit-for-approval`
  - [x] `POST /api/events/{id}/approve` (Admin/BoardMember only)
  - [x] `POST /api/events/{id}/reject` (Admin/BoardMember only) *(Basic implementation)*
  - [x] `POST /api/events/{id}/request-changes` (Admin/BoardMember only) *(Basic implementation)*
  - [x] `GET /api/events/pending-approval` (Admin/BoardMember only) *(Via dashboard)*
  - [x] Multi-day event approval (all days approved together)

- [x] **Approval UI Components (ENHANCED)** *(COMPLETED)*
  - [x] Approval status badges and indicators
  - [x] Board member approval dashboard with filtering
  - [x] Event coordinator audit tracking
  - [x] Approval history tracking via audit fields
  - [x] Multi-day event approval interface (unified approval)
  - [x] Quick filter buttons for pending approval events

#### 1.8 **Enhanced Admin Interface** *(COMPLETED)*
- [x] **EventsAdmin.razor Enhancements** *(COMPLETED)*
  - [x] Advanced filtering by school year, category, subcategory
  - [x] Quick filter buttons (All, Pending Approval, Need Volunteers, Upcoming, Active, Planning)
  - [x] Multi-day event indicators with day count badges
  - [x] Date range display for multi-day events
  - [x] Event summary statistics with real-time counts
  - [x] Clear filters and current year shortcuts
  - [x] Enhanced event display with audit information
  - [x] Creator and last modified tracking display

- [x] **Dashboard Integration** *(COMPLETED)*
  - [x] Event dashboard summary with statistics
  - [x] Multi-day event analytics in dashboard
  - [x] School year filtering for dashboard statistics
  - [x] Needs attention alerts for pending approvals

### 🎨 **Phase 2: Event Planning Components** ⏱️ *Week 3-4*
**Status: ⏳ Planned**

#### 2.1 Enhanced Event Creation & Management (IMMEDIATE PRIORITY)
- [ ] **Enhanced EventsEdit.razor** *(HIGH PRIORITY)*
  - [ ] Complete multi-day event editing interface
  - [ ] Day management for existing multi-day events
  - [ ] Approval workflow status display and actions
  - [ ] Event history/audit trail viewer
  - [ ] Better form validation and error handling
  - [ ] **NEW** Use TinyMCE editor with markdown-only storage (no HTML) for multi-line text fields (Event Description and similar)
  - [ ] **NEW** Update public pages to render markdown from those fields consistently

- [ ] **Copy to New Event Feature** *(HIGH PRIORITY)*
  - [x] "Copy to New Event" in EventsCreate.razor creates a new editable draft prefilled from the source event
  - [x] "Copy to New Event" button in EventsAdmin.razor for quick draft creation
  - [x] New copied events default to draft (`Planning`) and stay editable after creation
  - [ ] Multi-day event copying options (all days, specific days, etc.)
  - [ ] Template-based event creation for recurring events, including stations and other copyable components from prior events

- [ ] **Event Templates System** *(MEDIUM PRIORITY)*
  - [x] EventTemplate model for recurring annual events
  - [x] Template categories (Fire Prevention Week, Book Fair, Spirit Week, etc.)
  - [x] Pre-filled templates with default settings
  - [x] Template source-event linkage to copy day/timing structure from a prior event
  - [ ] Include stations and other copyable event components from prior events
  - [x] Template management interface for admins

- [x] **Event Details & Flyer Enhancements** *(MEDIUM PRIORITY)*
  - [x] Add "More Details" controller/page flow for extended event information
  - [x] Add optional event flyer upload/attachment support
  - [x] Show flyer/details link on event pages when either details or flyer exists

#### 2.2 Station Management System (ENHANCED)
- [ ] **EventStation Model**
  - [ ] StationId (PK)
  - [ ] EventId (FK to Event)
  - [ ] **NEW** EventDayId (FK to EventDay, nullable for single-day events)
  - [ ] StationType (enum: Game, Food, Activity, etc.)
  - [ ] Coordinates/Location on map
  - [ ] Capacity/Size
  - [ ] RequiredResources (FK to Resource)
  - [ ] SetupInstructions, CleanupInstructions
  - [ ] SignupGeniusLink, GivebacksLink

- [ ] **Station CRUD Operations (ENHANCED)**
  - [ ] Add/Edit/Delete stations within events
  - [ ] **NEW** Day-specific stations for multi-day events
  - [ ] Drag-and-drop reordering
  - [ ] Station templates (Ring Toss, Duck Pond, Food, etc.)
  - [ ] Copy stations from source events
  - [ ] **NEW** Copy stations across days within multi-day events

#### 2.3 Schedule/Timeline Builder (ENHANCED)
- [ ] **EventScheduleItem Model**
  - [ ] ScheduleItemId (PK)
  - [ ] EventId (FK to Event)
  - [ ] **NEW** EventDayId (FK to EventDay, nullable)
  - [ ] StationId (FK to EventStation)
  - [ ] TimeSlot (enum: Setup, EventStart, Cleanup)
  - [ ] Duration
  - [ ] AssignedTo (FK to ApplicationUser)
  - [ ] Notes

- [ ] **Schedule CRUD Operations (ENHANCED)**
  - [ ] Add/Edit/Delete schedule items
  - [ ] **NEW** Multi-day schedule management
  - [ ] Drag-and-drop reordering
  - [ ] Auto-generate schedule from event template
  - [ ] Assign volunteers to schedule items
  - [ ] **NEW** Schedule Management UI
    - [ ] Visual timeline builder with multi-day support
    - [ ] Conflict detection across multiple days
    - [ ] Schedule templates for recurring events
    - [ ] Copy schedule from source events

#### 2.4 Rules & Guidelines System (ENHANCED)
- [ ] **EventRule Model**
  - [ ] RuleId (PK)
  - [ ] EventId (FK to Event)
  - [ ] **NEW** EventDayId (FK to EventDay, nullable)
  - [ ] Description
  - [ ] AppliesTo (enum: Volunteers, Attendees, Both)
  - [ ] IsActive
  - [ ] CreatedDate
  - [ ] UpdatedDate
  - [ ] RuleTemplates (FK to global rule templates)
  - [ ] SequenceOrder

- [ ] **Rule Management UI (ENHANCED)**
  - [ ] Safety requirements
  - [ ] Age restrictions  
  - [ ] Weather policies
  - [ ] **NEW** Day-specific rules for multi-day events
  - [ ] Rule templates
  - [ ] Copy rules from source events

# 🛰️ **Phase 2.5: Real-Time Event Controls & Displays**  
⏱️ *NEW – After Phase 2, Before Phase 3*

## 📡 Overview
To support interactive events such as Talent Show, Bingo Night, Elections, PTA Meetings, and Breakfast Events (Muffins With Moms / Donuts With Dad), the system introduces a modular real-time control framework. This framework integrates with the existing Event model and multi-day architecture without replacing or conflicting with any existing features.

This system adds:

- Attachable Event Controls (modular functional units)  
- **Director Controls** (one per event)  
- Controller Mode for event operators (admin-only)  
- Display Mode for public screens (no login required)  
- SignalR-based real-time communication between controller and displays  
- Role-based displays (host, backstage, performer, audience)  
- Live/Test modes for rehearsals vs. live events  

---

## 🧩 **2.5.1 Event Controls System (NEW)**

### **EventControl Model (NEW)**

- [ ] Add `EventControl` model  
  - [ ] `EventId` (FK to Event)  
  - [ ] `ControlType` (string)  
  - [ ] `IsDirector` (bool)  
  - [ ] `SettingsJson` (control-specific configuration)  
  - [ ] `DisplayName`  
  - [ ] `SequenceOrder`  

### **Event Model Integration (NEW)**

- [ ] Add `List<EventControl>` to `Event`  
- [ ] UI for adding/removing controls in `EventsEdit.razor`  
- [ ] Template support for default controls  

---

## 🎛️ **2.5.1.1 Director Controls (NEW)**

Some events require a single “Director” control that manages the overall flow of the event (e.g., Talent Show Director, Bingo Director, Election Director). Director controls act as the primary operational control for the event.

### **Director Control Rules**
- Only **one** director control may be added to an event.  
- Director controls appear in a **dedicated dropdown** at the bottom of the event page.  
- The dropdown is initially blank until a director is selected.  
- Once a director is chosen, the system unlocks the ability to add **additional non-director controls** (e.g., SponsorDisplayControl, AgendaControl, NoteTakerControl).  
- Director controls have their own workflow/status (e.g., SignUps → Auditions → Rehearsal → Live for Talent Show).  
- Director controls link to their own controller sub-page.  

### **Director Control Types (NEW)**
- `TalentShowDirectorControl`  
- `BingoDirectorControl`  
- `ElectionDirectorControl`  

These are separate from functional controls such as `TalentShowControl`, `BingoControl`, etc.

---

## 🖥️ **2.5.2 Controller Page (NEW)**  
**Route:** `/events/{slug}/control`

### **Core Features**
- [ ] Password-protected (Admin/Board/Coordinator)  
- [ ] Live/Test mode toggle  
- [ ] List of displays waiting to be paired  
- [ ] Assign displays to specific controls  
- [ ] Tabs/subpages for each attached control  
- [ ] Real-time updates via SignalR  
- [ ] Control-specific actions (e.g., “Next Performer”, “Call Bingo Number”)  

### **Director Selection UI (NEW)**
- A dedicated dropdown labeled **“Event Director”**  
- Shows only director-type controls  
- Once selected, the director control becomes the primary tab in the controller  
- Additional controls can then be added from the standard controls list  
- Director control determines the main event workflow and status  

### **TEMP PILOT**
- [x] `/admin/talent-show/rehearsal` controller page with schedule navigation  
- [x] Overall show state (`PreShow`, `Live`, `PostShow`)  
- [x] Live sub-state controls (`HostTalk`, `ActShow`, `PauseIntermission`)  
- [x] Connected device registry + role assignment workflow  
- [x] Display rename support from controller page  

---

## 📺 **2.5.3 Display Page (NEW)**  
**Route:** `/display`

- [ ] No login required  
- [ ] Connects to SignalR  
- [ ] Receives 5-character pairing code  
- [ ] Shows “Waiting for controller…”  
- [ ] Assigned to a control by the controller  
- [ ] Renders that control’s UI  
- [ ] Supports multiple display roles (host, backstage, performer, audience)  

### **TEMP PILOT**
- [x] `/display` generic device page with pairing code  
- [x] Role-specific display behavior for `MainBoard`, `BackstageDirector`, `JudgesVote`, `AudienceVote`  
- [x] Assignment survives reconnect/refresh by persisted device identity  

---

## 🔌 **2.5.4 SignalR Hub (NEW)**

- [ ] Display registration  
- [ ] Pairing code generation  
- [ ] Broadcast control state  
- [ ] Route updates to displays assigned to specific controls  
- [ ] Maintain Live/Test isolation  
- [ ] Handle reconnects and state restoration  

### **TEMP PILOT**
- [x] `TalentShowHub` at `/hubs/talent-show`  
- [x] Real-time vote submission and rollup transport  

---

## 🎛️ **2.5.5 Control Types (UPDATED)**

### **Director Controls (NEW)**
- `TalentShowDirectorControl`  
- `BingoDirectorControl`  
- `ElectionDirectorControl`  

### **Reusable Controls**
- `SponsorDisplayControl`  
- `AgendaControl`  
- `NoteTakerControl`  
- `ScheduleControl`  
- `SlideshowControl`  
- `QRCodeControl`  
- `CountdownTimerControl`  

### **Event-Specific Controls**
- `TalentAuditionControl`  
- `TalentShowControl`  
- `PerformerBackstageControl`  
- `HostTeleprompterControl`  
- `ActTimerControl`  
- `KaraokeVideoControl`  
- `BingoControl`  
- `ElectionControl`  

---

## 🎤 **2.5.6 Example Event Configurations (UPDATED)**

### **Talent Show (Main Event)**
- **Director:** `TalentShowDirectorControl`  
- Additional Controls:  
  - `TalentShowControl`  
  - `SponsorDisplayControl`  
  - `HostTeleprompterControl`  
  - `PerformerBackstageControl`  
  - `ActTimerControl`  
  - `KaraokeVideoControl` (optional)  

### **Talent Show Auditions**
- **Director:** `TalentShowDirectorControl` (same director)  
- Additional Controls:  
  - `TalentAuditionControl`  
  - `NoteTakerControl`  
  - `ScheduleControl`  

### **Bingo Night**
- **Director:** `BingoDirectorControl`  
- Additional Controls:  
  - `SponsorDisplayControl`  

### **Election Event**
- **Director:** `ElectionDirectorControl`  
- Additional Controls:  
  - `AgendaControl`  
  - `NoteTakerControl`  

---

## 🗓️ **2.5.7 School Year Transition Control (PLANNED)**

A future `SchoolYearTransitionControl` will support:

- Election checklist  
- Officer transitions  
- Budget rollover  
- Committee resets  
- Event archive  
- Volunteer reset  
- Sponsor reset  
- Calendar reset  

---

## 🚀 **2.5.8 Implementation Order (UPDATED)**

### **Phase A — Core Framework**
- [ ] `EventControl` model  
- [ ] Director control support  
- [ ] SignalR hub  
- [ ] Display page  
- [ ] Controller page  
- [ ] `SponsorDisplayControl`  

### **Phase B — Talent Show**
- [ ] `TalentShowDirectorControl`  
- [ ] `TalentAuditionControl`  
- [ ] `TalentShowControl`  
- [ ] `PerformerBackstageControl`  
- [ ] `HostTeleprompterControl`  
- [ ] `ActTimerControl`  
- [ ] `KaraokeVideoControl`  

### **Phase C — Elections**
- [ ] `ElectionDirectorControl`  
- [ ] `ElectionControl`  
- [ ] `AgendaControl` integration  
- [ ] `NoteTakerControl` integration  

### **Phase D — School Year Transition**
- [ ] `SchoolYearTransitionControl`  
- [ ] Data archive/reset tools  

### 🤝 **Phase 3: Volunteer & Integration Systems** ⏱️ *Week 5-6*
**Status: ⏳ Planned**

#### 3.1 Excel Import System (SignUpGenius Alternative) (ENHANCED)
- [ ] **Excel Import Models**
- [ ] **Import Functionality (ENHANCED)**
  - [ ] Excel file upload and parsing
  - [ ] Data validation and cleanup
  - [ ] Volunteer status tracking
  - [ ] Duplicate detection
  - [ ] Compare with previous event volunteers
  - [ ] **NEW** Multi-day volunteer coordination
  - [ ] **NEW** Day-specific volunteer assignments

#### 3.2 Volunteer Role Management (ENHANCED)
- [ ] **EventVolunteerRole Model**
  - [ ] RoleId (PK)
  - [ ] EventId (FK to Event)
  - [ ] **NEW** EventDayId (FK to EventDay, nullable)
  - [ ] RoleName
  - [ ] Description
  - [ ] Capacity
  - [x] **NEW** SignupWindowStart (date/time volunteers can start signing up)
  - [x] **NEW** SignupWindowEnd (date/time volunteer signup closes)
  - [ ] SignUpGeniusTemplateLink
  - [ ] GivebacksGoal
  - [ ] CustomFields (JSON)

- [ ] **Role Management UI (ENHANCED)**
  - [ ] Create/edit roles within events
  - [x] **NEW** Volunteer signup availability date range UI (start/end)
  - [x] **NEW** Wording update from "Requires Volunteers" to "Volunteers Needed" (or "Can Use Volunteers" where appropriate)
  - [ ] **NEW** Day-specific volunteer roles
  - [ ] Assign volunteers to roles
  - [ ] Role-specific requirements and permissions
  - [ ] Integration with SignUpGenius and Givebacks

#### 3.3 Volunteer Management Interface (ENHANCED)
- [ ] **Volunteer Dashboard**
  - [ ] Role requirements vs. signups
  - [ ] **NEW** Multi-day volunteer tracking
  - [ ] Volunteer contact information
  - [ ] Check-in functionality
  - [ ] Volunteer retention tracking (across copies)

#### 3.4 Givebacks Integration Prep
- [ ] **Membership Import System**
  - [ ] Excel import for Givebacks membership data
  - [ ] Member account creation (email-based)
  - [ ] Passwordless login system preparation
  - [ ] Member status tracking

### 📧 **Phase 4: Communication & Templates** ⏱️ *Week 7-8*
**Status: ⏳ Planned**

#### 4.1 Event Analytics & Comparison (ENHANCED)
- [ ] **Copy Performance Analytics**
  - [ ] Compare metrics across event instances
  - [ ] **NEW** Multi-day event success tracking
  - [ ] Volunteer retention rates between copies
  - [ ] Fundraising performance tracking
  - [ ] Success factor analysis

- [ ] **Template Effectiveness**
  - [ ] Track which events are copied most frequently
  - [ ] Success metrics for template-based events
  - [ ] Template optimization recommendations
  - [ ] **NEW** Multi-day event template performance

#### 4.2 Google Cloud Integration Prep
- [ ] **Email System Foundation**
  - [ ] Email service interface
  - [ ] Template-based email generation
  - [ ] Coordinator notification system
  - [ ] Approval workflow notifications
  - [ ] Volunteer communication tools
  - [ ] **NEW** Multi-day event communication templates

- [ ] **Document Management (ENHANCED)**
  - [ ] **EventDocument Model (ENHANCED)**
  - [ ] DocumentId (PK)
  - [ ] EventId (FK to Event)
  - [ ] **NEW** EventDayId (FK to EventDay, nullable)
  - [ ] DocumentType (enum: Flyer, Form, Report, etc.)
  - [ ] FilePath
  - [ ] UploadedBy (FK to ApplicationUser)
  - [ ] UploadDate
  - [ ] AccessLevel (enum: Public, Private, Restricted)
  - [ ] RelatedTo (FK to Event, if applicable)
  - [ ] TemplateId (FK to DocumentTemplate, if applicable)
  - [ ] Status (enum: Active, Archived)

- [ ] **Document Management UI (ENHANCED)**
  - [ ] Upload, edit, delete documents
  - [ ] **NEW** Day-specific document management
  - [ ] Document categorization and tagging
  - [ ] Search and filter documents
  - [ ] Integration with event forms and reports
  - [ ] **Document Features**
  - [ ] Public vs. internal document separation
  - [ ] Google Cloud storage preparation
  - [ ] Document version control
  - [ ] Copy documents from source events

### 🗺️ **Phase 5: Advanced Features** ⏱️ *Week 9-10*
**Status: ⏳ Future**

#### 5.1 Advanced Approval Workflow
- [ ] **Multi-Level Approvals**
  - [ ] Principal approval for school events
  - [ ] Board approval for PTA events
  - [ ] Budget approval thresholds
  - [ ] Conditional approvals with requirements
  - [ ] **NEW** Multi-day event approval workflows

#### 5.2 Event Analytics Dashboard (ENHANCED)
- [ ] **Performance Tracking**
  - [ ] Event success scoring system
  - [ ] **NEW** Multi-day event analytics
  - [ ] Volunteer satisfaction metrics
  - [ ] Fundraising goal tracking
  - [ ] Attendance vs. projections

#### 5.3 Automated Workflows
- [ ] **Smart Reminders**
  - [ ] Approval deadline notifications
  - [ ] Volunteer recruitment reminders
  - [ ] Event preparation checklists
  - [ ] Post-event feedback collection
  - [ ] **NEW** Multi-day event coordination reminders

### 🔮 **Phase 6: Future Integrations** ⏱️ *Future*
**Status: 📋 Backlog**

#### 6.1 External API Integrations
- [ ] **SignUpGenius API** (when credentials available)
  - [ ] Real-time volunteer sync
  - [ ] Automated signup management
  - [ ] **NEW** Multi-day event volunteer coordination
- [ ] **Givebacks API** (when available)  
  - [ ] Membership verification
  - [ ] Automated account creation
- [ ] **Google Cloud APIs**
  - [ ] Gmail integration for communications
  - [ ] Google Drive for official documents
  - [ ] Google Calendar integration with multi-day support

#### 6.2 Mobile & Advanced Features
- [ ] **Mobile App Considerations**
  - [ ] Progressive Web App (PWA) features
  - [ ] Mobile approval workflow
  - [ ] Push notifications for approvals
  - [ ] **NEW** Mobile multi-day event management
- [ ] **Advanced Analytics**
  - [ ] Predictive event success modeling
  - [ ] Volunteer retention prediction
  - [ ] Optimal scheduling recommendations
  - [ ] **NEW** Multi-day event optimization

## 📁 File Structure Plan (UPDATED)

### Completed Multi-Day Event Files
```
LuxfordPTAWeb.Shared/Models/
├── Event.cs ✅ (Enhanced with multi-day support)
├── EventDay.cs ✅ (Complete multi-day model)

LuxfordPTAWeb/Controllers/
├── EventsController.cs ✅ (Enhanced with multi-day & audit support)
├── EventDayController.cs ✅ (Complete CRUD operations)

LuxfordPTAWeb.Client/AdminPages/
├── EventsCreate.razor ✅ (Enhanced with multi-day UI)
├── EventsAdmin.razor ✅ (Enhanced filtering & multi-day display)
├── EventCatSubAdmin.razor ✅ (Enhanced category management)

LuxfordPTAWeb.Shared/DTOs/
├── CopyEventRequestDTO.cs ✅ (Event copying support)
├── CopyEventDayRequestDTO.cs ✅ (Day copying support)
├── UpdateEventDTO.cs ✅ (Enhanced event updates)
├── EventDashboardSummaryDTO.cs ✅ (Dashboard statistics)
```

### Still Needed Files
```
LuxfordPTAWeb/Controllers/
├── EventTemplateController.cs ✅ (NEW - Phase 2 groundwork added)

LuxfordPTAWeb.Client/AdminPages/
├── EventsEdit.razor (NEEDS ENHANCEMENT - Phase 2)
├── EventTemplateAdmin.razor (NEW - Phase 2)

LuxfordPTAWeb.Client/Components/
├── EventDayTimeline.razor (NEW - Phase 2)
├── CategorySelector.razor (NEW - Phase 2)
├── EventCopyModal.razor (NEW - Phase 2)
```

## 🎯 Current Sprint Focus (UPDATED)

### **Sprint 1** (COMPLETED) - Multi-Day Foundation ✅
1. ✅ Enhanced Event Model + EventDay Model + Migration 
2. ✅ Multi-day event database schema with audit tracking
3. ✅ EventDay CRUD operations with permission controls
4. ✅ Multi-day UI components in EventsCreate.razor
5. ✅ Enhanced EventsAdmin.razor with filtering and multi-day display

### **Current Sprint** (This Week) - Enhanced Admin & Templates
1. ✅ **HIGH PRIORITY** Fixed event creation validation error with CreateEventDTO *(COMPLETED)*
2. ✅ **HIGH PRIORITY** Fixed EventDay creation validation error with CreateEventDayDTO *(COMPLETED)*
3. ✅ **HIGH PRIORITY** Implemented event category permissions and coordinator requirements *(COMPLETED)*
4. ✅ **HIGH PRIORITY** Complete EventsEdit.razor with multi-day editing *(COMPLETED)*
5. ✅ **HIGH PRIORITY** Implement "Copy to New Event" feature *(COMPLETED)*
6. ✅ **HIGH PRIORITY** Add "Copy to New Event" button in EventsAdmin.razor *(COMPLETED)*
7. 📝 **MEDIUM PRIORITY** Event template system for recurring events with stations/components *(NOT STARTED)*

## 🤔 Updated Open Questions & Decisions

1. **Event Templates**: Prioritize templates for Fire Prevention Week, Book Fair, and Spirit Week first.
2. **Copy UI Location**: "Copy to New Event" should be prominent in both EventsCreate and EventsAdmin.
3. **Copy Behavior**: Copy should create a draft (`Planning`) with title/details prefilled and editable after creation.
4. **Edit Interface**: Should multi-day event editing be on the same page or separate tabs?
5. **Template Scope**: Templates should include stations and other copyable components from prior events.
6. **Calendar Integration**: Should we build a full calendar view or integrate with existing calendar systems?
7. **Details Page Control**: Add a configurable control for whether an event/category should use a dedicated details page.
8. **Flyer Storage/Hosting**: ✅ Use local storage now; add Google Drive later behind admin configuration.

## 📝 Updated Implementation Notes

- **Multi-Day Success**: ✅ Multi-day events are fully functional with complete CRUD operations
- **Admin Interface**: ✅ Enhanced filtering and display working well with good UX
- **Audit Trail**: ✅ Full audit tracking implemented and working
- **Event Creation Fix**: ✅ Resolved validation error by implementing CreateEventDTO to separate API concerns from Entity Framework navigation properties
- **Next Priority**: Focus on event templates (stations/components), volunteer signup windows, and markdown editor rollout
- **Template System**: Ready to implement - database structure supports it
- **Performance**: Current queries are efficient with proper indexing on common filters

---

**Last Updated**: 01/16/25 
**Current Sprint**: Phase 2 - Enhanced Admin Interface, Copy to New Event, and Event Templates  
**Next Review**: After event template foundations and volunteer signup-window implementation  
**Repository**: [LuxfordPTABlazor](https://github.com/jedelfraisse/LuxfordPTABlazor)
