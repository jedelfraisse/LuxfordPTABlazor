# 📘 **TalentShowControl‑TryOuts‑Changes.md**  
*Updated with all clarifications and safety rules*

---

# 🎤 **1. Overview**

This document defines the complete, authoritative specification for the **Try‑Outs** module of the Talent Show Control System.  
It includes:

- Configuration  
- Session workflow  
- Queue logic  
- Status transitions  
- Display behavior  
- Judge workflow (mobile‑first)  
- Segment stub creation  
- Sign‑out safety requirements  
- PDF export rules  
- Data model updates  

This file is the master reference for implementation.

---

# ✅ **Implementation Status Checklist (Current)**

## Configure
- [x] General Settings tab fields implemented (Slot Length, Parent Sign-Out Required, Scoring Scale Max, Scoring Fields).
- [x] Sessions tab fields implemented (SessionId, Date, Start, End, Location, PublicVisible, ParentSignOutRequired).
- [x] Sessions Add/Edit/Delete implemented.
- [x] "Show on Live Site" / public visibility toggle implemented.
- [x] List of Sign-Ups tab supports list/filter/check-in/notes/approve/reject.

## Run Session + Queue
- [x] Queue statuses implemented (StandBy, OnDeck, Performing, Completed, Approved, Rejected).
- [x] Status actions implemented in run workspace (Check-In, On Deck, Start, Completed, Approve, Reject).
- [x] Check-In sets StandBy and CheckInTimestamp.
- [x] Session Selection flow implemented in run workspace via active-session selector.
- [x] "One at a time" OnDeck enforcement implemented.
- [x] Queue UI layout implemented (Current Performer, On Deck, StandBy, Completed, collapsed Approved/Rejected).

## Displays + Judge Experience
- [x] Session Workspace Displays tab implemented (Assign displays, Generate judge codes, Preview displays).
- [x] Dedicated Try-Out Judge mobile page implemented with Current/On Deck/Queue/Past tabs.
- [x] Judge completion workflow implemented ("I'm Done" per performer with director visibility in data model).
- [x] MainBoard try-out view implemented (Current + On Deck + Queue Preview).
- [x] Real-time sync implemented across MainBoard/Judge/Sign-In/Director try-out views.

## Approval Flow + Safety + Data
- [x] Approved try-out entries create/reuse rehearsal act and a basic show segment stub.
- [x] Two PDF exports implemented (Parent Sign-Out and Authorized Pickup).
- [x] PDFs include only currently StandBy students.
- [x] Data model updates implemented (PickupAdult, MusicUrl, try-out session fields, scoring, judge completion).

## Event Schedule / Public Visibility
- [x] Try-out sessions sync into EventDays for admin schedule visibility.
- [x] PublicVisible try-out sessions display on public event detail page.

---

# 🎛️ **2. Try‑Outs Module Structure**

Two primary actions:

## **2.1 Configure**
Multi‑tab configuration interface.

## **2.2 Run Session**
Session selection → live session workspace.

---

# 🧩 **3. Configure — Tabs and Fields**

## **3.1 Tab: General Settings**
Fields:
- Slot Length (minutes)  
- Parent Sign‑Out Required (Yes/No)  
- Scoring Scale Max  
- Scoring Fields (list)

Remove all other fields.

---

## **3.2 Tab: Sessions**
Fields:
```
SessionId
Date
StartTime
EndTime
Location
PublicVisible (bool)
ParentSignOutRequired (bool)
```

Features:
- Add / Edit / Delete  
- Toggle “Show on Live Site”  

---

## **3.3 Tab: List of Sign‑Ups**
Features:
- List all sign‑ups  
- Filter by status  
- View act details  
- Check‑in  
- Notes  
- Approve / Reject  

---

# 🎬 **4. Run Session — Workflow**

## **4.1 Session Selection**
Choose a session → opens Session Workspace.

---

# 🖥️ **5. Session Workspace — Tabs**

## **5.1 Displays / Pages**
Displays:
- **MainBoard**  
- **Judge Page (mobile‑first)**  
- **Sign‑In Page**

Features:
- Assign displays  
- Generate judge codes  
- Preview displays  

---

## **5.2 Sign‑Ups**
Features:
- Filter  
- Pagination  
- View details  
- Check‑in  
- Approve / Reject  

