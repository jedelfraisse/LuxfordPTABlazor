# 🎪 Luxford PTA Event Management System - Development Plan

## 📋 Project Overview
Building a comprehensive event management system for the Luxford PTA that handles event creation, volunteer coordination, and public display of events with integration points for SignUpGenius, Givebacks membership, and Google Cloud services.

## 🎯 Project Phases

### 📊 **Phase 1: Foundation & Basic CRUD** ⏱️ *Week 1-2*
**Status: 🟡 In Progress**

#### 1.1 Enhanced Event Model & Database
- [x] **Event Model Enhancement**
  - [x] Add EventCoordinatorId (FK to ApplicationUser)
  - [ ] **UPDATED** EventStatus enum (Planning, SubmittedForApproval, Active, InProgress, WrapUp, Completed, Cancelled)
  - [x] Add timing fields (SetupStartTime, EventStartTime, EventEndTime, CleanupEndTime)
  - [x] Add capacity fields (MaxAttendees, EstimatedAttendees)
  - [x] Add volunteer flags (RequiresVolunteers, RequiresSetup, RequiresCleanup)
  - [x] Add Notes (internal) and PublicInstructions fields
  - [x] Add WeatherBackupPlan field
  - [x] Add ExcelImportId for volunteer tracking
  - [ ] **NEW** Add approval tracking fields (ApprovedBy, ApprovedDate, ApprovalNotes)

#### 1.2 Basic Event CRUD Operations
- [ ] **EventsController Updates**
  - [ ] POST /api/events (Create) - Auto-submit for approval
  - [ ] PUT /api/events/{id} (Update)  
  - [ ] GET /api/events/{id} (Single event details)
  - [ ] Event ownership validation (coordinators can only edit their events)
  - [ ] **NEW** Approval workflow endpoints (approve, reject, request changes)

#### 1.3 Basic Create/Edit Forms
- [ ] **EventsCreate.razor**
  - [ ] Basic information form (Title, Date, Description, Location)
  - [ ] Event type and school year selection
  - [ ] Coordinator assignment (Admin only)
  - [ ] Timing configuration
  - [ ] **NEW** Copy from previous event functionality
  - [ ] **NEW** Auto-submit for approval after creation
- [ ] **EventsEdit.razor**  
  - [ ] Same as create with pre-populated data
  - [ ] Access control (coordinators vs admins)
  - [ ] **NEW** Approval status display and actions
  - [ ] Edit history/audit trail

#### 1.4 File Storage Setup
- [ ] **Local File Storage**
  - [ ] Create `wwwroot/events/{eventId}/` folder structure
  - [ ] Image upload functionality for event photos
  - [ ] Document upload for forms/flyers
  - [ ] File management API endpoints

#### 1.5 Event Template/Instance System (NEW)
- [ ] **Event Copy & Template Relationships**
  - [ ] Add `Slug` property to Event model for URL generation
  - [ ] Add `SourceEventId` (FK to source event) to Event model
  - [ ] Add `CopyGeneration` int to Event model
  - [ ] Helper methods for copy relationships

- [ ] **Copy Functionality**
  - [ ] "Copy from Previous Event" in EventsCreate.razor
  - [ ] "Copy Event" button in EventsAdmin.razor
  - [ ] API endpoint: `POST /api/events/{id}/copy`
  - [ ] API endpoint: `GET /api/events/available-for-copy`
  - [ ] Preserve relationships between original and copies

- [ ] **Smart Event Resolution**
  - [ ] `GET /api/events/by-slug/{slug}` - Smart event instance resolution
  - [ ] Priority logic: Active/InProgress → Most Recent Completed → Planning
  - [ ] Include all related instances in response
  - [ ] Query parameters: `?instance=2024`, `?showAll=true`

- [ ] **EventDetail.razor (NEW)**
  - [ ] Individual event page: `/events/{slug}`
  - [ ] Show primary event with full details
  - [ ] List other instances/copies with key metrics
  - [ ] Template information and "based on" relationships

#### 1.6 School Year Navigation System (NEW)
- [ ] **Dynamic School Year Selector**
  - [ ] Replace static calendar download with year navigation
  - [ ] API endpoint: `GET /api/events/school-years`
  - [ ] API endpoint: `GET /api/events/by-school-year/{year}`
  - [ ] Previous/Current/Next year navigation buttons

- [ ] **Enhanced Events.razor**
  - [ ] School year selector in header
  - [ ] Dynamic calendar download links
  - [ ] Filter all events by selected school year
  - [ ] Planning year visibility (draft/pending events)

