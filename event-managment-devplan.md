# 🎪 Luxford PTA Event Management System - Development Plan

## 📋 Project Overview
Building a comprehensive event management system for the Luxford PTA that handles event creation, volunteer coordination, and public display of events with integration points for SignUpGenius, Givebacks membership, and Google Cloud services.

## Last Year Newsletters to be added somewhere in the site:

- September 2024: https://secure.smore.com/n/n1x35s
- Octber 2024: https://secure.smore.com/n/1hpa0
- November 2024: https://secure.smore.com/n/80zfh
- December 2024: https://secure.smore.com/n/w25qj
- January 2025: https://secure.smore.com/n/uf8p7
- February 2025: https://secure.smore.com/n/5zje1
- March 2025: https://secure.smore.com/n/gx3bd
- April 2025:
- May 2025: https://secure.smore.com/n/tf42xc
- June 2025: https://secure.smore.com/n/ka96s


## 🎯 Project Phases

### 📊 **Phase 1: Foundation & Basic CRUD** ⏱️ *Week 1-2*
**Status: 🟡 In Progress**

#### 1.1 Enhanced Event Model & Database
- [x] **Event Model Enhancement**
  - [x] Add EventCoordinatorId (FK to ApplicationUser)
  - [x] **UPDATED** EventStatus enum (Planning, SubmittedForApproval, Active, InProgress, WrapUp, Completed, Cancelled)
  - [x] Add timing fields (SetupStartTime, EventStartTime, EventEndTime, CleanupEndTime)
  - [x] Add capacity fields (MaxAttendees, EstimatedAttendees)
  - [x] Add volunteer flags (RequiresVolunteers, RequiresSetup, RequiresCleanup)
  - [x] Add Notes (internal) and PublicInstructions fields
  - [x] Add WeatherBackupPlan field
  - [x] Add ExcelImportId for volunteer tracking
  - [x] Add approval tracking fields (ApprovedBy, ApprovedDate, ApprovalNotes) *(In Progress)*
  - [x] Multi-day event support (IsMultiDay flag, EventDays collection) *(In Progress)*

#### 1.1.1 **Multi-Day Event System (NEW)**
- [x] **EventDay Model** *(Started)*
  - [x] EventDayId (PK)
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

- [ ] **Multi-Day Event UI Components** *(Not Started)*
  - [ ] Multi-day toggle in EventsCreate.razor
  - [ ] Day management interface (add/edit/delete days)
  - [ ] Calendar view showing all days of multi-day events
  - [ ] Public display showing event overview + individual days
  - [ ] Day-specific volunteer coordination

#### 1.2 Basic Event CRUD Operations
- [x] **EventsController Updates** *(Started)*
  - [x] POST /api/events (Create) - Auto-submit for approval, support multi-day *(Partial)*
  - [x] PUT /api/events/{id} (Update) - Support multi-day updates *(Partial)*
  - [x] GET /api/events/{id} (Single event details) - Include EventDays *(Partial)*
  - [ ] Event ownership validation (coordinators can only edit their events)
  - [ ] **NEW** Approval workflow endpoints (approve, reject, request changes)
  - [ ] **NEW** Multi-day specific endpoints (manage days)

#### 1.2.1 **EventDay CRUD Operations (NEW)**
- [x] **EventDayController** *(Started)*
  - [x] POST /api/events/{eventId}/days (Add day to event)
  - [x] PUT /api/events/{eventId}/days/{dayId} (Update day)
  - [x] DELETE /api/events/{eventId}/days/{dayId} (Remove day)
  - [x] GET /api/events/{eventId}/days (Get all days for event)
  - [ ] POST /api/events/{eventId}/days/{dayId}/copy (Copy day to another event)

#### 1.3 Enhanced Create/Edit Forms
- [ ] **EventsCreate.razor (ENHANCED)**
  - [ ] **NEW** Better category/subcategory selection with search/filtering *(In Progress)*
  - [ ] **NEW** Event type templates (recurring events like Fire Prevention Week)
  - [ ] **NEW** Multi-day event toggle and day management *(In Progress)*
  - [ ] **NEW** Smart defaults based on event category/subcategory
  - [ ] **NEW** Copy from previous event functionality with better filtering
  - [ ] **NEW** Bulk event creation for series (e.g., weekly reading programs)
  - [x] Enhanced coordinator assignment with user search *(Done)*
  - [ ] **NEW** Auto-submit for approval after creation
  
