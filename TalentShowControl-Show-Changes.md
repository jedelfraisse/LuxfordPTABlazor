# TalentShowControl‑Show‑Changes.md  
## Live Show Subsystem — Required Changes

**Important:**  
The **entire Show workspace is being rebuilt from scratch.**  
Do **not** reuse or extend the existing Show UI, components, or logic.  
Replace the Show workspace with the structure defined in this document.

The Show subsystem is separate from Try‑Outs and must not reuse Try‑Outs UI or logic except where explicitly stated.

---

# 1. Show Workspaces

The Show has two workspaces:

- **Config** — configure and test the show (before and during Live)  
- **Live** — run the show in real time

These workspaces have **separate status systems**.

---

# 2. Config Workspace

The **Config** workspace is where the Director configures and tests the show.

Config has its own **editable** status:

- **Setup** — show is not ready  
- **Ready** — show is fully configured and Live workspace is unlocked

Config also displays the **read‑only Live Status** for awareness.

Config contains these tabs:

- **Status**  
- **Show Settings**  
- **Acts**  
- **Script / Timeline**  
- **Displays**

---

## 2.1 Config Status (Editable)

**Config.Status** can be changed manually by the Director:

- **Setup**  
- **Ready**

The system may *suggest* readiness based on validation, but the Director has final control.

### Required items for Ready (system validation):

- At least one **Host**  
- At least one **Act**  
- Script / Timeline has at least one segment  
- Displays assigned (MainDisplay + Host Prompt minimum)  
- If Judges Voting enabled → categories + scale valid  
- If Audience Voting enabled → voting config valid  

When Config.Status is set to **Ready**:

- Displays switch to **Ready Mode**  
- Live workspace becomes available  

---

## 2.2 Status Tab (Config)

Shows:

### Editable:
- **Config Status:** Setup / Ready

### Read‑only:
- **Live Status:** Pre‑Show / Stand‑By / Live / Wrap‑Up / Done

Also shows summary of:

- Judges Voting  
- Audience Voting  
- Host Names  
- Scoring Categories  
- Scoring Scale  

---

## 2.3 Show Settings Tab

Fields:

- Judges Voting  
- Audience Voting  
- Host Name(s)  
- Scoring Categories  
- Scoring Scale  

Used by:

- Teleprompter  
- Judges display  
- Voter page  
- Live runtime logic  

---

## 2.4 Acts Tab

Final show list.

Features:

- Add/edit acts  
- Assign performers  
- Assign media  
- Assign staging notes  

Acts are referenced by Act segments.

---

## 2.5 Script / Timeline Tab

The timeline is composed of **segments**.

### Segment Types

- **HostTalk**  
- **Act** (contains its own Intro)  
- **Intermission**  
- **Awards**  
- **Custom**

#### HostTalk Segment

Used for:

- Opening  
- Transitions  
- Banter  
- Announcements  

Fields:

- Segment Name  
- Host Text  
- Notes  
- Cues  
- Media triggers  

#### Act Segment (with Intro)

Fields:

- Act Name  
- Performer(s)  
- **Intro Text**  
- Act Notes  
- Cues  
- Media triggers  
- Voting settings  
- Attached Act ID  

Behavior:

- Intro moves with the Act  
- Teleprompter shows Intro when segment starts  
- Judges/Voters activate automatically  

#### Intermission Segment

Fields:

- Segment Name  
- Optional Host Text  
- Notes  
- Cues  
- Media triggers  

#### Awards Segment

Fields:

- Segment Name  
- Host Text  
- Award categories  
- Winner announcements  
- Media triggers  
- Notes  

#### Custom Segment

Wildcard segment.

Fields:

- Segment Name  
- Optional Host Text  
- Notes  
- Cues  
- Media triggers  

---

## 2.6 Displays Tab (Config)

Assigns physical displays to logical roles:

- MainDisplay  
- Backstage Prompt  
- Frontstage Prompt  
- Host Prompt  
- Judges  
- Voters (/vote)

---

## 2.7 Display Behavior in **Setup** (Config.Status = Setup)

Displays must be fully testable.

### MainDisplay
- “Display Connected — Setup Mode”  
- Test pattern  
- **Font size test text**  
- Optional audio test  
- Optional theme preview  

### Backstage Prompt
- “Backstage Display Connected — Setup Mode”  
- Test cues/timing placeholders  
- **Font size test text**

