# **TalentShowControl‑Show‑Changes2.md**  
## **Refinements & Enhancements to the Show Module**

This document defines the **second‑pass refinements** to the Talent Show module.  
These changes improve usability, clarity, and workflow efficiency without altering the core architecture defined in `TalentShowControl‑Show‑Changes.md`.

---

# **1. Config Workspace — Refinements**

## **1.1 Show Settings — Field Order & Visibility**

Reorder fields as follows:

1. **Host Names**  
   - Repeatable list  
   - Required  

2. **Voting Options**  
   - `[ ] Judges Voting`  
   - `[ ] Audience Voting`  

3. **Scoring Scale**  
   - Visible when **either** Judges Voting **or** Audience Voting is enabled  

4. **Scoring Categories**  
   - Visible when **either** Judges Voting **or** Audience Voting is enabled  
   - If empty, auto‑populate with:  
     - Stage Presence  
     - Creativity  
     - Technical Skill  
     - Entertainment Value  
     - Overall Impression  

---

## **1.2 Show Start Time Override**

Add:

### **Show Start Time Override**
- Optional datetime  
- Overrides the Event Schedule’s official start time  
- Used when the show is running early/late  
- Affects countdowns and “Show begins at…” messaging  
- If not set → fall back to Event Schedule start time  

---

## **1.3 Background Image Overrides**

### **Global Setup**
Add:

**Global Display Background Image**  
- Optional image upload  
- Default background for all modules and all display roles  

### **Show Settings**
Add:

**Show Background Override**  
- Optional image upload  
- Overrides the global background for all Show display roles  
- If not set → fall back to global background  

---

## **1.4 Additional Optional Settings**

Add:

- **Show Theme** (text or dropdown; used for display styling)  
- **Default Segment Duration** (`mm:ss`)  
- **Enable Sponsor Rotation** (checkbox)

---

# **2. Acts Tab — Refinements**

## **2.1 Additional Act Metadata**

Add the following fields:

- Music URL  
- Music Start Offset  
- Act Duration (`mm:ss`)  
- Performer Notes  
- Stage Notes  
- Lighting Notes  
- Sound Notes  
- Props Required  
- **Host Intro** (optional — used to auto‑fill Act segment Intro Text)

---

## **2.2 Auto‑Fill Behavior**

When adding an Act segment in Script/Timeline:

- Segment Name auto‑fills from Act Name  
- Intro Text auto‑fills from Act’s Host Intro  
- Duration auto‑fills from Act Duration  
- Performers appear in the segment list  

---

# **3. Script / Timeline — Refinements**

## **3.1 Duration Format**

All durations must use:

**`mm:ss` format**  
(Internally seconds are fine.)

---

## **3.2 Adding an Act Segment — Show Only Unused Acts**

When adding an Act segment:

- Segment Name becomes a **dropdown of Acts not yet used**  
- Acts already placed in the timeline are excluded  
- If all Acts are used → disable Add Act or show a message  

---

## **3.3 Segment Editing**

Editing a segment must support:

- Editing all fields  
- Duration in `mm:ss`  
- Act segments show Act dropdown  
- Non‑Act segments show free‑text name  
- Changing segment type clears irrelevant fields  
- Updates apply immediately  

---

## **3.4 Segment List Improvements**

Each segment in the timeline list must show:

- Segment type icon  
- Segment name  
- **Performers (for Acts)**  
- Duration  
- Drag handle  
- Edit button  
- Delete button (if allowed)

---

## **3.5 Host Name Inserter (Literal Text Only)**

When editing Host Text or Intro Text:

- Provide UI to insert host names as **literal text**, e.g.:  
  `Alice:`  
  `Ben:`  
- Cursor placed after colon+space  
- No tokens, no placeholders, no templating  

---

## **3.6 Performer Name Inserter (Literal Text Only)**

For Act segments:

- Provide UI to insert performer names as **literal text**, e.g.:  
  `Sarah`  
  `The Dancing Dragons`  

No `[name]`, `{performer}`, or dynamic replacement.

---

## **3.7 Natural Script Writing**

The Director writes the script exactly as spoken:

```
Alice: The next performer has a knack for the g-tar.
Ben: Welcome Sarah to the stage.
```

- No placeholders  
- No auto‑rewriting if host names change later  
- Teleprompter displays exactly what is typed  

---

## **3.8 Segment Editing Allowed in Setup**

Even though Rehearsals is the preferred place to finalize scripts:

- The Script/Timeline editor must be fully editable during Setup  
- Supports events with no rehearsal time  
- Allows Directors to prepare everything early  

---

# **4. Displays Tab — Refinements**

## **4.1 Display Role Descriptions**

At the top of the tab, list each role with a description:

- MainDisplay  
- Backstage Prompt  
- Frontstage Prompt  
- Host Prompt  
- Judges  
- Voters  

---

## **4.2 Display Assignment List**

List all connected displays with:

- Editable display name  
- Role assignment dropdown  
- Connection status  
- Preview button  

---

## **4.3 Display Previews — Full Example Layouts**

During Setup Mode, previews must show **full example displays**, not placeholder text.

### **MainDisplay**
- “Welcome to the Talent Show!”  
- Sponsor rotation placeholder  
- Background theme  
- Example lower‑third  
- Example act card  

### **Backstage Prompt**
- Example cues  
- Example On Deck performer  
- Timing placeholder  

### **Frontstage Prompt**
- Example segment info  
- Timing placeholder  

### **Host Prompt**
- Teleprompter scroll test  
- Example host script  
- Font size controls  

### **Judges**
- Example scoring UI (disabled)  

### **Voters**
- “Voting will open soon”  
- Example voting card (disabled)

---

# **End of Change2**

This completes the second‑pass refinement layer for the Talent Show module.