- [ ] **EventsEdit.razor (ENHANCED)**  
  - [ ] Same as create with pre-populated data *(In Progress)*
  - [ ] **NEW** Multi-day management interface *(In Progress)*
  - [ ] Access control (coordinators vs admins)
  - [ ] **NEW** Approval status display and actions
  - [ ] **NEW** Event history/audit trail
  - [ ] **NEW** Day-specific editing for multi-day events

#### 1.3.1 **Enhanced Admin Category Management (NEW)**
- [x] **EventCategoryAdmin.razor (ENHANCED)** *(Started)*
  - [x] Better subcategory management *(Done)*
  - [ ] Category templates and defaults
  - [ ] Bulk category operations
  - [ ] Category usage analytics
  - [ ] Import/export category configurations

- [ ] **Event Template System**
  - [ ] EventTemplate model for recurring events
  - [ ] Template categories (Annual Events, Weekly Programs, etc.)
  - [ ] Pre-filled templates for common events
  - [ ] Template sharing between school years

#### 1.4 File Storage Setup
- [ ] **Local File Storage**
  - [x] Create `wwwroot/events/{eventId}/` folder structure *(Done)*
  - [x] Image upload functionality for event photos *(Done)*
  - [x] Document upload for forms/flyers *(Done)*
  - [x] File management API endpoints *(Done)*
  - [ ] **NEW** Day-specific file uploads for multi-day events

#### 1.5 Event Template/Instance System (ENHANCED)
- [x] **Event Copy & Template Relationships** *(Started)*
  - [x] Add `Slug` property to Event model for URL generation *(Done)*
  - [x] Add `SourceEventId` (FK to source event) to Event model *(Done)*
  - [x] Add `CopyGeneration` int to Event model *(Done)*
  - [x] Helper methods for copy relationships *(Done)*
  - [ ] **NEW** Multi-day event copying (copy all days)

- [x] **Enhanced Copy Functionality** *(Started)*
  - [x] "Copy from Previous Event" in EventsCreate.razor with advanced filtering *(Partial)*
  - [x] "Copy Event" button in EventsAdmin.razor *(Partial)*
  - [ ] **NEW** "Copy Multi-Day Event" with day selection options
  - [x] API endpoint: `POST /api/events/{id}/copy` *(Done)*
  - [x] API endpoint: `GET /api/events/available-for-copy` *(Done)*
  - [ ] **NEW** Template-based copying for recurring annual events
  - [x] Preserve relationships between original and copies *(Done)*

- [x] **Smart Event Resolution (ENHANCED)** *(Started)*
  - [x] `GET /api/events/by-slug/{slug}` - Smart event instance resolution *(Done)*
  - [x] Priority logic: Active/InProgress → Most Recent Completed → Planning *(Done)*
  - [x] Include all related instances in response *(Done)*
  - [x] Query parameters: `?instance=2024`, `?showAll=true` *(Done)*
  - [ ] **NEW** Multi-day event aggregation in responses

- [ ] **EventDetail.razor (ENHANCED)**
  - [ ] Individual event page: `/events/{slug}`
  - [ ] Show primary event with full details
  - [ ] **NEW** Multi-day event timeline view
  - [ ] **NEW** Day-specific registration/volunteer signup links
  - [ ] List other instances/copies with key metrics
  - [ ] Template information and "based on" relationships

#### 1.6 School Year Navigation System (ENHANCED)
- [x] **Dynamic School Year Selector** *(Started)*
  - [x] Replace static calendar download with year navigation *(Done)*
  - [x] API endpoint: `GET /api/events/school-years` *(Done)*
  - [x] API endpoint: `GET /api/events/by-school-year/{year}` *(Done)*
  - [x] Previous/Current/Next year navigation buttons *(Done)*
  - [ ] **NEW** Multi-day event filtering by school year

- [x] **Enhanced Events.razor** *(Started)*
  - [x] School year selector in header *(Done)*
  - [x] Dynamic calendar download links *(Done)*
  - [x] Filter all events by selected school year *(Done)*
  - [x] Planning year visibility (draft/pending events) *(Done)*
  - [ ] **NEW** Multi-day event display options