---

## **5.3 Try‑Out Queue**

### **Statuses**
```
StandBy
OnDeck
Performing
Completed
Approved
Rejected
```

### **Status Rules**
- **Check‑In → StandBy**  
  - “Present” is implied  
  - `Status = StandBy`  
  - `CheckInTimestamp = now()`  

- **StandBy → OnDeck**  
  - One at a time  
  - Manual selection  

- **OnDeck → Performing**  
  - Director clicks “Start Performance”  

- **Performing → Completed**  
  - Director clicks “Mark Completed”  

- **Completed → Approved/Rejected**  
  - Can happen anytime  

### **Queue UI**
- Current Performer  
- On Deck  
- StandBy list (sortable)  
- Completed  
- Approved/Rejected (collapsed)  

---

# 📱 **6. Judge Page — Mobile‑First**

### **Layout**
- Single‑column  
- Large tap targets  
- Sticky tabs  
- Minimal scrolling  
- Resilient UI with retry  
- Cache only the current scoring form  

### **Tabs**
#### **Tab 1: Current Performer**
- Name  
- Act title  
- Music URL  
- Scoring fields  
- Comment box  
- **“I’m Done”** (per performer)

#### **Tab 2: On Deck**
- Name  
- Act title  

#### **Tab 3: Queue**
- Read‑only StandBy list  

#### **Tab 4: Past Performers**
- All Completed performers  
- Judges can finish or revise scoring  
- “I’m Done” toggle remains available  

### **Judge Completion**
- Per‑performer  
- Director sees completion status  
- Director is never blocked  
- Judges can score even after director moves on  

---

# 🎛️ **7. MainBoard Requirements**

Shows:
- **Current Performer**  
- **On Deck**  
- **Queue Preview** (3rd, 4th, 5th, etc.)

Updates instantly on any status change.

---

# 🎬 **8. Segment Stub Creation (Approval Flow)**

When an act is **Approved** in Try‑Outs:

- Move the act into **Rehearsals**  
- Create a **basic Show Segment stub** containing:
  ```
  SegmentId
  PerformerName(s)
  ActTitle
  Category
  MusicUrl
  TryOutNotes
  ScoringSummary
  ```
- **Do NOT** include:
  - Host text  
  - Teleprompter text  
  - Cues  
  - Timing  
  - Backstage instructions  

These are added during **Rehearsals**.

---

# 🖨️ **9. Sign‑Out Safety System (Two PDFs)**

Any time students stay after school (Try‑Outs, Rehearsals, etc.), they must be signed out by an authorized adult.

We generate **two PDFs**:

---

## **PDF #1 — Parent Sign‑Out Sheet (for parents)**  
Columns:
- Student Name  
- Pickup Adult (blank line — they write their name)  
- Signature  

**Purpose:**  
- Adult writes their name  
- Staff checks ID  
- Staff verifies authorization  
- Parents never see authorized names  

---

## **PDF #2 — Authorized Pickup List (staff‑only)**  
Columns:
- Student Name  
- Authorized Pickup Adult(s)  

**Purpose:**  
- Staff verifies ID  
- Never shown to parents  
- No signatures  

---

## **Which students appear?**
Only students currently in **StandBy** status.

Not included:
- Completed  
- Approved  
- Rejected  
- Not checked in  

---

# 🔄 **10. Display Sync Requirements**

Any status change updates:

- MainBoard  
- Judge Pages  
- Sign‑In Page  
- Director Queue View  

All displays must stay in sync in real time.

---

# 📝 **11. Data Model Updates**

### **Sign‑Up**
```
PickupAdult
MusicUrl
```

### **Try‑Out Session**
```
SessionId
Date
StartTime
EndTime
Location
PublicVisible
ParentSignOutRequired
```

### **Scoring**
```
Score {
  Category
  Value
  JudgeId
  PerformerId
}
```

### **Judge Completion**
```
JudgeCompletion {
  JudgeId
  PerformerId
  Completed (bool)
}
```

---

# 🎉 **12. Summary**

This file now includes:

- All clarifications  
- All safety rules  
- All judge workflow updates  
- All display logic  
- All PDF rules  
- All approval → rehearsal → segment stub logic  


