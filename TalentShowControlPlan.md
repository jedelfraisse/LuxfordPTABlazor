# 🎭 TalentShowControlPlan.md  
*A modular, sub‑event–driven control system for managing Talent Show events in the Luxford PTA platform.*

## 📌 Overview  
The Talent Show Control System is built around the idea that a Talent Show is a collection of **sub‑events**, each with its own configuration and runtime workflow:

- **Planning Meetings**  
- **Try‑Out Sessions**  
- **Rehearsal Sessions**  
- **The Show (Live Event)**  

The **Director Dashboard** acts as the hub, allowing the director to configure or run any sub‑event.  
Each sub‑event can occur **multiple times**, has its own data, and exposes its own controls.

This document defines:

- Sub‑event architecture  
- Sign‑Up workflow  
- Display Setup control  
- Configuration vs runtime separation  
- Show timeline model  
- Controls and display roles  
- Data models  
- SignalR interactions  
- API surface  
- Implementation order  

---

# 1. 🎬 Director Dashboard (Top-Level Hub)

When the Director opens, it shows four sub‑event controllers:

```
[ Planning Meetings ]   — Configure | Run
[ Try-Outs ]            — Configure | Run
[ Rehearsals ]          — Configure | Run
[ The Show ]            — Configure | Run
```

Additional global tools:

```
[ Sign-Up Page ]        — Configure | Preview
[ Display Setup ]       — Configure | Run
```

Each sub‑event:

- Can be scheduled more than once  
- Has its own configuration panel  
- Has its own runtime panel  
- Has notes and unresolved questions  
- Integrates with displays and real‑time controls  

---

# 2. 🧩 Sub‑Event Architecture

## 2.1 Planning Meetings  
**Purpose**  
To define the show, gather decisions, and track open questions.

**Configuration Includes**  
- Meeting schedule  
- Notes  
- Unanswered questions  
- Event metadata (acts, judges, categories, rules)  
- Public schedule items  

**Runtime Includes**  
- Live note-taking  
- Decision tracking  
- Agenda progression  

---

## 2.2 Try‑Out Sessions  
**Purpose**  
To audition performers and select acts for the show.

**Configuration Includes**  
- Try‑out session schedule  
- Slot length  
- Max auditions  
- Public posting toggle  

**Runtime Includes**  
- List of sign‑ups  
- Marking performers: Arrived → Performing → Done  
- Notes and scoring  
- Approve/deny for show  
- Auto‑populate the Act List  

**Displays**  
- “Now Auditioning”  
- “Auditions Today”  
- Optional timer  

---

## 2.3 Rehearsal Sessions  
**Purpose**  
To test the show configuration and adjust it.

**Configuration Includes**  
- Rehearsal schedule  
- Generated rehearsal order  
- Media checks  
- Host script preparation  

**Runtime Includes**  
- Mark acts as rehearsed  
- Adjust order  
- Capture timing  
- Fix issues before the show  

**Displays**  
- Backstage “Up Next”  
- Host teleprompter  
- Act timer  
- Main projector (test mode)  

---

## 2.4 The Show (Live Event)  
**Purpose**  
To run the actual Talent Show using a flexible timeline.

### Show Timeline Model  
The show uses a **repeatable segment timeline**:

Segment types:

- **Pre‑Show** (only one)  
- **Host Talk**  
- **Act** (dropdown of acts not yet added)  
- **Intermission**  
- **Post‑Show** (only one)  

The director can:

- Add segments  
- Reorder segments  
- Edit segment details  
- Remove segments  

### Runtime Includes  
- Current segment  
- Next segment  
- Timers  
- Media playback  
- Voting controls  
- Backstage cues  
- Display management  
- “Advance to next segment”  

**Displays**  
- MainBoard  
- BackstageDirector  
- HostTeleprompter  
- ActTimer  
- JudgesVote  
- AudienceVote  

---

# 3. 📝 Sign-Up Page (Public-Facing)

## Purpose  
To allow students/parents to register acts for the Talent Show.

## Features  
- Public URL  
- Name of performer(s)  
- Grade / Teacher  
- Act title  
- Act description  
- Media upload (optional)  
- Special requirements  
- Contact email/phone  
- Confirmation email  

## Director Tools  
- View all sign-ups  
- Approve/deny  
- Convert approved sign-ups into **TalentShowAct** entries  
- Assign try‑out slots  
- Export list  

