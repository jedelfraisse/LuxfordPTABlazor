# **TalentShowControl‑Show‑Displays.md**  
## **Unified Display Framework for Try‑Outs, Rehearsals, and Live Show**

This document defines the **global display architecture** for the Talent Show system.  
It standardizes:

- Act fields  
- Segment fields  
- Display roles  
- Display templates  
- Status‑based rendering  
- Placeholder system  
- Device‑specific overrides  
- Director walk‑around tuning workflow  

All displays load **`/display`**, and the Director assigns each device a **Display Role**.

---

# **1. Global Act Model (Used in ALL Modules)**

Acts use a single, unified model across:

- Try‑Outs  
- Rehearsals  
- Live Show  
- Backstage  
- Judges/Voters  
- Host Prompt  
- All displays  

### **Act Model**
```
ActId              Guid
PerformerNames     string
ActTitle           string
Category           string
Duration           string      // mm:ss
MediaFile          string
PerformerNotes     string
```

---

# **2. Global Segment Model (Show Module Only)**

Segments define the **Show timeline** and reference Acts when needed.

### **Segment Model**
```
SegmentId          Guid
SegmentType        enum        // HostTalk, Act, Intermission, SponsorAd, Awards
SegmentTitle       string
Duration           string      // mm:ss (optional)

ActId              Guid?       // Only for Act segments
IntroText          string      // Only for Act segments

HostText           string      // Only for non-Act segments

SponsorId          Guid?       // SponsorAd segments
AwardId            Guid?       // Awards segments
```

### **Visibility Rules**
- If **SegmentType = Act** → show **IntroText**, hide HostText  
- If **SegmentType ≠ Act** → show **HostText**, hide IntroText  

---

# **3. Display Roles**

Each `/display` client is assigned one of the following roles:

- **MainDisplay**  
- **BackstagePrompt**  
- **FrontstagePrompt**  
- **HostPrompt**  
- **Judges**  
- **Voters**  

Each role renders different content.

---

# **4. Display Payload Structure (Unified Across All Modules)**

All displays receive the same payload structure:

```
{
  "role": "MainDisplay",
  "module": "Show",            // TryOuts, Rehearsals, Show
  "showStatus": "Live",        // Setup, Ready, PreShow, StandBy, Live, WrapUp, Done

  "segment": {
    "type": "Act",
    "title": "Guitar Solo",
    "duration": "02:30",
    "hostText": null,
    "introText": "Alice: Please welcome Sarah to the stage.",
    "act": {
      "performerNames": "Sarah",
      "actTitle": "Guitar Solo",
      "category": "Music",
      "duration": "02:30",
      "mediaFile": "/media/sarah.mp3",
      "performerNotes": "Spotlight center stage"
    }
  },

  "queue": [],                 // Try-Outs only
  "onDeck": null,              // Try-Outs only

  "templateHtml": "<h1>Welcome</h1>",
  "templateType": "PreShow",

  "backgroundImage": "url",
  "theme": "string",
  "countdown": null
}
```

---

# **5. Display Rules by Module**

## **5.1 Try‑Outs**

### **MainDisplay**
Shows:
- Current performer  
- On deck  
- Next 5 acts  

### **BackstagePrompt**
Shows:
- Current performer  
- On deck  
- Timing  

### **FrontstagePrompt**
Shows:
- Current performer  
- Timing  

### **HostPrompt / Judges / Voters**
Unused.

---

## **5.2 Rehearsals**

### **MainDisplay**
Shows:
- Act card  
- Media testing  
- Timing  

### **BackstagePrompt**
Shows:
- PerformerNames  
- Cues  

### **FrontstagePrompt**
Shows:
- PerformerNames  
- Timing  

### **HostPrompt**
Shows:
- IntroText (Act)  
- HostText (non‑Act)  

### **Judges / Voters**
Idle.

---

## **5.3 Live Show**

### **Host Talk**
- MainDisplay: HostTalk template  
- HostPrompt: HostText  
- Backstage/Frontstage: SegmentTitle + timing  

### **Act**
- MainDisplay: Act card or Act template  
- HostPrompt: IntroText  
- BackstagePrompt: PerformerNames + cues  
- FrontstagePrompt: PerformerNames + timing  
- Judges/Voters: scoring/voting  

### **Intermission**
- MainDisplay: Intermission template  
- HostPrompt: HostText  

### **Sponsor Ad**
- MainDisplay: Sponsor media  
- HostPrompt: HostText  

### **Awards**
- MainDisplay: Award template  
- HostPrompt: HostText  

---

# **6. Show Status Templates (MainDisplay)**

MainDisplay uses **templates** instead of segment rendering when the show is not in Live mode.

Each template is stored as **Markdown** and converted to HTML.

### Templates:
- **SetupTemplateMarkdown**  
- **ReadyTemplateMarkdown**  
- **PreShowTemplateMarkdown**  
- **StandByTemplateMarkdown**  
- **LiveStartTemplateMarkdown**  
- **WrapUpTemplateMarkdown**  
- **DoneTemplateMarkdown**  

---

# **7. Markdown Support**

Templates support:

- Headings (`#`, `##`, `###`)  
- Bold / italic  
- Line breaks  
- Lists  
- Sponsor carousel placeholder  
- Background override placeholder  

---

# **8. Template Placeholders**

## **8.1 Global Placeholders (Always Available)**
```
{{Title}}
{{Subtitle}}
{{HostedBy}}
{{SchoolName}}
{{EventDate}}
{{EventTime}}
{{SponsorCarousel}}
{{Background}}
```

## **8.2 Act Placeholders (Live Mode, Act Segment Only)**
```
{{ActTitle}}
{{PerformerNames}}
{{Category}}
{{Duration}}
{{MediaFile}}
```

## **8.3 System Placeholders**
```
{{Countdown}}
{{Theme}}
{{Now}}
```

---

# **9. Device Details Panel (Workspace‑Side Only)**

Each connected display has a **Details** button in the Director workspace.

### **9.1 Device-Specific Font Controls**
- Font scale (0.5× → 2.0×)  
- Line height  
- Padding  
- Reset to default  

Stored per device:
```
Device.FontScale
Device.LineHeight
Device.Padding
```

### **9.2 Device Metadata**
- Device name  
- Device type  
- Screen resolution  
- Assigned role  
- Last seen  
- Connection status  

### **9.3 No Preview**
The Director adjusts settings while physically viewing the display.

---

# **10. Template Verification Workflow (Director Walk‑Around Mode)**

Directors must verify that MainDisplay templates render correctly on each physical display.

### Workflow:
1. Open the Displays tab on a mobile device  
2. Select a display → Details  
3. Adjust font scale  
4. Change show status to cycle through templates  
5. Verify each template on the physical display  
6. Save device‑specific scaling  
7. Repeat for each display  

---

# **11. Display Update Rules**

Displays update when:

- Show status changes  
- Segment changes  
- Device role changes  
- Template changes  
- Font scaling changes  
- Theme/background changes  

---

# **12. Session Expiration Rules**

When the show enters **Done**:

- All display sessions expire  
- All devices return to Unassigned  
- `/display` shows “Show has ended”  

---

# **End of TalentShowControl‑Show‑Displays.md**