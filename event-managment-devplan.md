# 🎪 Luxford PTA Event Management System - Development Plan

## 📋 Project Overview
Building a comprehensive event management system for the Luxford PTA that handles event creation, volunteer coordination, and public display of events with integration points for SignUpGenius, Givebacks membership, and Google Cloud services.

## 🎯 Project Phases

### 📊 **Phase 1: Foundation & Basic CRUD** ⏱️ *Week 1-2*
**Status: 🟡 In Progress**

#### 1.1 Enhanced Event Model & Database
- [ ] **Event Model Enhancement**
  - [ ] Add EventCoordinatorId (FK to ApplicationUser)
  - [ ] Add EventStatus enum (Planning, Active, InProgress, Completed, Cancelled)
  - [ ] Add timing fields (SetupStartTime, EventStartTime, EventEndTime, CleanupEndTime)
  - [ ] Add capacity fields (MaxAttendees, EstimatedAttendees)
  - [ ] Add volunteer flags (RequiresVolunteers, RequiresSetup, RequiresCleanup)
  - [ ] Add Notes (internal) and PublicInstructions fields
  - [ ] Add WeatherBackupPlan field
  - [ ] Add ExcelImportId for volunteer tracking

#### 1.2 Basic Event CRUD Operations
- [ ] **EventsController Updates**
  - [ ] POST /api/events (Create)
  - [ ] PUT /api/events/{id} (Update)  
  - [ ] GET /api/events/{id} (Single event details)
  - [ ] Event ownership validation (coordinators can only edit their events)

#### 1.3 Basic Create/Edit Forms
- [ ] **EventsCreate.razor**
  - [ ] Basic information form (Title, Date, Description, Location)
  - [ ] Event type and school year selection
  - [ ] Coordinator assignment (Admin only)
  - [ ] Timing configuration
  - [ ] Status management
- [ ] **EventsEdit.razor**  
  - [ ] Same as create with pre-populated data
  - [ ] Access control (coordinators vs admins)
  - [ ] Edit history/audit trail

#### 1.4 File Storage Setup
- [ ] **Local File Storage**
  - [ ] Create `wwwroot/events/{eventId}/` folder structure
  - [ ] Image upload functionality for event photos
  - [ ] Document upload for forms/flyers
  - [ ] File management API endpoints

### 🎨 **Phase 2: Event Planning Components** ⏱️ *Week 3-4*
**Status: ⏳ Planned**

#### 2.1 Station Management System
- [ ] **EventStation Model**
```
  - Id, EventId, Name, Description, Location
  - RequiredVolunteers, SetupInstructions
  - DisplayOrder, IsActive
```
- [ ] **Station CRUD Operations**
  - [ ] Add/Edit/Delete stations within events
  - [ ] Drag-and-drop reordering
  - [ ] Station templates (Ring Toss, Duck Pond, Food, etc.)

#### 2.2 Schedule/Timeline Builder  
- [ ] **EventScheduleItem Model**
```
  - Id, EventId, StartTime, EndTime, Title
  - Description, ResponsibleVolunteer, Location
```
- [ ] **Schedule Management UI**
  - [ ] Visual timeline builder
  - [ ] Conflict detection
  - [ ] Schedule templates

#### 2.3 Rules & Guidelines System
- [ ] **EventRule Model**
```
  - Id, EventId, RuleType, Title, Description
  - IsRequired, DisplayOrder
```
- [ ] **Rule Management UI**
  - [ ] Safety requirements
  - [ ] Age restrictions  
  - [ ] Weather policies
  - [ ] Rule templates

### 🤝 **Phase 3: Volunteer & Integration Systems** ⏱️ *Week 5-6*
**Status: ⏳ Planned**

#### 3.1 Excel Import System (SignUpGenius Alternative)
- [ ] **Excel Import Models**
```
  - SignUpImport (Id, EventId, ImportDate, FileName)
  - VolunteerSignUp (Id, ImportId, Name, Email, PhoneNumber, Role, TimeSlot)
```
- [ ] **Import Functionality**
  - [ ] Excel file upload and parsing
  - [ ] Data validation and cleanup
  - [ ] Volunteer status tracking
  - [ ] Duplicate detection

#### 3.2 Volunteer Role Management
- [ ] **EventVolunteerRole Model**
```
  - Id, EventId, RoleName, Description
  - RequiredCount, SignedUpCount, StationId
```
- [ ] **Volunteer Dashboard**
  - [ ] Role requirements vs. signups
  - [ ] Volunteer contact information
  - [ ] Check-in functionality

