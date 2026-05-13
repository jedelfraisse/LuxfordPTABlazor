# 🎭 TalentShowControlPlan.md
*A modular, state‑driven control system for managing Talent Show events in the Luxford PTA platform.*

## 📌 Overview
The Talent Show Control System provides a complete, real‑time operational framework for planning, auditioning, rehearsing, and running a live Talent Show. It integrates with the existing Event + EventDay architecture and the Real‑Time Event Controls framework (Phase 2.5).

This document defines:
- Top‑level Talent Show lifecycle states  
- Operational sub‑states for the live show  
- Required data models  
- Controls and display roles  
- SignalR interactions  
- API surface  
- Implementation order  

---

# 1. 🎬 Talent Show Lifecycle (Top-Level States)

The Talent Show progresses through **five major phases**, each with unique data, controls, and UI requirements.

## 1.1 Planning
**Purpose**
- Define show structure  
- Determine number of acts and judges  
- Create categories and rules  
- Build preliminary schedule  

**Data**
- TargetActCount  
- JudgeCount  
- Categories  
- Rules & Guidelines  
- PreliminarySchedule  
- SignupFormLink  

**Active Controls**
- AgendaControl  
- NoteTakerControl  
- SponsorDisplayControl (optional)

**Displays**
- None (optional sponsor loop)

---

## 1.2 Try-Outs (Auditions)
**Purpose**
- Collect submissions  
- Schedule auditions  
- Record attendance  
- Capture notes and scores  
- Select performers for the show  

**Data**
- PerformerSubmissions  
- AuditionSchedule  
- AuditionNotes  
- Scores  
- SelectedForShow flag  

**Active Controls**
- TalentAuditionControl  
- NoteTakerControl  
- ScheduleControl  

**Displays**
- Backstage “Now Auditioning”  
- Hallway “Auditions Today”  
- Timer (optional)

---

## 1.3 Rehearsals
**Purpose**
- Finalize act order  
- Test media  
- Time each act  
- Prepare host script  
- Prepare backstage flow  

**Data**
- FinalPerformerList  
- ActOrder  
- DurationPerAct  
- MediaFiles  
- HostScript  
- SpecialRequirements  

**Active Controls**
- TalentShowControl (Rehearsal Mode)  
- ActTimerControl  
- PerformerBackstageControl  
- HostTeleprompterControl  

**Displays**
- Backstage “Up Next”  
- Performer countdown timer  
- Host teleprompter  
- Main projector (test mode)

---

## 1.4 Show (Operational Sub‑States)
The live show uses a **state machine** inside the TalentShowControl.

### Sub‑States
- **PreShow**  
- **HostTalk**  
- **ActShow**  
- **PauseIntermission**  

### PreShow
**Purpose**
- Pair displays  
- Confirm readiness  
- Show lineup  
- Warm up audience  

### HostTalk
**Purpose**
- Host script  
- Next act preview  
- Optional sponsor banner  

### ActShow
**Purpose**
- Display current act  
- Timer countdown  
- Media playback  
- Voting open/closed  

### PauseIntermission
**Purpose**
- Intermission board  
- Countdown  
- Next segment preview  

---

## 1.5 PostShow
**Purpose**
- Show results  
- Thank sponsors  
- Upload media  
- Archive data  
- Collect feedback  

**Data**
- FinalStandings  
- Feedback  
- MediaUploads  
- LessonsLearned  

**Active Controls**
- NoteTakerControl  
- SlideshowControl  
- SponsorDisplayControl  

**Displays**
- Recap slideshow  
- Sponsor loop  

---

# 2. 🧩 Controls Used in Talent Show

## 2.1 TalentAuditionControl
- Manage audition schedule  
- Mark attendance  
- Capture notes and scores  
- Select performers  

## 2.2 TalentShowControl
- Core live show engine  
- Manages operational sub‑states  
- Controls transitions  
- Drives all display roles  