#### 1.7 Approval Workflow System (ENHANCED)
- [x] **Enhanced EventStatus Enum** *(Done)*
  - [x] Planning (0) - Initial creation state
  - [x] SubmittedForApproval (1) - Submitted to board/principal
  - [x] Active (2) - Approved and public
  - [x] InProgress (3) - Event currently happening
  - [x] WrapUp (4) - Event finished, collecting feedback/metrics
  - [x] Completed (5) - Event fully closed out
  - [x] Cancelled (6) - Event cancelled or not approved

- [ ] **Approval Workflow API (ENHANCED)**
  - [x] `POST /api/events/{id}/submit-for-approval` *(Done)*
  - [x] `POST /api/events/{id}/approve` (Admin/BoardMember only) *(Done)*
  - [x] `POST /api/events/{id}/reject` (Admin/BoardMember only) *(Done)*
  - [x] `POST /api/events/{id}/request-changes` (Admin/BoardMember only) *(Done)*
  - [x] `GET /api/events/pending-approval` (Admin/BoardMember only) *(Done)*
  - [ ] **NEW** Multi-day event approval (all days approved together)

- [ ] **Approval UI Components (ENHANCED)**
  - [x] Approval status badges and indicators *(Done)*
  - [x] Board member approval dashboard *(Done)*
  - [x] Event coordinator notification system *(Done)*
  - [x] Approval history tracking *(Done)*
  - [ ] **NEW** Multi-day event approval interface

### 🎨 **Phase 2: Event Planning Components** ⏱️ *Week 3-4*
**Status: ⏳ Planned**

#### 2.1 Station Management System (ENHANCED)
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

#### 2.2 Schedule/Timeline Builder (ENHANCED)
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

#### 2.3 Rules & Guidelines System (ENHANCED)
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
  - [ ] SignUpGeniusTemplateLink
  - [ ] GivebacksGoal
  - [ ] CustomFields (JSON)

- [ ] **Role Management UI (ENHANCED)**
  - [ ] Create/edit roles within events
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

### New Multi-Day Event Files
```
LuxfordPTAWeb.Shared/Models/
├── EventDay.cs (NEW)
├── EventTemplate.cs (NEW)
└── EventDayVolunteerRole.cs (NEW)

LuxfordPTAWeb/Controllers/
├── EventDayController.cs (NEW)
├── EventTemplateController.cs (NEW)

LuxfordPTAWeb.Client/AdminPages/
├── EventsCreateMultiDay.razor (NEW)
├── EventDayManagement.razor (NEW)
├── EventTemplateAdmin.razor (NEW)

LuxfordPTAWeb.Client/Components/
├── MultiDayEventCard.razor (NEW)
├── EventDayTimeline.razor (NEW)
├── CategorySelector.razor (NEW)
```

## 🎯 Current Sprint Focus (UPDATED)

### **Sprint 1** (This Week) - Multi-Day Foundation
1. 🔄 **NEW** Enhanced Event Model + EventDay Model + Migration *(In Progress)*
2. 🔄 **NEW** Multi-day event database schema *(In Progress)*
3. 🔄 **NEW** EventDay CRUD operations *(In Progress)*
4. 🔄 **NEW** Basic multi-day UI components *(Not Started)*

### **Next Sprint** (Week 2) - Enhanced Admin Interface
1. 📝 **NEW** Enhanced EventsCreate.razor with category/subcategory improvements *(In Progress)*
2. 📝 **NEW** Multi-day event creation interface *(Not Started)*
3. 📝 **NEW** Event template system for recurring events *(Not Started)*
4. 📝 **NEW** Better event copying with multi-day support *(Not Started)*

### **Sprint 3** (Week 3) - Advanced Multi-Day Features
1. 📝 **NEW** Multi-day volunteer coordination *(Not Started)*
2. 📝 **NEW** Day-specific station management *(Not Started)*
3. 📝 **NEW** Multi-day approval workflow *(Not Started)*
4. 📝 **NEW** Enhanced event analytics for multi-day events *(Not Started)*

## 📊 Progress Tracking (UPDATED)

- **Phase 1**: 🟡 45% Complete (Multi-day model, controller, and admin features started)
- **Phase 2**: ⏳ Not Started
- **Phase 3**: ⏳ Not Started  
- **Phase 4**: ⏳ Not Started
- **Phase 5**: ⏳ Not Started
- **Phase 6**: 📋 Future

## 🔧 Technical Specifications (UPDATED)

