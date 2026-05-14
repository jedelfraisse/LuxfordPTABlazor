# 🎭 TalentShowControl‑ActTechnicalRequirements.md  
*A unified technical requirements sheet that follows each act from Sign‑Up through Try‑Outs, Rehearsals, and the Live Show.*

## 📌 Purpose  
Every act in the Talent Show requires specific technical preparation.  
This document defines the **Act Technical Requirements** model and how it evolves across the four major phases:

1. **Sign‑Ups** — initial information from performers  
2. **Try‑Outs** — helper review and refinement  
3. **Rehearsals** — verification and finalization  
4. **Show** — runtime prompts for backstage and front‑of‑house  

This ensures that all staff have a consistent, accurate, and complete understanding of what each act needs.

---

# 1. 📝 Data Model: ActTechnicalRequirements

```
ActId  
EventId  

# Music / Audio
MusicNeeded (bool)
MusicSource (PerformerProvided | StaffDownload | None | LiveInstrument)
MusicLink (nullable)
MusicFilePath (nullable)
MusicStartTime (nullable)
MusicEndCue (nullable)
MusicNotes

# Props
PropsList[]  
PropsProvidedBy (Performer | School | Mixed)
PropsSetupNotes  
PropsTeardownNotes  

# Staging
StagingNeeds[] (Chair, Table, MicStand, Spotlight, FloorTape, etc.)
MicrophoneType (Handheld | Stand | Headset | None)
LightingNotes  
StagePositionNotes  

# Safety / Special Requirements
SpecialRequirements  
AccessibilityNeeds  
SafetyConcerns  

# Backstage / Front Stage Notes
BackstageNotes  
FrontStageNotes  

# Status Flags
MusicConfirmed (bool)
PropsConfirmed (bool)
StagingConfirmed (bool)
TechSheetFinalized (bool)
```

---

# 2. 🧩 Phase 1 — Sign‑Ups (Initial Capture)

The Sign‑Up form collects the **first draft** of the tech sheet.

### Fields Collected  
- Does your act require music?  
- How will music be provided?
  - Upload  
  - Provide link  
  - Performer brings device  
  - No music  
- Props you plan to use  
- Who provides the props?  
- Staging needs (chairs, tables, microphones, etc.)  
- Special requirements  
- Safety concerns  
- Accessibility needs  

### Purpose  
This gives helpers an early understanding of what the act will require.

---

# 3. 🎤 Phase 2 — Try‑Outs (Refinement)

During Try‑Outs, helpers refine and validate the technical requirements.

### Helper Actions  
- Confirm music source  
- Verify music link or file  
- Add missing details  
- Flag unsafe or unmanageable props  
- Add tech notes  
- Identify staging issues  
- Mark items needing follow‑up  

### Outputs  
- `MusicConfirmed`  
- `PropsConfirmed`  
- `StagingConfirmed`  
- `TechNotes`  
- `NeedsFollowUp`  

This becomes the **second draft** of the tech sheet.

---

# 4. 🎬 Phase 3 — Rehearsals (Finalization)

Rehearsals finalize the technical requirements for the live show.

### Helper Actions  
- Test music playback  
- Confirm props are ready and safe  
- Verify staging needs  
- Capture cue list  
- Add backstage notes  
- Add front‑of‑house notes  
- Mark tech sheet as finalized  

### Outputs  
- `MusicTested`  
- `PropsReady`  
- `StagingReady`  
- `CueList`  
- `BackstageNotes`  
- `FrontStageNotes`  
- `TechSheetFinalized = true`  

This becomes the **final tech sheet** used during the show.

---

# 5. 🎛️ Phase 4 — Show (Runtime Usage)

During the live show, the system uses the tech sheet to prompt:

### Backstage Crew  
- Props needed for the next act  
- Props teardown from previous act  
- Performer setup time  
- Special handling notes  
- Safety concerns  
- Accessibility needs  

### Front‑of‑House / AV  
- Music file + start time  
- Lighting cues  
- Microphone type  
- Stage position  
- Timing notes  
- End cues  

### Example Runtime Prompt  
```
Next Act: "Samantha — Vocal Solo"
Music: staff-provided (start at 0:12)
Props: 1 stool (school-provided)
Mic: handheld
Backstage: help carry prop box
Front Stage: spotlight center stage
```

These prompts ensure the show runs smoothly and professionally.

---

# 6. 🔗 Integration With Other Helpers

### Sign‑Ups → Try‑Outs  
Initial tech sheet becomes editable by helpers.

### Try‑Outs → Rehearsals  
Refined tech sheet becomes the rehearsal checklist.

### Rehearsals → Show  
Finalized tech sheet drives backstage and FOH prompts.

### Show → Logs  
Tech sheet is included in the final show report.

---

# 7. 🧭 Navigation (Where This Appears)

This file is referenced by:

- **Sign‑Ups** (Configure + Run)  
- **Try‑Outs** (Run)  
- **Rehearsals** (Run)  
- **Show** (Configure + Run)  

It is not a standalone UI, but a shared data model used across helpers.

