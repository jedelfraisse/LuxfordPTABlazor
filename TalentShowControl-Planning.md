# 🎭 TalentShowControl‑Planning.md  
*A helper‑focused workspace for configuring and running Planning Meetings in the Talent Show system.*

## 📌 Purpose  
Planning Meetings define the foundational decisions for the Talent Show.  
This workspace separates:

- **Meeting Setup** → Meeting dates, schedule sync, ownership, and helper assignments  
- **Meetings** → Conduct sessions and capture decisions  

---

# 1. 🔧 Meeting Setup (Planning Meetings Setup)

The Configure section prepares the environment so helpers know **when** planning meetings occur and what they are responsible for.

## 1.1 Planning Meeting Dates  
These dates sync directly with the Event Schedule.

### Features  
- Add planning meeting dates  
- Edit planning meeting dates  
- Remove planning meeting dates  
- Automatically sync to Event Schedule  
- Ownership rules:
  - If a helper adds a date, only that helper may edit or delete it  
  - Director can view all dates but respects ownership locks  

### Data Stored  
```
MeetingDateId  
EventId  
Date  
AddedByUserId  
LockedToUserId (nullable)  
Notes (optional)  
```

---

## 1.2 Optional: Agenda Template  
Directors may define a template for helpers to follow.

### Examples  
- Review max acts  
- Confirm number of judges  
- Discuss categories  
- Review rules & guidelines  
- Confirm sign‑up window  

### Behavior  
- Editable by Director  
- Visible to helpers during Run mode  

---

## 1.3 Assign Helpers  
Directors may assign helpers to specific planning meetings.

### Features  
- Assign one or more helpers  
- Helpers see only meetings they are assigned to  
- Ownership rules apply to assigned helpers  

---

# 2. 🏃 Meetings (Helper Workflow)

The Meetings section is where the helper actually conducts the planning meeting and records the required information for the Talent Show.

## 2.1 Max Acts  
- Numeric field  
- “Flexible?” toggle  
- If flexible: allow a range (e.g., 12–15)

### Data  
```
MaxActs  
MaxActsFlexible (bool)  
MaxActsMin (nullable)  
MaxActsMax (nullable)  
```

---

## 2.2 Number of Judges  
- Numeric field  
- Optional: list judge names  

### Data  
```
JudgeCount  
JudgeNames[]  
```

---

## 2.3 Sign‑Up Window  
Defines when performers can register.

### Fields  
- Start date  
- End date  
- Auto‑close toggle  
- External sign‑up link (if not using built‑in form)

### Data  
```
SignupStart  
SignupEnd  
AutoClose (bool)  
ExternalSignupUrl (nullable)  
```

---

## 2.4 Categories  
- Add category  
- Remove category  
- Reorder categories  
- Optional: category descriptions  

### Data  
```
Categories[] {
  CategoryId  
  Name  
  Description  
  OrderIndex  
}
```

---

## 2.5 Rules & Guidelines  
- Rich text editor  
- Optional PDF upload  
- “Publish to event page” toggle  

### Data  
```
RulesMarkdown  
RulesPdfPath (nullable)  
PublishRules (bool)  
```

---

## 2.6 Notes & Decisions  
A living record of the planning meeting.

### Features  
- Freeform notes  
- “Unanswered questions” checklist  
- “Resolved” toggle per item  
- Timestamped entries  

### Data  
```
Notes[] {
  NoteId  
  Text  
  CreatedAt  
  CreatedBy  
}

Questions[] {
  QuestionId  
  Text  
  Resolved (bool)  
  ResolvedAt (nullable)  
}
```

---

# 3. 📤 Outputs of Planning Meetings

The results of Planning Meetings feed into:

- Try‑Outs configuration  
- Rehearsal configuration  
- Show timeline defaults  
- Public event page  
- Sign‑Up Page configuration  
- Director Hub summaries  

---

# 4. 🧭 Navigation

From the Director Hub:

```
[ Planning Meetings ]
  Meetings  → /admin/talent-show/planning/{eventId}
```
