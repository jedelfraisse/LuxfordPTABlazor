# 🎭 TalentShowControl‑TryOuts.md  
*A helper‑focused workspace for configuring and running Try‑Out Sessions for the Talent Show.*

## 📌 Purpose  
Try‑Outs allow the Talent Show team to **audition performers**, evaluate them, and select which acts will appear in the final show.

This workspace separates:

- **Configure** → Set up try‑out sessions, slot lengths, and helper assignments  
- **Sessions** → Conduct the actual audition day, mark attendance, score acts, and select performers  

Try‑Outs receive their performers from **Sign‑Ups**, not from the public.

---

# 1. 🔧 Configure (Setup for Try‑Out Helpers)

The Configure section prepares the structure of the try‑out process.

## 1.1 Try‑Out Sessions  
Try‑Outs are scheduled events with defined start/end times.

### Features  
- Add try‑out sessions  
- Edit session times  
- Remove sessions  
- Assign helpers to each session  
- Sync sessions to Event Schedule  
- Ownership rules:
  - If a helper creates a session, only that helper may edit/delete it  
  - Director can view all sessions  

### Data  
```
TryOutSessionId  
EventId  
StartTime  
EndTime  
AddedByUserId  
LockedToUserId (nullable)  
Notes (optional)
```

---

## 1.2 Slot Configuration  
Defines how performers are scheduled within each session.

### Fields  
- Slot length (minutes)  
- Break length (optional)  
- Auto‑schedule sign‑ups (optional)  
- Max performers per session  

### Behavior  
- Changing slot length recalculates available slots  
- Auto‑schedule assigns approved sign‑ups to earliest available slots  

### Data  
```
SlotLengthMinutes  
BreakMinutes  
AutoSchedule (bool)  
MaxPerformersPerSession  
```

---

## 1.3 Try‑Out Requirements  
Optional configuration for helpers.

### Options  
- Require media upload before try‑out  
- Require parent/guardian presence  
- Require equipment list  
- Require category selection  

### Data  
```
RequireMedia (bool)  
RequireGuardian (bool)  
RequireEquipmentList (bool)  
RequireCategory (bool)
```

---

## 1.4 Try‑Out Scoring Template  
Defines what helpers score during the audition.

### Default Fields  
- Stage presence  
- Preparedness  
- Creativity  
- Overall impression  

### Behavior  
- Add/remove scoring fields  
- Set scoring scale (1–5, 1–10, etc.)  
- Optional comment box  

### Data  
```
ScoringFields[] {
  FieldId  
  Label  
  MaxScore  
  OrderIndex  
}
```

---

# 2. 🏃 Sessions (Helper Audition Workflow)

The Sessions section is where helpers conduct the actual try‑out session.

## 2.1 Performer Queue  
Shows all performers assigned to the session.

### Columns  
- Performer name  
- Act title  
- Slot time  
- Status (Scheduled, Arrived, Performing, Completed)  
- Notes  

### Actions  
- Mark Arrived  
- Mark Performing  
- Mark Completed  
- Skip / No‑Show  
- Reassign slot  

---

## 2.2 Performer Details  
When a performer is selected, helpers see:

- Performer info  
- Act description  
- Media (if provided)  
- Special requirements  
- Category  
- Contact info  

---

## 2.3 Scoring & Notes  
Helpers score the performer using the scoring template.

### Fields  
- Scoring sliders or numeric inputs  
- Comment box  
- “Recommend for Show?” toggle  
- “Needs more info” toggle  

### Data  
```
Scores[] {
  FieldId  
  Score  
}

Comments  
RecommendForShow (bool)  
NeedsMoreInfo (bool)
```

---

## 2.4 Selection Workflow  
After the try‑out:

### Options  
- **Select for Show**  
  - Creates/updates a TalentShowAct  
  - Marks SelectedForShow = true  

- **Hold / Needs More Info**  
  - Returns to Sign‑Ups for clarification  

- **Not Selected**  
  - Sends polite rejection (optional)  

---

## 2.5 Session Summary  
At the end of the session:

- Total performers  
- No‑shows  
- Selected for show  
- Held for review  
- Rejected  
- Export results  

---

# 3. 🔗 Integration With Sign‑Ups & Acts

### From Sign‑Ups → Try‑Outs  
- Invited sign‑ups appear in the Try‑Out queue  
- “Invite to Try‑Outs” assigns a slot  
 
### From Try‑Outs → Acts  
- Selected performers create or confirm **TalentShowAct** entries and create/confirm an **Act Segment**  
- Acts feed into:
  - Rehearsals  
  - Show timeline  
  - Backstage displays  

---

# 4. 🧭 Navigation

From the Director Hub:

```
[ Try-Outs ]
  Configure → /admin/talent-show/tryouts/configure/{eventId}
  Sessions  → /admin/talent-show/tryouts/sessions/{eventId}
```

---

# 🎉 End of TalentShowControl‑TryOuts.md