## Data Model  
```
SignupId  
EventId  
PerformerName  
Grade  
Teacher  
ActTitle  
ActDescription  
MediaFilePath  
SpecialRequirements  
ContactEmail  
ContactPhone  
Approved (bool)  
CreatedAt  
```

---

# 4. 🖥️ Display Setup Control

## Purpose  
To pair physical displays with their roles before any sub‑event runs.

## Roles  
- MainBoard  
- BackstageDirector  
- HostTeleprompter  
- ActTimer  
- JudgesVote  
- AudienceVote  

## Features  
- Generate display pairing codes  
- Display enters code → becomes assigned  
- Director sees list of connected displays  
- Test mode:
  - Show test image  
  - Test audio  
  - Test countdown  
  - Test teleprompter scroll  
- Unassign displays  
- History of assignments  

## Data Model  
```
DisplayId  
EventId  
DisplayCode  
Role  
AssignedAt  
UnassignedAt  
LastSeen  
```

---

# 5. 🖥️ Controls

## 5.1 TalentAuditionControl  
- Manage audition schedule  
- Mark attendance  
- Capture notes and scores  
- Select performers  

## 5.2 TalentRehearsalControl  
- Rehearsal order  
- Timing  
- Media testing  
- Host script preview  

## 5.3 TalentShowControl (Live Engine)  
- Drives show timeline  
- Manages transitions  
- Controls displays  
- Opens/closes voting  

## 5.4 PerformerBackstageControl  
- “Up Next”  
- Readiness indicators  
- Cues  

## 5.5 HostTeleprompterControl  
- Script  
- Cues  
- Timing  

## 5.6 ActTimerControl  
- Countdown timer  

## 5.7 SponsorDisplayControl  
- Sponsor loop  
- Pre‑Show, Intermission, Post‑Show  

---

# 6. 🗄️ Data Models

## 6.1 SubEvent  
```
SubEventId  
EventId  
Type (Planning, TryOuts, Rehearsal, Show)  
Name  
StartTime  
EndTime  
IsPublic  
Notes  
```

## 6.2 ShowSegment  
```
SegmentId  
ShowId  
SegmentType (PreShow, HostTalk, Act, Intermission, PostShow)  
ActId (nullable)  
OrderIndex  
DurationSeconds  
Notes  
```

## 6.3 TalentShowAct  
```
ActId  
EventId  
PerformerName  
Title  
DurationSeconds  
OrderIndex  
MediaFilePath  
Notes  
SelectedForShow  
```

## 6.4 TalentShowSessionState  
```
Id  
EventId  
CurrentSegmentId  
VotingOpen  
TimerState  
LastUpdated  
```

## 6.5 TalentShowVote  
```
VoteId  
EventId  
ActId  
DeviceId/UserId  
Timestamp  
```

## 6.6 TalentShowDisplayAssignmentHistory  
```
AssignmentId  
EventId  
DisplayCode  
Role  
AssignedAt  
UnassignedAt  
```

---

# 7. 🔌 SignalR Events

## Director → Displays  
- `segmentChanged`  
- `timerStarted`  
- `timerStopped`  
- `votingOpened`  
- `votingClosed`  
- `displayUpdate`  

## Displays → Director  
- `displayReady`  
- `voteSubmitted`  
- `mediaError`  

---

# 8. 🌐 API Endpoints

## Sign-Ups  
```
GET /api/talentshow/{eventId}/signups  
POST /api/talentshow/{eventId}/signups  
PUT /api/talentshow/{eventId}/signups/{signupId}  
DELETE /api/talentshow/{eventId}/signups/{signupId}  
```

## Display Setup  
```
POST /api/talentshow/{eventId}/display/pair  
GET /api/talentshow/{eventId}/display/list  
POST /api/talentshow/{eventId}/display/unassign  
```

## Sub‑Events  
```
GET /api/talentshow/{eventId}/subevents  
POST /api/talentshow/{eventId}/subevents  
PUT /api/talentshow/{eventId}/subevents/{subEventId}  
DELETE /api/talentshow/{eventId}/subevents/{subEventId}  
```

## Show Segments  
```
GET /api/talentshow/{eventId}/show/{showId}/segments  
POST /api/talentshow/{eventId}/show/{showId}/segments  
PUT /api/talentshow/{eventId}/show/{showId}/segments/{segmentId}  
DELETE /api/talentshow/{eventId}/show/{showId}/segments/{segmentId}  
POST /api/talentshow/{eventId}/show/{showId}/segments/reorder  
```