## 2.3 PerformerBackstageControl
- Shows “Up Next”  
- Shows readiness indicators  
- Shows cues  

## 2.4 HostTeleprompterControl
- Host script  
- Cues  
- Timing  

## 2.5 ActTimerControl
- Countdown timer  
- Visual + numeric  

## 2.6 KaraokeVideoControl (optional)
- Plays performer media  
- Supports audio/video sync  

## 2.7 SponsorDisplayControl
- Sponsor loop  
- PreShow + Intermission + PostShow  

---

# 3. 🖥️ Display Roles

## 3.1 MainBoard
- Current act  
- Transitions  
- Intermission  
- Voting status  

## 3.2 BackstageDirector
- Next act  
- Readiness  
- Cues  

## 3.3 HostTeleprompter
- Script  
- Cues  
- Timing  

## 3.4 ActTimer
- Countdown only  

## 3.5 JudgesVote
- Voting UI  
- Act info  

## 3.6 AudienceVote
- Simple vote UI  

---

# 4. 🗄️ Data Models

## 4.1 TalentShowSessionState
```
Id  
EventId  
CurrentState (enum: Planning, TryOuts, Rehearsal, PreShow, HostTalk, ActShow, Intermission, PostShow)  
CurrentActId  
NextActId  
HostScriptPointer  
IntermissionEndTime  
VotingOpen (bool)  
LastUpdated  
```

## 4.2 TalentShowAct
```
ActId  
EventId  
PerformerName  
Title  
DurationSeconds  
OrderIndex  
MediaFilePath  
Notes  
SelectedForShow (bool)  
```

## 4.3 TalentShowVote
```
VoteId  
EventId  
ActId  
DeviceId/UserId  
Timestamp  
```

## 4.4 TalentShowDisplayAssignmentHistory
```
AssignmentId  
EventId  
DisplayCode  
Role  
AssignedAt  
UnassignedAt  
```

---

# 5. 🔌 SignalR Events

## 5.1 Director → Displays
- `stateUpdated`  
- `actChanged`  
- `timerStarted`  
- `timerStopped`  
- `votingOpened`  
- `votingClosed`  

## 5.2 Displays → Director
- `displayReady`  
- `voteSubmitted`  
- `mediaError`  

---

# 6. 🌐 API Endpoints

## 6.1 Session State
```
GET /api/talentshow/{eventId}/state  
POST /api/talentshow/{eventId}/state  
POST /api/talentshow/{eventId}/advance  
POST /api/talentshow/{eventId}/open-voting  
POST /api/talentshow/{eventId}/close-voting  
```

## 6.2 Acts
```
GET /api/talentshow/{eventId}/acts  
POST /api/talentshow/{eventId}/acts  
PUT /api/talentshow/{eventId}/acts/{actId}  
DELETE /api/talentshow/{eventId}/acts/{actId}  
POST /api/talentshow/{eventId}/acts/reorder  
```

## 6.3 Voting
```
POST /api/talentshow/{eventId}/vote  
GET /api/talentshow/{eventId}/results  
```

---

# 7. 🚀 Implementation Order

## Phase 1 — Persistence Layer
- Schema + migration  
- CRUD for acts  
- CRUD for session state  

## Phase 2 — Director API
- Load/save state  
- Advance act  
- Open/close voting  

## Phase 3 — Workbench Integration
- Director UI reads/writes DB-backed state  
- State transitions trigger hub events  

## Phase 4 — Display Rendering
- Role-specific rendering  
- Hub pushes deltas  

## Phase 5 — Voting + Results
- Audience + judges voting  
- Aggregation + final standings  

---

# 8. 📦 Future Enhancements
- Multi-judge scoring modes  
- Weighted scoring  
- Audience QR code voting  
- Automated media preloading  
- Backstage check-in system  
- Multi-night shows  
- Multi-category finals  

---

# 🎉 End of TalentShowControlPlan.md