#### 1.7 Approval Workflow System (NEW)
- [ ] **Enhanced EventStatus Enum**
  - [ ] Planning (0) - Initial creation state
  - [ ] SubmittedForApproval (1) - Submitted to board/principal
  - [ ] Active (2) - Approved and public
  - [ ] InProgress (3) - Event currently happening
  - [ ] WrapUp (4) - Event finished, collecting feedback/metrics
  - [ ] Completed (5) - Event fully closed out
  - [ ] Cancelled (6) - Event cancelled or not approved

- [ ] **Approval Workflow API**
  - [ ] `POST /api/events/{id}/submit-for-approval`
  - [ ] `POST /api/events/{id}/approve` (Admin/BoardMember only)
  - [ ] `POST /api/events/{id}/reject` (Admin/BoardMember only)
  - [ ] `POST /api/events/{id}/request-changes` (Admin/BoardMember only)
  - [ ] `GET /api/events/pending-approval` (Admin/BoardMember only)

- [ ] **Approval UI Components**
  - [ ] Approval status badges and indicators
  - [ ] Board member approval dashboard
  - [ ] Event coordinator notification system
  - [ ] Approval history tracking

### 🎨 **Phase 2: Event Planning Components** ⏱️ *Week 3-4*
**Status: ⏳ Planned**

#### 2.1 Station Management System
- [ ] **EventStation Model**
  - [ ] StationId (PK)
  - [ ] EventId (FK to Event)
  - [ ] StationType (enum: Game, Food, Activity, etc.)
  - [ ] Coordinates/Location on map
  - [ ] Capacity/Size
  - [ ] RequiredResources (FK to Resource)
  - [ ] SetupInstructions, CleanupInstructions
  - [ ] SignupGeniusLink, GivebacksLink

- [ ] **Station CRUD Operations**
  - [ ] Add/Edit/Delete stations within events
  - [ ] Drag-and-drop reordering
  - [ ] Station templates (Ring Toss, Duck Pond, Food, etc.)
  - [ ] Copy stations from source events

#### 2.2 Schedule/Timeline Builder  
- [ ] **EventScheduleItem Model**
  - [ ] ScheduleItemId (PK)
  - [ ] EventId (FK to Event)
  - [ ] StationId (FK to EventStation)
  - [ ] TimeSlot (enum: Setup, EventStart, Cleanup)
  - [ ] Duration
  - [ ] AssignedTo (FK to ApplicationUser)
  - [ ] Notes

- [ ] **Schedule CRUD Operations**
  - [ ] Add/Edit/Delete schedule items
  - [ ] Drag-and-drop reordering
  - [ ] Auto-generate schedule from event template
  - [ ] Assign volunteers to schedule items
  - [ ] **NEW** Schedule Management UI
    - [ ] Visual timeline builder
    - [ ] Conflict detection
    - [ ] Schedule templates
    - [ ] Copy schedule from source events

#### 2.3 Rules & Guidelines System
- [ ] **EventRule Model**
  - [ ] RuleId (PK)
  - [ ] EventId (FK to Event)
  - [ ] Description
  - [ ] AppliesTo (enum: Volunteers, Attendees, Both)
  - [ ] IsActive
  - [ ] CreatedDate
  - [ ] UpdatedDate
  - [ ] RuleTemplates (FK to global rule templates)
  - [ ] SequenceOrder

- [ ] **Rule Management UI**
  - [ ] Safety requirements
  - [ ] Age restrictions  
  - [ ] Weather policies
  - [ ] Rule templates
  - [ ] Copy rules from source events

### 🤝 **Phase 3: Volunteer & Integration Systems** ⏱️ *Week 5-6*
**Status: ⏳ Planned**

#### 3.1 Excel Import System (SignUpGenius Alternative)
- [ ] **Excel Import Models**
- [ ] **Import Functionality**
  - [ ] Excel file upload and parsing
  - [ ] Data validation and cleanup
  - [ ] Volunteer status tracking
  - [ ] Duplicate detection
  - [ ] Compare with previous event volunteers

#### 3.2 Volunteer Role Management
- [ ] **EventVolunteerRole Model**
  - [ ] RoleId (PK)
  - [ ] EventId (FK to Event)
  - [ ] RoleName
  - [ ] Description
  - [ ] Capacity
  - [ ] SignUpGeniusTemplateLink
  - [ ] GivebacksGoal
  - [ ] CustomFields (JSON)

- [ ] **Role Management UI**
  - [ ] Create/edit roles within events
  - [ ] Assign volunteers to roles
  - [ ] Role-specific requirements and permissions
  - [ ] Integration with SignUpGenius and Givebacks

#### 3.3 Volunteer Management Interface
- [ ] **Volunteer Dashboard**
  - [ ] Role requirements vs. signups
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
  - [ ] Volunteer retention rates between copies
  - [ ] Fundraising performance tracking
  - [ ] Success factor analysis

- [ ] **Template Effectiveness**
  - [ ] Track which events are copied most frequently
  - [ ] Success metrics for template-based events
  - [ ] Template optimization recommendations

