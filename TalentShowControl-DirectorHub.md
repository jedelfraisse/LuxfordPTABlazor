# 🎛️ TalentShowControl‑DirectorHub.md  
*A centralized dashboard providing access to all Talent Show administration modules.*

## 📌 Purpose  
The Director Hub is the **entry point** for managing the Talent Show.  
It presents five modules, each responsible for a distinct phase of the event lifecycle:

1. **Global Setup**  
2. **Sign‑Ups**  
3. **Try‑Outs**  
4. **Rehearsals**  
5. **The Show**

Each module links to its own workflows and maintains strict ownership of its configuration and data.

---

# 1. 🧭 Dashboard Layout

The Director Hub displays five navigation boxes:

```
[ Global Setup ]
[ Sign‑Ups ]
[ Try‑Outs ]
[ Rehearsals ]
[ The Show ]
```

Each box leads to the corresponding helper module.

---

# 2. 📦 Module Overview

## 2.1 Global Setup  
**Purpose:** Configure event‑wide defaults and global rules.

### Responsibilities  
- Event title, theme, and branding  
- Default display settings  
- Default media rules  
- Default cue rules  
- Default segment behavior  
- Voting defaults  
- Global toggles  
- Any configuration shared across helpers  

### Navigation  
```
Configure → /admin/talent-show/global/{eventId}
```

---

## 2.2 Sign‑Ups  
**Purpose:** Manage the public performer intake form.

### Editable (owned by Sign‑Ups)  
- SignupStart  
- SignupEnd  
- AutoClose  
- ShowCountdown  
- TryOutMode  
- Form questions  
- Public page settings  
- Confirmation messages  

### Read‑Only (owned by Global Setup)  
- Event theme  
- Branding  
- Global media rules  
- Global cue rules  

### Navigation  
```
Configure → /admin/talent-show/signups/configure/{eventId}
Review    → /admin/talent-show/signups/review/{eventId}
Preview   → /talent-show/signups/{eventId}
```

---

## 2.3 Try‑Outs  
**Purpose:** Schedule and conduct try‑out sessions.

### Editable (owned by Try‑Outs)  
- Try‑Out session dates  
- Slot times  
- Helper assignments  
- Selection decisions  
- Walk‑in creation (Act + Segment)

### Read‑Only (owned by Global Setup)  
- Event theme  
- Branding  
- Global media rules  

### Navigation  
```
Configure → /admin/talent-show/tryouts/configure/{eventId}
Sessions  → /admin/talent-show/tryouts/sessions/{eventId}
```

---

## 2.4 Rehearsals  
**Purpose:** Build and refine the final Show Timeline using segments.

### Responsibilities  
- Segment editing  
- Segment ordering  
- Segment media  
- Segment cues  
- Segment display instructions  
- Segment timing  
- Adding new segments  
- Finalizing tech sheets  

### Navigation  
```
Configure → /admin/talent-show/rehearsals/configure/{eventId}
Sessions  → /admin/talent-show/rehearsals/sessions/{eventId}
```

---

## 2.5 The Show  
**Purpose:** Run the live Talent Show using the finalized Segment Timeline.

### Responsibilities  
- Manual segment advancement  
- Skip/reorder/insert  
- Display instructions  
- Media playback  
- Cue triggering  
- Timer control  
- Judges voting  
- Audience voting  
- Backstage prompts  
- Host teleprompter  

### Navigation  
```
Configure → /admin/talent-show/show/configure/{eventId}
Live      → /admin/talent-show/show/live/{eventId}
Public    → /talent-show/{eventId}
```

---

# 3. 🔗 Workflow Summary

The Director Hub reflects the full Talent Show lifecycle:

```
Global Setup → Sign‑Ups → Try‑Outs → Rehearsals → The Show
```

Each module owns its own configuration and data.  
Global Setup provides defaults.  
Downstream helpers execute their specific responsibilities.