#### 3.3 Givebacks Integration Prep
- [ ] **Membership Import System**
  - [ ] Excel import for Givebacks membership data
  - [ ] Member account creation (email-based)
  - [ ] Passwordless login system preparation
  - [ ] Member status tracking

### 📧 **Phase 4: Communication & Templates** ⏱️ *Week 7-8*
**Status: ⏳ Planned**

#### 4.1 Event Templates
- [ ] **EventTemplate Model**
```
  - Id, Name, Description, EventTypeId
  - DefaultStations, DefaultSchedule, DefaultRules
```
- [ ] **Template System**
  - [ ] Fall Festival template
  - [ ] Book Fair template  
  - [ ] Talent Show template
  - [ ] Custom template creation

#### 4.2 Google Cloud Integration Prep
- [ ] **Email System Foundation**
  - [ ] Email service interface
  - [ ] Template-based email generation
  - [ ] Coordinator notification system
  - [ ] Volunteer communication tools

#### 4.3 Document Management
- [ ] **EventDocument Model**
```
  - Id, EventId, FileName, FilePath
  - DocumentType, IsPublic, UploadDate
```
- [ ] **Document Features**
  - [ ] Public vs. internal document separation
  - [ ] Google Cloud storage preparation
  - [ ] Document version control

### 🗺️ **Phase 5: Advanced Features** ⏱️ *Week 9-10*
**Status: ⏳ Future**

#### 5.1 Event Mapping System
- [ ] **Event Map Integration**
  - [ ] Station location mapping
  - [ ] Interactive venue maps
  - [ ] Accessibility information
  - [ ] QR codes for station information

#### 5.2 Approval Workflow
- [ ] **Event Approval System**
  - [ ] Draft → Review → Approved → Published workflow
  - [ ] Admin approval requirements
  - [ ] Change request system
  - [ ] Approval history tracking

#### 5.3 Reporting & Analytics  
- [ ] **Event Analytics**
  - [ ] Volunteer participation reports
  - [ ] Event attendance tracking
  - [ ] Success metrics dashboard
  - [ ] Year-over-year comparisons

### 🔮 **Phase 6: Future Integrations** ⏱️ *Future*
**Status: 📋 Backlog**

#### 6.1 External API Integrations
- [ ] **SignUpGenius API** (when credentials available)
  - [ ] Real-time volunteer sync
  - [ ] Automated signup management
- [ ] **Givebacks API** (when available)  
  - [ ] Membership verification
  - [ ] Automated account creation
- [ ] **Google Cloud APIs**
  - [ ] Gmail integration for communications
  - [ ] Google Drive for official documents
  - [ ] Google Calendar integration

#### 6.2 Mobile & Advanced Features
- [ ] **Mobile App Considerations**
  - [ ] Progressive Web App (PWA) features
  - [ ] Mobile check-in system
  - [ ] Push notifications
- [ ] **Advanced Analytics**
  - [ ] Volunteer retention tracking
  - [ ] Event ROI analysis
  - [ ] Predictive planning tools

## 📁 File Structure Plan

```
LuxfordPTAWeb/
├── Data/
│   ├── Models/
│   │   ├── Event.cs ✅ (Enhanced)
│   │   ├── EventStation.cs
│   │   ├── EventScheduleItem.cs
│   │   ├── EventRule.cs
│   │   ├── EventVolunteerRole.cs
│   │   ├── EventTemplate.cs
│   │   ├── EventDocument.cs
│   │   ├── SignUpImport.cs
│   │   ├── VolunteerSignUp.cs
│   │   └── Enums/
│   │       ├── EventStatus.cs
│   │       ├── DocumentType.cs
│   │       └── RuleType.cs
│   └── ApplicationDbContext.cs ✅ (Updated)
├── Controllers/
│   ├── EventsController.cs ✅ (Enhanced)
│   ├── EventStationsController.cs
│   ├── EventScheduleController.cs
│   ├── EventVolunteersController.cs
│   ├── EventDocumentsController.cs
│   └── FileUploadController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IExcelImportService.cs
│   │   ├── IEmailService.cs
│   │   └── IFileStorageService.cs
│   ├── ExcelImportService.cs
│   ├── EmailService.cs (Implementation placeholder)
│   └── FileStorageService.cs
└── wwwroot/
    └── events/ (File storage)
        ├── {eventId}/
        │   ├── images/
        │   ├── documents/
        │   └── flyers/

LuxfordPTAWeb.Client/
├── AdminPages/
│   ├── EventsAdmin.razor ✅
│   ├── EventsCreate.razor 📝 (In Progress)
│   ├── EventsEdit.razor 📝 (In Progress)  
│   ├── EventStations.razor
│   ├── EventSchedule.razor
│   ├── EventVolunteers.razor
│   ├── EventDocuments.razor
│   └── EventTemplates.razor
├── Components/
│   ├── Events/
│   │   ├── EventStationManager.razor
│   │   ├── ScheduleBuilder.razor
│   │   ├── VolunteerRoleManager.razor
│   │   ├── RuleManager.razor
│   │   └── FileUploader.razor
│   └── Shared/
│       ├── TimeRangePicker.razor
│       └── StatusBadge.razor
├── Services/
│   ├── EventService.cs
│   ├── FileUploadService.cs
│   └── ExcelImportService.cs
└── Models/
    └── DTOs/
        ├── EventDto.cs
        ├── EventStationDto.cs
        ├── EventScheduleDto.cs
        └── VolunteerRoleDto.cs
```

