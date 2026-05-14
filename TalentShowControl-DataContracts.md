# 🎛️ TalentShowControl‑DataContracts.md  
*A unified data model for all helpers in the Talent Show subsystem.*

This file defines the **canonical field names**, **entity relationships**, and **shared structures** used across all Talent Show modules.  
All module files must reference these definitions to avoid drift.

---

# 1. 🎭 Core Entities

## 1.1 Act  
Represents a performer or group performing in the show.

```
Act {
  ActId
  PerformerNames
  Grade
  Teacher
  Title
  Description
  ContactEmail
  ContactPhone
  Category (optional)
  SpecialRequirements
  MediaFile (optional)
}
```

---

## 1.2 Segment  
The atomic unit of the Show Timeline.  
Created when an act is approved with no Try‑Out required, or when selected in Try‑Outs (including walk‑ins).

```
Segment {
  SegmentId
  SegmentType        // Act, ActIntro, HostAnnouncement, Intermission, Award, SponsorLoop, BackstageReset, Hidden, Custom
  ActId (nullable)
  Title
  OrderIndex
  IsHidden

  DisplayInstructions {
    HostTeleprompter
    Backstage
    MainBoard
    TimerPresetSeconds
  }

  MediaFile
  CueList[]
  TechRequirements (nullable)
  ExpectedDurationSeconds
  ActualDurationSeconds
  RunsLong
}
```

---

## 1.3 Tech Requirements (Act Segments Only)  
Canonical structure from **ActTechnicalRequirements.md**.

```
TechRequirements {
  Props[]
  MusicSource
  StagingNeeds
  SafetyNotes
  BackstageNotes
  FrontStageNotes
  MicType
  LightingNeeds
}
```

---

# 2. 📝 Sign‑Ups & Try‑Outs

## 2.1 Sign‑Up Submission  
```
Signup {
  SignupId
  PerformerNames
  Grade
  Teacher
  ActTitle
  ActDescription
  ContactEmail
  ContactPhone
  SpecialRequirements
  MediaUpload (optional)
  Status (Pending, ApprovedDirect, InvitedToTryOuts, Rejected, NeedsMoreInfo)
}
```

## 2.2 Sign‑Up Settings  
```
SignupSettings {
  SignupStart
  SignupEnd
  AutoClose
  ShowCountdown
  // No global try-out mode: review decision is per submission
}
```

---

## 2.3 Try‑Out Entry  
```
TryOutEntry {
  TryOutEntryId
  ActId
  SessionId
  SlotTime
  Notes
  Selected (bool)
}
```

---

# 3. 🎬 Rehearsals

## 3.1 Rehearsal Session  
```
RehearsalSession {
  RehearsalSessionId
  EventId
  StartTime
  EndTime
  ScheduleEntryId
  AddedByUserId
  LockedToUserId
  Notes
}
```

---

## 3.2 Segment Rehearsal Data  
```
SegmentRehearsal {
  SegmentId
  MediaTested (bool)
  MediaNotes
  ExpectedDurationSeconds
  ActualDurationSeconds
  RunsLong (bool)
  CueStatus[]   // per-cue pass/fail
  TechSheetFinalized (bool)
  RehearsalNotes[]
}
```

---

# 4. 🎥 Media & Cues

## 4.1 Media  
```
MediaFile {
  FileId
  FilePath
  FileType
  DurationSeconds (optional)
}
```

## 4.2 Cue  
```
Cue {
  CueId
  SegmentId
  CueType   // Lighting, Audio, Display, StageMovement, Teleprompter
  CueText
  TriggerTime (optional)
}
```

---

# 5. 🗳️ Voting

## 5.1 Judges Voting  
```
JudgeVote {
  JudgeId
  ActId
  Score
  Notes
  SubmittedAt
}
```

## 5.2 Audience Voting  
```
AudienceVote {
  ActId
  UserId (anonymous or hashed)
  Score
  SubmittedAt
}
```

---

# 6. 🖥️ Display Instructions

## 6.1 DisplayInstructions  
```
DisplayInstructions {
  HostTeleprompter
  Backstage
  MainBoard
  TimerPresetSeconds
}
```

---

# 7. 🔗 Relationships Overview

```
Signup → ApprovedDirect → Act → Segment
Signup → InvitedToTryOuts → TryOutEntry → (Selected) → Act → Segment
TryOutEntry → Refines Act + Segment
Rehearsals → Builds Segment
Show → Runs Segment
```

Segments are the backbone of the entire system.

---

# 8. 📦 Naming Conventions

- **CamelCase** for all fields  
- **PascalCase** for entity names  
- **SegmentType** is always a string enum  
- **ActId** is nullable for non‑Act segments  
- **DisplayInstructions** is always present, even if empty  
