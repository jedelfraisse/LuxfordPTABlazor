# PTA Membership Drive — Single Unit Website Specification (Updated with Membership List + School Year Support)

Claude, please adopt the following rules and generate all scaffolding, components, models, admin pages, and import logic according to this specification. This feature is for a **single PTA unit website**, not a multi-unit portal.

---

# 0. Implementation Status (as built — 2026-08-18)

This feature has been implemented. The sections below are kept as the original design spec, with
the following deviations made during implementation — agreed with the site owner before building:

1. **`SchoolYear` is a real foreign key, not a raw year number.** This codebase already has a
   `SchoolYear` entity (`Id`, `Name` e.g. "2024-2025", `StartDate`/`EndDate`, `Status`, ...) used by
   every other model (`Event`, `BoardPosition`, etc.). Everywhere this spec says `int SchoolYear` or
   `int? ComparisonYear`, the actual model uses `int SchoolYearId` / `int? ComparisonYearId` as a
   foreign key into that table, with the matching navigation property. "Since 2019-2020" and
   "`SchoolYear - 1`" style comparisons are resolved by querying `SchoolYear.StartDate` ordering
   instead of doing arithmetic on a year number.
2. **`/membership-drive` and `/membership` are separate pages.** `/membership` already existed as a
   stub join/signup page; `/membership-drive` (this spec's public page) shows stats and milestones
   and links to `/membership` via its Join button. `/membership` itself still has no working
   join/payment form — that's out of scope for this spec, which only covers admin-managed records
   and CSV import.
3. **CSV import is two endpoints, not one**, so "admin can review before final import" (§2.1) is a
   real step rather than a same-request assumption:
   - `POST /api/membership/import/preview` — parses the file, returns per-row validation errors and
     duplicate flags, **writes nothing**.
   - `POST /api/membership/import/commit` — takes the (admin-reviewed/edited) row list back and
     persists it, with a chosen duplicate strategy (Skip or Overwrite).
4. **`TargetValue` is always admin-entered**, never auto-computed — including for `Staff100Percent`,
   where "total staff count" isn't derivable from membership data alone (there's no roster of
   non-member staff to count against). The one exception is `HistoricalSurpass`, where `TargetValue`
   is optional and defaults to a dynamically computed historical high if left blank.
5. **§6 "Built-In Milestones" are quick-start form templates, not auto-seeded rows.** The admin
   milestone editor has buttons that pre-fill the create form with each of the 5 templates below;
   nothing is silently created in the database for a school year.
6. **`MemberType`/`PaymentType` stayed free-text strings** (not enums) so CSV imports from different
   sources aren't rejected for using different vocabulary. `Status` became a real `MembershipStatus`
   enum (`Active`/`Inactive`) since that value is small and fixed. `IsStaff` is derived at
   import/save time by checking whether `MemberType` contains "staff" or "teacher" (case-insensitive).
7. **Membership records and CSV export are Admin/BoardMember-only with no public fallback** (§9).
   The one public/anonymous exception is aggregate stats (`GET /api/membership/{schoolYearId}/stats`)
   and visible-milestone progress (`GET /api/membership/milestones/{schoolYearId}`) — both contain no
   PII and are what powers the public `/membership-drive` page's live stats and milestone list.
8. **Email is not required — Email OR PhoneNumber is required.** Some members (notably staff) may
   only have a phone number on file. Both CSV import and the manual add/edit form require at least
   one of the two, not specifically Email. Duplicate detection (§2.1) now keys off Email when present,
   falling back to a digits-only PhoneNumber match when it isn't; a row with neither is never flagged
   as a duplicate (there's no reliable identity to compare).
9. **Visibility of not-yet-public school years/milestones is role-based, not Admin/BoardMember-only.**
   Any authenticated user holding one of the app's real roles (`Admin`, `BoardMember`, or
   `Volunteer`) can see hidden/future school years and hidden milestones; only anonymous and
   authenticated-but-roleless visitors are restricted to `IsVisibleToPublic`/visible-only content.
   This is purely a *view* rule — editing (`[Authorize(Roles = "Admin,BoardMember")]` on write
   endpoints) is unchanged.