## 🎯 Current Sprint Focus

### **Sprint 1** (This Week)
1. ✅ Enhanced Event Model + Migration
2. 🔄 Basic EventsCreate.razor form
3. 🔄 Basic EventsEdit.razor form  
4. 🔄 Updated EventsController CRUD operations

### **Next Sprint** (Week 2)
1. 📝 File upload system
2. 📝 Station management MVP
3. 📝 Event coordinator assignment
4. 📝 Basic schedule builder

## 📊 Progress Tracking

- **Phase 1**: 🟡 25% Complete
- **Phase 2**: ⏳ Not Started
- **Phase 3**: ⏳ Not Started  
- **Phase 4**: ⏳ Not Started
- **Phase 5**: ⏳ Not Started
- **Phase 6**: 📋 Future

## 🔧 Technical Specifications

### Database Schema Changes
```sql
-- New tables to be created in migrations
CREATE TABLE EventStations (
    Id int IDENTITY(1,1) PRIMARY KEY,
    EventId int NOT NULL FOREIGN KEY REFERENCES Events(Id),
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500),
    Location nvarchar(100),
    RequiredVolunteers int DEFAULT 0,
    SetupInstructions nvarchar(1000),
    DisplayOrder int DEFAULT 0,
    IsActive bit DEFAULT 1
);

CREATE TABLE EventScheduleItems (
    Id int IDENTITY(1,1) PRIMARY KEY,
    EventId int NOT NULL FOREIGN KEY REFERENCES Events(Id),
    StartTime datetime2 NOT NULL,
    EndTime datetime2 NOT NULL,
    Title nvarchar(200) NOT NULL,
    Description nvarchar(500),
    ResponsibleVolunteer nvarchar(100),
    Location nvarchar(100)
);

CREATE TABLE EventRules (
    Id int IDENTITY(1,1) PRIMARY KEY,
    EventId int NOT NULL FOREIGN KEY REFERENCES Events(Id),
    RuleType nvarchar(50) NOT NULL, -- Safety, Age, Weather, General
    Title nvarchar(200) NOT NULL,
    Description nvarchar(1000) NOT NULL,
    IsRequired bit DEFAULT 0,
    DisplayOrder int DEFAULT 0
);

CREATE TABLE EventVolunteerRoles (
    Id int IDENTITY(1,1) PRIMARY KEY,
    EventId int NOT NULL FOREIGN KEY REFERENCES Events(Id),
    RoleName nvarchar(100) NOT NULL,
    Description nvarchar(500),
    RequiredCount int DEFAULT 1,
    SignedUpCount int DEFAULT 0,
    StationId int FOREIGN KEY REFERENCES EventStations(Id)
);

CREATE TABLE SignUpImports (
    Id int IDENTITY(1,1) PRIMARY KEY,
    EventId int NOT NULL FOREIGN KEY REFERENCES Events(Id),
    ImportDate datetime2 DEFAULT GETDATE(),
    FileName nvarchar(255) NOT NULL,
    RecordCount int DEFAULT 0
);

CREATE TABLE VolunteerSignUps (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ImportId int NOT NULL FOREIGN KEY REFERENCES SignUpImports(Id),
    Name nvarchar(100) NOT NULL,
    Email nvarchar(255),
    PhoneNumber nvarchar(20),
    Role nvarchar(100),
    TimeSlot nvarchar(100),
    IsCheckedIn bit DEFAULT 0,
    CheckInTime datetime2
);
```