## Acts  
```
GET /api/talentshow/{eventId}/acts  
POST /api/talentshow/{eventId}/acts  
PUT /api/talentshow/{eventId}/acts/{actId}  
DELETE /api/talentshow/{eventId}/acts/{actId}  
```

## Voting  
```
POST /api/talentshow/{eventId}/vote  
GET /api/talentshow/{eventId}/results  
```

---

# 9. 🚀 Implementation Order

## Phase 1 — Persistence  
- SubEvent model  
- ShowSegment model  
- SignUp model  
- DisplayAssignment model  

## Phase 2 — Director Dashboard  
- List sub‑events  
- Configure | Run links  
- Sign‑Up preview  
- Display Setup  

## Phase 3 — Configuration Panels  
- Planning  
- Try‑Outs  
- Rehearsal  
- Show timeline editor  
- Sign‑Up configuration  

## Phase 4 — Runtime Panels  
- Try‑Outs runtime  
- Rehearsal runtime  
- Show runtime engine  
- Display Setup runtime  

## Phase 5 — Displays + SignalR  
- Role-specific rendering  
- Real‑time updates  

## Phase 6 — Voting + Results  
- Audience + judges voting  
- Aggregation + final standings  

---

# 10. 📦 Future Enhancements  
- Multi‑judge scoring  
- Weighted scoring  
- QR code audience voting  
- Automated media preloading  
- Backstage check‑in  
- Multi‑night shows  
- Category finals  

# 11. 🛠️ Admin UI Refactor Requirements

The updated Talent Show architecture represents a structural pivot from the current stage‑based admin UI. The existing implementation must be refactored to support the new **Director Hub + Sub‑Event** model.

## 11.1 Director Page → Hub Model  
**Current:**  
`/admin/eventcontrols/talent-show/{eventId}` loads a single lifecycle stage panel.

**Required:**  
Replace with a **Director Hub** showing cards for each sub‑event:

```
[ Planning Meetings ]   — Configure | Run
[ Try-Outs ]            — Configure | Run
[ Rehearsals ]          — Configure | Run
[ The Show ]            — Configure | Run
```

This becomes the new entry point for all Talent Show operations.

---

## 11.2 Stage Model → Sub‑Event Model  
**Current:**  
A single `TalentShowLifecycleState` enum drives the entire event.

**Required:**  
Introduce a **SubEvent** model that supports multiple instances of:

- Planning Meetings  
- Try-Out Sessions  
- Rehearsal Sessions  
- Shows  

Each sub‑event has its own configuration and runtime workspace.

---

## 11.3 Add Global Tools to the Director Hub  
Two new tools must be added as first‑class sections:

- **Sign-Up Page** — Configure | Preview  
- **Display Setup** — Configure | Run  

These are global and not tied to a specific sub‑event.

---

## 11.4 Show Runtime → Segment Timeline  
**Current:**  
The live show uses a fixed internal state machine:  
PreShow → HostTalk → ActShow → Intermission → PostShow.

**Required:**  
Replace with a **ShowSegment timeline**:

- PreShow (single)  
- Host Talk (repeatable)  
- Act (repeatable)  
- Intermission (repeatable)  
- PostShow (single)  

Segments must be addable, removable, and reorderable.

---

## 11.5 Routing & Labels Must Reflect the New Model  
**Current:**  
Routes and UI labels reference “stages” and “lifecycle states.”

**Required:**  
Rename or replace:

- `talent-show-stages` → `talent-show-director`  
- Stage panels → Sub‑Event Configuration  
- Live state machine → Show Timeline  

The admin UI must reflect the new architecture.

---

## 11.6 Summary Table

| Area | Current | Required |
|------|---------|----------|
| Director Entry | Single stage panel | Multi-card Director Hub |
| Event Flow | Single lifecycle enum | Multiple sub-events |
| Global Tools | None | Sign-Up + Display Setup |
| Show Runtime | Fixed state machine | Reorderable segment timeline |
| Routing | Stage-based | Sub-event + timeline workspaces |

---

## 11.7 Migration Notes  
- Keep `TalentShowLifecycleState` for backward compatibility but mark as deprecated.  
- Introduce `SubEvent` and `ShowSegment` tables.  
- Convert existing stage panels into configuration panels.  
- Refactor live show control to operate on segments.  
- Move display pairing into the new Display Setup tool.