### Frontstage Prompt
- “Frontstage Display Connected — Setup Mode”  
- **Font size test text**

### Host Prompt
- “Teleprompter Connected — Setup Mode”  
- Scroll test  
- **Font size test text**  
- Optional font size controls  

### Judges
- “Judging System Connected — Setup Mode”  
- “Prepare for show”  
- **Font size test text**

### Voters (/vote)
- “Welcome to the Talent Show!”  
- “Voting will open soon”  
- If scheduled: “Voting opens at [time]”  
- Optional font size test text  

---

## 2.8 Display Behavior in **Ready** (Config.Status = Ready)

Once configuration is complete, displays switch to **audience‑ready mode**.

### MainDisplay
- “Welcome to the Talent Show!”  
- Sponsor rotation (if enabled)  
- Optional scheduled start time  
- Optional countdown to show start  
- Background theme  

### Backstage Prompt
- “Ready Mode — Please prepare”  
- First segment  
- On Deck performer  

### Frontstage Prompt
- “Ready Mode — Please prepare”  
- First segment  
- Timing  

### Host Prompt
- Opening HostTalk script  
- Host names  
- Notes  

### Judges
- “Prepare for the show”  

### Voters (/vote)
- “Welcome to the Talent Show!”  
- “Voting will open soon”  
- Optional scheduled start time  

---

# 3. Live Workspace

The **Live** workspace is the runtime control panel.  
It is only available when **Config.Status = Ready**.

Live workspace controls:

- **Live Status**  
- Segment flow  
- Performer flow  
- Countdown timers  
- Live display updates  

---

## 3.1 Live Status (Read‑Only in Config, Editable in Live)

Live.Status has five states:

- **Pre‑Show**  
- **Stand‑By**  
- **Live**  
- **Wrap‑Up**  
- **Done**

Only the **Live workspace** can change Live.Status.

---

## 3.2 Transitions and Countdowns

### 3.2.1 Entering Live.Pre‑Show

Triggered manually by Director.

Displays show:

- MainDisplay: “Welcome to the Talent Show!”  
- Sponsors (if enabled)  
- Audience entering  
- Hosts preparing  
- Crew preparing  

### 3.2.2 Pre‑Show → Stand‑By

Director triggers this.

Displays show:

- “Show starting soon”  
- First segment  
- On Deck performer  

### 3.2.3 Stand‑By → Live (Countdown)

Director triggers countdown (`[var]` seconds).

Displays show:

- “Going Live in X seconds”  
- Host Prompt shows countdown  
- Backstage/Frontstage show countdown  
- Judges/Voters inactive until Live  

At zero:

- Live.Status = **Live**  
- First segment becomes active  

---

## 3.3 Segment Controls (Live)

Director can:

- Start segment  
- End segment  
- Next segment  
- Skip segment  

---

## 3.4 Performer Flow (Acts)

Flow:

- StandBy  
- OnDeck  
- Performing  
- Completed  

Rules:

- Auto‑OnDeck allowed  
- Auto‑Performing **not** allowed  

---

# 4. Display Roles and Behavior

## 4.1 Display Roles

- MainDisplay  
- Backstage Prompt  
- Frontstage Prompt  
- Host Prompt  
- Judges  
- Voters (/vote)  

---

## 4.2 Display Behavior by Live Status

### Pre‑Show
As defined in **2.8 Ready Mode**.

### Stand‑By
- MainDisplay: “Show starting soon”  
- Backstage/Frontstage: first segment + On Deck  
- Host Prompt: opening script  
- Judges/Voters: inactive  

### Live
Displays update based on segment type:

| Segment Type | Host Prompt | MainDisplay | Backstage | Frontstage | Judges | Voters |
|--------------|-------------|-------------|-----------|------------|--------|--------|
| HostTalk | Host text | Segment title | Segment info | Segment info | Off | Off |
| Act | Intro text | Act title + performer | Performer + On Deck + cues | Performer + timing | On | On (if enabled) |
| Intermission | Optional text | Intermission screen | Timing | Timing | Off | Off |
| Awards | Award script | Award graphics | Award flow | Award flow | Off | Off |
| Custom | Host text | Segment title | Segment info | Segment info | Off | Off |

### Wrap‑Up
- MainDisplay: wrap‑up screen  
- Host Prompt: closing script  
- Judges/Voters: closed  

### Done
- Displays expire  
- Show archived  
