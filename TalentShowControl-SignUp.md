# 🎭 TalentShowControl‑SignUps.md  
*A helper‑focused workspace for configuring and managing public Talent Show sign‑ups.*

## 📌 Purpose  
The Sign‑Ups helper manages the **public intake form** for performers.  
It defines:

- What questions to ask  
- When sign‑ups are allowed  
- How submissions flow into Try‑Outs  
- How helpers review, approve, reject, or request resubmission  

Sign‑Ups **do not** have a scheduled event.  
They operate on a **date range** and a **review workflow**.

---

# 1. 🔧 Configure (Public Form Setup)

The Configure section defines the **structure and behavior** of the public sign‑up form.

## 1.1 Sign‑Up Window  
Defines when the form is open.

### Fields  
- Start date  
- End date  
- “Auto‑close at end date” toggle  
- “Show countdown to close” toggle  

### Data  
```
SignupStart  
SignupEnd  
AutoClose (bool)  
ShowCountdown (bool)
```

---

## 1.2 Form Questions  
Directors choose which questions appear on the public form.

### Default Questions  
- Performer name(s)  
- Grade  
- Teacher  
- Act title  
- Act description  
- Contact email  
- Contact phone  
- Special requirements  
- Media upload (optional)

### Optional Questions  
- Parent/guardian name  
- Number of performers  
- Equipment needs  
- Category (if categories exist)  
- “Anything else we should know?”

### Behavior  
- Add/remove questions  
- Mark required/optional  
- Reorder questions  

---

## 1.3 Try‑Out Integration  
All sign‑ups go to a review queue first.

### Case-by-Case Review Outcomes
1. **Approve to Rehearsals (No Try‑Out)**  
   - Sign‑Up → Review → ApprovedDirect  
   - Creates/updates Act + Segment directly

2. **Invite to Try‑Outs**  
   - Sign‑Up → Review → InvitedToTryOuts  
   - Creates a TryOutEntry (no segment yet)

3. **Needs More Info / Rejected**  
   - Stays in review workflow until resolved or closed

---

## 1.4 Confirmation & Messaging  
- Custom confirmation message  
- Optional email confirmation  
- Optional “What to expect next” message  
- Optional “If invited to Try‑Outs…” message  

---

## 1.5 Public Page Settings  
- Public URL  
- Title, subtitle, banner image  
- Publish/unpublish toggle  
- Preview mode  

---

# 2. 🏃 Review (Helper Review Workflow)

The Review section is where helpers **review proposals**, approve them, invite them to Try‑Outs, or reject them kindly.

## 2.1 Review Queue  
List of all submissions with filters:

- Pending review  
- Invited to Try‑Outs  
- Approved (no try‑out needed)  
- Rejected  
- Needs resubmission  

---

## 2.2 Approve / Invite / Reject  
Each sign‑up can be:

### ✔️ **Approved**  
- If Try‑Outs are required → “Invite to Try‑Outs” (no segment yet)  
- If no Try‑Outs → becomes an Act and is added directly to Segments  

### 🎟️ **Invited to Try‑Outs**  
- Helper selects a try‑out session  
- System assigns a slot  
- Email invitation sent (optional)

### ❌ **Rejected (with reason)**  
- Helper selects a reason:
  - Not enough detail  
  - Not appropriate for show  
  - Duplicate submission  
  - Missing required info  
  - Other (custom text)  
- Optional: “Allow resubmission” toggle  
- Sends a polite rejection message  

### 🔄 **Request Resubmission**  
- Helper asks for more info  
- Submission stays in “Needs More Info”  
- User can resubmit without starting over  

---

## 2.3 Export Tools  
- Export CSV  
- Export contact list  
- Export media list  
- Export special requirements  

---

# 3. 🔗 Integration With Try‑Outs

Sign‑Ups feed directly into Try‑Outs:

- Invited sign‑ups appear in Try‑Outs  
- “Invite to Try‑Outs” creates a Try‑Out entry  
- Try‑Out results create or confirm Act + Segment  
- Rejected sign‑ups stay out of Try‑Outs entirely  

This keeps the workflow clean and predictable.

---

## **3.1 Segment Creation (New)**  
When a sign‑up is approved with **no Try‑Out required**, the system automatically creates a **generic Segment** for the act.

### Behavior  
- A new `Segment` is created for each direct-approved sign‑up  
- “Invite to Try‑Outs” creates a `TryOutEntry` (no segment yet)  
- If selected in Try‑Outs, the act is added to Segments at that point  
- Segment is minimal at this stage  
- Segment will be expanded during Rehearsals  
- Walk‑ins at Try‑Outs also create a Segment  

### Initial Segment Fields  
```
SegmentType: Act  
ActId: <newly created Act>  
OrderIndex: null  
IsHidden: false  
Title: "<Act title>"  
DisplayInstructions: {
  HostTeleprompter: ""
  Backstage: ""
  MainBoard: ""
  TimerPresetSeconds: null
}
MediaFile: null  
CueList: []  
TechRequirements: initial draft from Sign‑Up  
```

### Purpose  
This ensures the Show timeline begins forming immediately and that Rehearsals can build out the segment fully.

---

# 4. 🧭 Navigation

From the Director Hub:

```
[ Sign-Ups ]
  Configure → /admin/talent-show/signups/configure/{eventId}
  Review    → /admin/talent-show/signups/review/{eventId}
  Preview   → /talent-show/signups/{eventId}
```
