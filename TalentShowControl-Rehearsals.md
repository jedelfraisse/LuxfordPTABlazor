# 🎭 TalentShowControl‑Rehearsals.md  
*A helper‑focused workspace for building and refining the full Show Timeline through scheduled rehearsal sessions.*

## 📌 Purpose  
Rehearsals are where the Talent Show is **actually built**.

This is where helpers:

- Finalize **segments** (not just acts)  
- Add host announcements, transitions, intermissions, hidden backstage steps  
- Test media and cues  
- Finalize tech requirements  
- Capture real segment durations  
- Refine display instructions for Host, Backstage, MainBoard, and Timer  
- Reorder or add segments  
- Prepare the final Show Timeline  

Rehearsals operate on **scheduled sessions**, similar to Try‑Outs, but with the goal of producing the **final Show**.

---

# 1. 🔧 Configure (Setup for Rehearsal Helpers)

The Configure section prepares the structure of the rehearsal process and loads the initial segments.

---

## 1.1 Rehearsal Sessions  
Rehearsals are scheduled sub‑events that appear on the Event Schedule.

### Features  
- Add rehearsal sessions  
- Edit session times  
- Remove sessions  
- Assign helpers  
- Sync sessions to Event Schedule  
- Ownership rules:
  - Creator owns the session  
  - Only owner may edit/delete  
  - Director can view all  

### Data  
```
RehearsalSessionId  
EventId  
StartTime  
EndTime  
ScheduleEntryId  
AddedByUserId  
LockedToUserId  
Notes  
```

---

## 1.2 Load Initial Segments  
Segments come from:

- Sign‑Ups (direct-approved generic Act segments)  
- Try‑Outs (selected acts create/confirm segments and refine tech requirements)  
- Walk‑ins (new Act segments created during Try‑Outs)

### Features  
- Import all Act segments  
- Mark segments as “Needs Build‑Out”  
- Allow helpers to add new non‑Act segments:
  - Host announcement  
  - Intro video  
  - Intermission  
  - Award segment  
  - Sponsor loop  
  - Backstage reset  
  - Hidden segment  

### Data  
```
Segment[] {
  SegmentId  
  SegmentType  
  ActId (nullable)  
  OrderIndex  
  IsHidden  
}
```

---

## 1.3 Segment Ordering  
Rehearsals define the **initial Show order**.

### Features  
- Drag‑and‑drop reorder  
- Group Act Intro + Act together  
- Prevent duplicate intros  
- Skip segments (move to bottom)  
- Insert new segments  
- Save multiple rehearsal orders (optional)

---

## 1.4 Segment Requirements Preparation  
Each segment may require:

- Media  
- Cues  
- Tech Requirements (if Act segment)  
- Display instructions  
- Timer presets  

### Features  
- List missing items  
- Flag incomplete segments  
- Prepare helpers for what to test during rehearsal  

---

# 2. 🏃 Sessions (Segment‑Based Rehearsal Workflow)

This is where helpers rehearse each segment and build the final Show Timeline.

---

## 2.1 Segment Queue  
Shows all segments scheduled for the session.

### Columns  
- Segment type  
- Segment title  
- Act (if applicable)  
- Order  
- Status (Waiting, On Stage, Completed)  
- Media status  
- Timing status  
- Tech sheet status  

### Actions  
- Mark On Stage  
- Mark Completed  
- Skip (moves to bottom)  
- Reorder on the fly  
- Add new segment  

---

## 2.2 Segment Editor  
When a segment is selected, helpers can edit:

### Segment Metadata  
- Segment type  
- Title  
- Act reference (if Act segment)  
- Hidden/visible toggle  

### Display Instructions  
- Host Teleprompter text  
- Backstage Director instructions  
- MainBoard content  
- Timer preset (seconds)  

### Media  
- Upload/replace media  
- Mark media as OK  
- Add media notes  

### Cues  
- Add/edit cues  
- Cue types:
  - Lighting  
  - Audio  
  - Display  
  - Stage movement  
  - Teleprompter  

### Tech Requirements (Act segments only)  
- Props  
- Music source  
- Staging needs  
- Safety concerns  
- Backstage notes  
- Front‑of‑house notes  

---

## 2.3 Media Test  
Test media for **any segment**, not just acts.

### Features  
- Play media  
- Mark OK  
- Mark “Needs fix”  
- Add notes  

### Data  
```
MediaTested (bool)  
MediaNotes  
```

---

## 2.4 Timing  
Rehearsals capture real durations for **every segment**.

### Features  
- Start timer  
- Stop timer  
- Save actual duration  
- Compare to expected duration  
- Flag segments running long  

### Data  
```
ExpectedDurationSeconds  
ActualDurationSeconds  
RunsLong (bool)
```

---

## 2.5 Cue Testing  
Helpers test cues attached to the segment.

### Features  
- Trigger cue  
- Mark cue as working  
- Add cue notes  

---

## 2.6 Tech Sheet Finalization (Act Segments Only)  
Finalize the Act Technical Requirements.

### Checklist  
- Music tested  
- Props confirmed  
- Staging confirmed  
- Safety confirmed  
- Backstage notes added  
- FOH notes added  

### Data  
```
TechSheetFinalized (bool)
```

---

## 2.7 Backstage & FOH Instruction Review  
Helpers verify that each segment has:

- Host announcement text  
- Backstage prep instructions  
- MainBoard content  
- Timer preset  
- Transition notes  

This ensures the Show runtime has everything it needs.

---

## 2.8 Add / Remove / Reorder Segments  
Rehearsals allow helpers to:

- Add new segments  
- Insert transitions  
- Insert intermissions  
- Add hidden backstage steps  
- Reorder segments  
- Skip segments (move to bottom)  

Segments cannot be deleted, but they can be skipped.

---

## 2.9 Session Summary  
At the end of the rehearsal:

- Total segments rehearsed  
- Segments missing media  
- Segments running long  
- Segments not ready  
- Acts not ready  
- Export summary  

---

# 3. 🔗 Integration With The Show

Rehearsals produce the **final Show Timeline**.

### Rehearsals → Show  
- Segment order  
- Segment durations  
- Segment media  
- Segment cues  
- Segment display instructions  
- Finalized tech sheets  
- Hidden segments  
- Intermissions  
- Transitions  
- Host scripts  
- Backstage instructions  

The Show module **runs** the timeline created here.

---

# 4. 🧭 Navigation

From the Director Hub:

```
[ Rehearsals ]
  Configure → /admin/talent-show/rehearsals/configure/{eventId}
  Sessions  → /admin/talent-show/rehearsals/sessions/{eventId}
```