### Database Schema Changes (NEW)
```sql
-- New EventDay table for multi-day events
CREATE TABLE EventDays (
    EventDayId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT FOREIGN KEY REFERENCES Events(Id),
    DayNumber INT NOT NULL,
    Date DATETIME2 NOT NULL,
    DayTitle NVARCHAR(255),
    Description NVARCHAR(MAX),
    Location NVARCHAR(500),
    StartTime DATETIME2,
    EndTime DATETIME2,
    IsActive BIT DEFAULT 1,
    SpecialInstructions NVARCHAR(MAX),
    MaxAttendees INT
);

-- Add multi-day flag to Events table
ALTER TABLE Events ADD IsMultiDay BIT DEFAULT 0;
ALTER TABLE Events ADD Slug NVARCHAR(255);
ALTER TABLE Events ADD SourceEventId INT;
ALTER TABLE Events ADD CopyGeneration INT DEFAULT 0;
```

### Multi-Day Event Examples
1. **Fire Prevention Week (Oct 6-12, 2025)**
   - Day 1: Fire Safety Education (classroom visits)
   - Day 2: Fire Truck Demonstration
   - Day 3: Escape Plan Workshop
   - Day 4: Fire Department Tour
   - Day 5: Fire Safety Fair

2. **Book Fair Week**
   - Day 1-3: Student Shopping Days
   - Day 4: Family Night
   - Day 5: Teacher Preview

3. **Spirit Week**
   - Monday: Pajama Day
   - Tuesday: Crazy Hair Day
   - Wednesday: Twin Day
   - Thursday: Sports Day
   - Friday: School Colors Day

### Security & Permissions (UPDATED)
- **Event Coordinators**: Can create/edit assigned events, manage multi-day events, view approval status
- **Board Members**: Can approve PTA events, view all pending approvals, manage event templates
- **Admins**: Can approve all events, manage approval workflow, full multi-day event management
- **Principal**: Can approve school-related events (future enhancement)
- **Public**: Can only view Active, InProgress, WrapUp, and Completed events (including all days)

## 🤔 Updated Open Questions & Decisions

1. **Multi-Day Approval**: Should each day be approved individually or as a complete event?
2. **Day-Specific Volunteers**: How granular should volunteer assignments be for multi-day events?
3. **Event Templates**: Should we create templates for common recurring events (Fire Prevention Week, Book Fair)?
4. **Copy Permissions**: Who can copy events? Original coordinators vs. anyone?
5. **School Year Transitions**: When should the system switch to showing the next school year?
6. **Multi-Day Attendance**: Should we track attendance per day or per overall event?
7. **Budget Tracking**: Should budget approval consider total event cost or per-day costs?

## 📝 Updated Implementation Notes

- **Multi-Day First Design**: Build with multi-day events as a core feature, not an afterthought
- **Template-Driven Creation**: Pre-built templates for recurring annual events like Fire Prevention Week
- **Enhanced Admin UX**: Better category/subcategory selection with search and filtering
- **Smart Copying**: Copy multi-day events with option to select specific days
- **Flexible Day Management**: Add/remove/edit individual days within multi-day events
- **Unified Approval**: Multi-day events approved as complete units

## 🎪 Enhanced Sample Multi-Day Event Structure

### Fire Prevention Week 2025
- **Event**: Fire Prevention Week
- **Dates**: October 6-12, 2025
- **Category**: Educational Programs
- **Subcategory**: Safety Education
- **Coordinator**: Safety Committee Chair
- **Status**: Planning → SubmittedForApproval → Active

#### Individual Days:
1. **Day 1 (Oct 6)**: Classroom Fire Safety Education
2. **Day 2 (Oct 7)**: Fire Truck Visit & Demo
3. **Day 3 (Oct 8)**: Escape Plan Workshop
4. **Day 4 (Oct 9)**: Fire Department Tour (Field Trip)
5. **Day 5 (Oct 10)**: Fire Safety Fair (Family Event)

#### Volunteers Needed:
- Setup crew (Day 1 & 5)
- Student guides (Days 2-4)
- Family event coordinators (Day 5)
- Cleanup crew (Day 5)

---

**Last Updated**: 01/15/25 
**Current Sprint**: Phase 1 - Multi-Day Events Foundation & Enhanced Admin Interface
**Next Review**: After multi-day event implementation  
**Repository**: [LuxfordPTABlazor](https://github.com/jedelfraisse/LuxfordPTABlazor)