### API Endpoints Plan
```
Events API:
POST   /api/events                      - Create event
PUT    /api/events/{id}                 - Update event
GET    /api/events/{id}                 - Get single event with full details
DELETE /api/events/{id}                 - Delete event
GET    /api/events/{id}/coordinator     - Get coordinator info

Stations API:
GET    /api/events/{id}/stations        - Get all stations for event
POST   /api/events/{id}/stations        - Add station
PUT    /api/events/{id}/stations/{id}   - Update station
DELETE /api/events/{id}/stations/{id}   - Delete station
POST   /api/events/{id}/stations/reorder - Reorder stations

Schedule API:
GET    /api/events/{id}/schedule        - Get event schedule
POST   /api/events/{id}/schedule        - Add schedule item
PUT    /api/events/{id}/schedule/{id}   - Update schedule item
DELETE /api/events/{id}/schedule/{id}   - Delete schedule item

Volunteers API:
GET    /api/events/{id}/volunteers      - Get volunteer info
POST   /api/events/{id}/import-excel    - Import volunteer data
GET    /api/events/{id}/roles           - Get volunteer roles
POST   /api/events/{id}/roles           - Add volunteer role
POST   /api/events/{id}/checkin/{volunteerId} - Check in volunteer

File Upload API:
POST   /api/events/{id}/upload          - Upload file
GET    /api/events/{id}/files           - Get event files
DELETE /api/events/{id}/files/{fileId}  - Delete file
```

### Security & Permissions
- **Event Coordinators**: Can only edit assigned events
- **Board Members**: Can edit any event + assign coordinators
- **Admins**: Full access to all features
- **Public**: Read-only access to published events

## 🤔 Open Questions & Decisions

1. **Volunteer Membership Requirement**: Confirm with PTA if volunteers must be members
2. **Google Cloud Timeline**: When will Google Cloud credentials be available?
3. **Event Approval Workflow**: Which events require approval? All or just public ones?
4. **File Size Limits**: What are the limits for uploaded documents/images?
5. **Event Archive Policy**: How long should completed events be retained?
6. **Template Sharing**: Should event templates be shared across school years?
7. **Volunteer Check-in**: Do we need QR code or manual check-in system?

## 📝 Notes

- Focus on local storage and Excel imports until external APIs are available
- Build interfaces now, implement integrations later
- Prioritize coordinator workflow over admin features initially
- Keep public event display simple and clean
- Consider mobile-first design for volunteer check-in features
- Plan for offline functionality during events

## 🚀 Getting Started Checklist

### Phase 1 Prerequisites
- [ ] Enhanced Event model completed
- [ ] Database migration created and applied
- [ ] Basic EventsController CRUD operations
- [ ] Authentication/authorization working
- [ ] File upload directory structure created

### Development Environment Setup
- [ ] .NET 9 SDK installed
- [ ] SQL Server/LocalDB configured
- [ ] Visual Studio 2022 with Blazor templates
- [ ] Git repository configured
- [ ] Development database seeded with test data

## 🎪 Sample Event Structure Example
```
Fall Festival 2025
├── Basic Info
│   ├── Date: October 25, 2025
│   ├── Time: 10:00 AM - 2:00 PM
│   ├── Location: School Campus
│   └── Coordinator: Ashley Threadgill
├── Schedule
│   ├── 8:00 AM - Setup begins
│   ├── 10:00 AM - Event opens to public  
│   ├── 2:00 PM - Event ends
│   └── 4:00 PM - Cleanup complete
├── Stations (8 total)
│   ├── Game Station 1 (Ring Toss) - 2 volunteers
│   ├── Game Station 2 (Duck Pond) - 2 volunteers
│   ├── Food Station (Popcorn & Cotton Candy) - 3 volunteers
│   ├── Check-in/Ticket Sales - 2 volunteers
│   └── Information Booth - 1 volunteer
├── Volunteer Roles (24 total needed)
│   ├── Event Coordinator (1) ✅
│   ├── Station Leaders (5) - 3/5 filled
│   ├── Setup Crew (8) - 2/8 filled
│   ├── Cleanup Crew (6) - 1/6 filled
│   └── Floater/Support (4) - 0/4 filled
├── Rules & Guidelines
│   ├── Safety Requirements (5 rules)
│   ├── Age Restrictions (3 rules)
│   └── Weather Policy (1 rule)
└── Documents
    ├── Volunteer Instructions.pdf
    ├── Setup Diagram.jpg
    ├── Emergency Contacts.pdf
    └── Event Flyer.jpg
```

---

**Last Updated**: December 2024  
**Current Sprint**: Phase 1 - Foundation & Basic CRUD  
**Next Review**: After Phase 1 completion  
**Repository**: [LuxfordPTABlazor](https://github.com/jedelfraisse/LuxfordPTABlazor)