#### 4.2 Google Cloud Integration Prep
- [ ] **Email System Foundation**
  - [ ] Email service interface
  - [ ] Template-based email generation
  - [ ] Coordinator notification system
  - [ ] Approval workflow notifications
  - [ ] Volunteer communication tools

- [ ] **Document Management**
  - [ ] **EventDocument Model (ENHANCED)**
  - [ ] DocumentId (PK)
  - [ ] EventId (FK to Event)
  - [ ] DocumentType (enum: Flyer, Form, Report, etc.)
  - [ ] FilePath
  - [ ] UploadedBy (FK to ApplicationUser)
  - [ ] UploadDate
  - [ ] AccessLevel (enum: Public, Private, Restricted)
  - [ ] RelatedTo (FK to Event, if applicable)
  - [ ] TemplateId (FK to DocumentTemplate, if applicable)
  - [ ] Status (enum: Active, Archived)

- [ ] **Document Management UI**
  - [ ] Upload, edit, delete documents
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

#### 5.2 Event Analytics Dashboard
- [ ] **Performance Tracking**
  - [ ] Event success scoring system
  - [ ] Volunteer satisfaction metrics
  - [ ] Fundraising goal tracking
  - [ ] Attendance vs. projections

#### 5.3 Automated Workflows
- [ ] **Smart Reminders**
  - [ ] Approval deadline notifications
  - [ ] Volunteer recruitment reminders
  - [ ] Event preparation checklists
  - [ ] Post-event feedback collection

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
  - [ ] Mobile approval workflow
  - [ ] Push notifications for approvals
- [ ] **Advanced Analytics**
  - [ ] Predictive event success modeling
  - [ ] Volunteer retention prediction
  - [ ] Optimal scheduling recommendations

## 📁 File Structure Plan (UPDATED)

## 🎯 Current Sprint Focus (UPDATED)

### **Sprint 1** (This Week)
1. ✅ Enhanced Event Model + Migration
2. 🔄 **NEW** Updated EventStatus enum with approval workflow
3. 🔄 **NEW** Basic copy functionality in EventsCreate.razor
4. 🔄 Updated EventsController with approval endpoints

### **Next Sprint** (Week 2)
1. 📝 **NEW** Approval workflow UI (EventApprovals.razor)
2. 📝 **NEW** School year navigation system
3. 📝 **NEW** Event copy system (API + UI)
4. 📝 Individual event detail pages

### **Sprint 3** (Week 3)
1. 📝 File upload system
2. 📝 Station management MVP (with copy support)
3. 📝 Enhanced approval notifications
4. 📝 Event analytics dashboard

## 📊 Progress Tracking (UPDATED)

- **Phase 1**: 🟡 35% Complete (Enhanced with new features)
- **Phase 2**: ⏳ Not Started
- **Phase 3**: ⏳ Not Started  
- **Phase 4**: ⏳ Not Started
- **Phase 5**: ⏳ Not Started
- **Phase 6**: 📋 Future

## 🔧 Technical Specifications (UPDATED)

### Database Schema Changes

### Security & Permissions (UPDATED)
- **Event Coordinators**: Can create/edit assigned events, view approval status
- **Board Members**: Can approve PTA events, view all pending approvals
- **Admins**: Can approve all events, manage approval workflow
- **Principal**: Can approve school-related events (future enhancement)
- **Public**: Can only view Active, InProgress, WrapUp, and Completed events

## 🤔 Updated Open Questions & Decisions

1. **Approval Authority**: Should Principal approval be required for school events vs. PTA events?
2. **Auto-Approval**: Should certain event types or coordinators have auto-approval privileges?
3. **Approval Deadlines**: How far in advance must events be submitted for approval?
4. **Copy Permissions**: Who can copy events? Original coordinators vs. anyone?
5. **School Year Transitions**: When should the system switch to showing the next school year?
6. **Notification Preferences**: Email vs. in-app notifications for approval workflow?
7. **Bulk Approvals**: Should board members be able to approve multiple events at once?

## 📝 Updated Implementation Notes

- **Approval-First Design**: All new events automatically submit for approval upon creation
- **Copy-Friendly Architecture**: Every component supports copying data from source events
- **School Year Context**: All views can be filtered by school year for better organization
- **Progressive Enhancement**: Start with basic approval workflow, add complexity later
- **Audit Trail**: Complete history of all approval actions for transparency
- **Performance Focus**: Track success metrics to improve future event planning

## 🎪 Enhanced Sample Event Structure Example

---

**Last Updated**: 09/15/25 
**Current Sprint**: Phase 1 - Foundation, Copy System & Approval Workflow
**Next Review**: After approval workflow implementation  
**Repository**: [LuxfordPTABlazor](https://github.com/jedelfraisse/LuxfordPTABlazor)