See §7 for the actual API surface as built (it differs slightly from the original endpoint list).

---

# 1. Feature Purpose

The Membership Drive feature provides:

- A **public-facing Membership Drive page** showing milestones and progress.
- A **back-end admin interface** allowing PTA admins to create, edit, reorder, hide, or delete milestones.
- A **membership list system**, internal-only, with CSV import.
- Automatic calculation of membership counts and milestone progress **per school year**.

---

# 2. Data Models

## 2.1 MembershipRecord

Represents a single PTA member for a specific school year.

**As built** (see §0.1 and §0.6 — `SchoolYearId` is a real FK, `Status` is an enum):

```csharp
public class MembershipRecord
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime JoinDate { get; set; }
    public string MemberType { get; set; }        // Parent/Guardian, Teacher/Staff, Student, etc.
    public int SchoolYearId { get; set; }         // FK -> SchoolYear
    public SchoolYear SchoolYear { get; set; }
    public decimal Price { get; set; }
    public MembershipStatus Status { get; set; }  // Active, Inactive
    public string PaymentType { get; set; }       // cash, credit_card, etc.
    public string? TeacherName { get; set; }       // optional
    public bool IsStaff { get; set; }             // derived from MemberType containing "staff"/"teacher"
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### CSV Import Requirements

- Admin uploads a CSV file.
- System parses fields matching the structure of the uploaded CSV.
- All records are stored under the selected **SchoolYear**.
- Duplicate detection by Email + SchoolYear — falling back to PhoneNumber + SchoolYear when a row
  has no email (see §0.8; Email is not strictly required, but Email or PhoneNumber is).
- Admin can review before final import.
- **As built**: import is two steps — `POST /api/membership/import/preview` parses the file and
  returns validation/duplicate flags per row without writing anything; the admin reviews (can
  uncheck rows, choose a Skip/Overwrite strategy for duplicates), then `POST
  /api/membership/import/commit` persists the reviewed rows. Uses `CsvHelper`, with flexible
  case/punctuation-insensitive header matching so exports from different tools aren't rejected.

---

## 2.2 MembershipMilestone

Milestones must be tied to a **specific school year**.

**As built** (see §0.1 — `SchoolYearId`/`ComparisonYearId` are real FKs):

```csharp
public class MembershipMilestone
{
    public Guid Id { get; set; }
    public int SchoolYearId { get; set; }         // REQUIRED FK -> SchoolYear
    public SchoolYear SchoolYear { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public MilestoneType MilestoneType { get; set; }
    public int? TargetValue { get; set; }         // e.g., 100 staff, 120% growth, 200 members — always admin-entered (see §0.4)
    public int? ComparisonYearId { get; set; }    // FK -> SchoolYear; used for growth baseline or historical-surpass anchor
    public SchoolYear? ComparisonYear { get; set; }
    public bool IsVisible { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## 2.3 MilestoneType Enum

```csharp
public enum MilestoneType
{
    Staff100Percent,
    StaffVolume,
    PercentGrowth,
    TotalMembership,
    HistoricalSurpass,
    Custom
}
```

---

# 3. Automatic Calculations (Per School Year)

All calculations must be scoped to the milestone’s **SchoolYear**.

### 3.1 Current Membership Count
Count all `MembershipRecord` where `SchoolYear == milestone.SchoolYear`.

### 3.2 Staff Membership Count
Count all records where:
- `SchoolYear == milestone.SchoolYear`
- `IsStaff == true`

### 3.3 Percent Growth
Compare milestone’s SchoolYear to `ComparisonYear`.

Formula:
```
(CurrentYearTotal / ComparisonYearTotal) * 100
```

### 3.4 Historical Surpass
Compare current year’s total to the highest membership count since 2019–2020.

---

# 4. Admin Back-End Requirements

## 4.1 Membership List Management

Route:
```
/admin/membership/list
```

Admin can:

- View all members for a selected school year.
- Filter by:
  - MemberType
  - TeacherName
  - Status
  - PaymentType
- Import CSV for a selected school year.
- Export membership list (internal only).
- Edit individual records.
- Delete individual records.

## 4.2 Milestone Management

Route:
```
/admin/membership/milestones
```

Admin can:

- Create milestone (must select SchoolYear)
- Edit milestone
- Toggle visibility
- Reorder milestones
- Delete milestones
- Preview milestone progress

## 4.3 Membership Dashboard

Shows:

- Current membership count (per selected school year)
- Staff membership count
- Growth %
- Historical high
- All milestones with progress bars

---

# 5. Public Front-End Page

Route:
```
/membership-drive
```

### Page Sections

#### A. Header
```
PTA Membership Drive — Help Us Reach Our Goals!
```

#### B. Live Stats (for current school year)
- Current members
- Staff members
- Percent growth
- Historical comparison

#### C. Milestone List
For each visible milestone:
- Title
- Description
- Progress bar
- Current vs target
- Achieved badge

#### D. Join Button
Link to membership signup.

---

# 6. Built-In Milestones (Per School Year)

**As built**: these are "Quick Start" template buttons in the admin milestone editor that pre-fill
the create form — they are not auto-seeded into the database for every school year (see §0.5).

### 1. Behind the Wheel — 100% Faculty
```
MilestoneType: Staff100Percent
TargetValue: TotalStaffCount for SchoolYear
```

### 2. Staff Volume Milestones
```
MilestoneType: StaffVolume
TargetValue: 100, 150, etc.
```

### 3. President’s Trailblazer Award
```
MilestoneType: PercentGrowth
TargetValue: 120 (120% of last year)
ComparisonYear: SchoolYear - 1
```

### 4. General 10% Growth Challenge
```
MilestoneType: PercentGrowth
TargetValue: 110
ComparisonYear: SchoolYear - 1
```

### 5. Total Membership Numerical Surpass
```
MilestoneType: HistoricalSurpass
TargetValue: HighestSince2019
ComparisonYear: 2019
```

---

# 7. API Endpoints

**As built** — see §0.3 and §0.7 for why import and stats differ from the original list. All
`api/membership/...` endpoints are Admin/BoardMember-only except `stats` and the milestones GET,
which are anonymous.

### Membership Records
```
GET    /api/membership/{schoolYearId}                      // supports ?memberType=&teacherName=&status=&paymentType=
GET    /api/membership/{schoolYearId}/stats                 // anonymous — aggregate counts only, no PII
GET    /api/membership/{schoolYearId}/export                // CSV download
POST   /api/membership                                      // create a single record manually
POST   /api/membership/import/preview                       // multipart file upload -> validation/duplicate preview, no writes
POST   /api/membership/import/commit                        // persists reviewed rows
PUT    /api/membership/{id}
DELETE /api/membership/{id}
```

### Milestones
```
GET    /api/membership/milestones/{schoolYearId}            // anonymous; ?includeHidden=true honored only for Admin/BoardMember
POST   /api/membership/milestones
PUT    /api/membership/milestones/{id}
DELETE /api/membership/milestones/{id}
PUT    /api/membership/milestones/reorder
```

---

# 8. Front-End Components

### MembershipListTable
- Paginated table
- Filters
- CSV import button

### MilestoneCard
- Title
- Description
- Progress bar
- Achieved badge

### MilestoneList
- Sorted by SortOrder
- Only visible milestones

### MembershipStatsPanel
- Current members
- Staff members
- Growth %
- Historical high

### AdminMilestoneEditor
- SchoolYear selector
- Milestone type selector
- Target value input
- Comparison year input
- Visibility toggle
- Sort order controls

---

# 9. Permissions

### Admin
- Full CRUD on membership records
- Full CRUD on milestones
- Access to dashboard

### Public
- View membership drive page only

---

# 10. Output Requirements

Claude should:

1. Review this spec and suggest improvements.
2. Generate:
   - Data models and migrations
   - CSV import service
   - Membership record service
   - Milestone evaluation service
   - API controllers
   - Admin UI
   - Public UI
3. Use a clean, modular architecture consistent with the chosen stack.

```
