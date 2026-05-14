# 🎭 TalentShowControl‑Show.md  
*A helper‑focused workspace for running the live Talent Show using the finalized Segment Timeline produced during Rehearsals.*

## 📌 Purpose  
The Show module is the **runtime engine** for the live Talent Show.

It does **not** build the show.  
It **runs** the show using the fully‑prepared Segment Timeline created during Rehearsals.

The Show module provides:

- Manual segment advancement  
- Skip/reorder/insert (no delete)  
- Display instructions for Host, Backstage, MainBoard, and Timer  
- Judges voting flow  
- Audience voting flow  
- Media playback  
- Cue triggering  
- Timing warnings  
- Backstage and FOH prompts  
- Hidden segment handling  
- Unlimited intermissions  

This is the final step in the Talent Show workflow.

---

# 1. 🔧 Configure (Show Runtime Setup)

The Configure section loads the finalized Segment Timeline and prepares the runtime environment.

---

## 1.1 Load Final Segment Timeline  
The Show loads the segments produced during Rehearsals.

### Segment Types  
- **Act** (with ActId + Tech Requirements)  
- **Act Intro** (attached to Act)  
- **Host Announcement**  
- **Intermission**  
- **Award Segment**  
- **Sponsor Loop**  
- **Intro Video**  
- **Backstage Reset**  
- **Hidden Segment**  
- **Custom Segment**  

### Data  
```
Segment[] {
  SegmentId
  SegmentType
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
  TechRequirements (if Act segment)
  ExpectedDurationSeconds
}
```

---

## 1.2 Display Role Assignment  
The Show runtime supports four display roles:

- **MainBoard** (audience)  
- **BackstageDirector**  
- **HostTeleprompter**  
- **ActTimer**  

### Behavior  
- Each display connects via a unique URL  
- Displays update automatically when the Director advances segments  
- Hidden segments do not update MainBoard  

---

## 1.3 Voting Configuration  
### Judges Voting  
- Judges receive a private URL  
- Judges can only vote on the **current** act  
- Judges cannot skip ahead  
- Judges must submit before advancing  
- Director controls voting window  

### Audience Voting  
- Optional (based on Planning configuration)  
- QR code shown on MainBoard  
- Public voting page  
- Director controls voting window  

---

## 1.4 Media & Cue Preload  
Before the show begins:

- All media files are preloaded  
- All cues are validated  
- All display instructions are cached  
- Timer presets are loaded  

---

# 2. 🏃 Live (Show Runtime Engine)

This is the core of The Show module.  
Everything here happens during the live event.

---

## 2.1 Segment Timeline View  
Shows the full list of segments in order.

### Columns  
- Order  
- Segment type  
- Title  
- Act (if applicable)  
- Status (Waiting, Live, Completed, Skipped)  
- Duration (expected vs actual)  
- Media status  
- Cue status  

### Actions  
- **Start Segment**  
- **Next Segment** (manual advance only)  
- **Skip Segment** (moves to bottom)  
- **Reorder** (allowed anytime)  
- **Insert New Segment**  
- **Restore Skipped Segment**  
- **Jump to Segment** (Director override)  

### Rules  
- Segments cannot be deleted  
- Act Intro stays attached to Act  
- No two intros may appear together  
- Hidden segments never appear on MainBoard  

---

## 2.2 Segment Live View  
When a segment is active, the Director sees:

### Segment Metadata  
- Segment type  
- Title  
- Act info (if applicable)  
- Tech Requirements summary  
- Media file  
- Cue list  
- Timer preset  

### Display Instructions  
- Host Teleprompter text  
- Backstage instructions  
- MainBoard content  
- Timer behavior  

### Controls  
- Trigger cues  
- Play/pause media  
- Start/stop timer  
- Send “Prepare Next Act” prompt  
- Open judges voting  
- Open audience voting  
- Close voting  

---

## 2.3 Display Behavior  
Each display updates automatically when a segment becomes active.

### Host Teleprompter  
Shows:  
- Host script  
- Act intro text  
- Award text  
- Announcement text  
- Transition text  

### Backstage Director  
Shows:  
- Props needed  
- Staging needs  
- Music source  
- Setup/teardown instructions  
- Safety notes  
- “Next Act Tech Sheet” (auto + manual override)  

### MainBoard  
Shows:  
- Act title card  
- Intermission screen  
- Sponsor loop  
- Award slide  
- Voting QR code  
- Nothing for hidden segments  

### Act Timer  
Shows:  
- Timer preset  
- Start/stop controls (Director only)  
- Over‑time warning  

---

## 2.4 Timing & Warnings  
The Show runtime provides:

### Features  
- Manual timer start/stop  
- Auto‑load preset duration  
- Over‑time warnings (no enforcement)  
- Warnings appear on:
  - Director  
  - Backstage  
  - Host Teleprompter  
  - Timer  

### Data  
```
ActualDurationSeconds
RunsLong (bool)
```

---

## 2.5 Judges Voting Flow  
### Behavior  
- Judges see only the current act  
- Judges cannot skip ahead  
- Judges must submit before advancing  
- Director opens/closes voting  
- Judges see “Next Act” only after Director advances  

### Data  
```
JudgeVote {
  JudgeId
  ActId
  Score
  Notes
}
```

---

## 2.6 Audience Voting Flow  
### Behavior  
- Optional  
- QR code shown on MainBoard  
- Public voting page  
- Director opens/closes voting  
- Voting tied to ActId  

### Data  
```
AudienceVote {
  ActId
  UserId (anonymous or hashed)
  Score or Choice
}
```

---

## 2.7 Skip, Reorder, Insert  
### Skip  
- Moves segment to bottom  
- Cannot skip Act Intro separately from Act  

### Reorder  
- Allowed anytime  
- Act Intro stays attached to Act  

### Insert  
- Allowed anytime  
- Common inserts:
  - Emergency announcement  
  - Extra intermission  
  - Backstage reset  
  - Sponsor message  

---

## 2.8 Hidden Segments  
Hidden segments are internal‑only.

### Behavior  
- Never shown on MainBoard  
- Shown on Backstage + Host Teleprompter  
- Used for:
  - Quiet cues  
  - Backstage prep  
  - Stage resets  
  - Safety checks  

---

## 2.9 End of Show Summary  
After the final segment:

- Total segments completed  
- Total acts performed  
- Judges results  
- Audience results  
- Timing summary  
- Media issues  
- Cue issues  
- Export full show log  

---

# 3. 🔗 Integration With Rehearsals

### Rehearsals → Show  
The Show loads:

- Final Segment Timeline  
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

The Show does **not** modify segment content — only order and runtime behavior.

---

# 4. 🧭 Navigation

From the Director Hub:

```
[ The Show ]
  Configure → /admin/talent-show/show/configure/{eventId}
  Live      → /admin/talent-show/show/live/{eventId}
  Public    → /talent-show/{eventId}
```
