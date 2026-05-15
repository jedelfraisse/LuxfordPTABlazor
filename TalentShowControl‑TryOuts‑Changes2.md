# 📘 **TalentShowControl‑TryOuts‑Changes2.md**  
*Additional Corrections & Enhancements to the Try‑Outs Module*

---

# 🎯 **1. Config Page Corrections**

## **1.1 Remove Check‑In from Config → List of Sign‑Ups**
The List of Sign‑Ups tab currently includes:

- Approve  
- Reject  
- Check‑In ❌ (incorrect)

### **Change**
- **Remove Check‑In** from the Config page entirely.  
- Check‑In is a **session‑based action** and must occur only inside **Run Session**.

---

# 🎛️ **2. Run Session — Session List & Active Session Behavior**

## **2.1 When No Session Is Active**
The Run Session page displays a **read‑only list** of all Try‑Out sessions.

Each row includes:

- Date  
- Start Time  
- End Time  
- Location  
- **Activate** button  

No editing is allowed here.  
Editing sessions is only done in **Configure → Sessions**.

---

## **2.2 When a Session *Is* Active**
Once a session is activated, the Run Session page becomes the **Active Session Workspace**.

### **2.2.1 Session Summary Header**
Displayed at the top:

```
Date
Start Time
End Time
Location
```

To the right:  
**Change Session** button  
(returns to the session list so a different session can be activated)

### **2.2.2 Session Stats**
Three stat boxes:

- **Total Sign‑Ups**  
- **# Checked In**  
- **# Completed**  

### **2.2.3 Current Performance Block**
Shows:

- Performer name  
- Act title  
- Music URL  
- Judge completion statuses  
- Display statuses  

Controls:

- **Play Song**  
- **Stop Song**  
- **Complete Act**  
- **Prompt Next Act**

### **2.2.4 Queue Tabs**

#### **Tab: StandBy**
- Ordered list  
- Actions:
  - Put On Deck  
  - Check‑Out (left early or checked in by mistake)

#### **Tab: Completed**
- Completed performers  
- Approve / Reject  
- Judge completion visibility  

#### **Tab: Not Checked In**
- Late arrivals  
- Action: **Check‑In**

---

# 🖥️ **3. Universal Display Provisioning (Try‑Outs Only)**

## **3.1 One Universal URL**
All display devices begin setup by visiting:

```
/display
```

This page is a **gateway** for device registration.  
Devices do **not** choose their own role.

---

## **3.2 Display Registration + Local Storage**
When a device loads `/display`, it:

1. Connects to the server  
2. Generates a **Display Code** (e.g., `AB4F2`)  
3. Stores the Display Code in **localStorage**  
4. Checks localStorage for a previously assigned role  
5. Shows:

```
Display Code: AB4F2
Status: Waiting for assignment...
Tap to Enable Audio/Video (if required)
```

### **Local Storage Fields**
```
displayCode
assignedRole
```

### **Purpose**
- Survive refresh  
- Survive accidental reload  
- Survive iPad sleep/wake  
- Maintain role **during the current Try‑Outs session only**

---

## **3.3 Director Assigns Display Roles**
In **Configure → Displays**, the Director sees:

- All connected displays  
- Their Display Codes  
- Their connection status  
- A dropdown to assign each display a Try‑Outs role:

```
[AB4F2] → TryOuts.MainBoard
[QX9L1] → TryOuts.ActDisplay
[ZP7D8] → TryOuts.JudgePage
```

Once assigned:

- The device stores the role in localStorage  
- The device automatically loads the correct Try‑Outs display page  

---

## **3.4 Audio/Video Unlock (iPad & iOS Browsers)**
Because iOS requires a user gesture:

- `/display` shows **Tap to Enable Audio/Video**  
- Once tapped, the device is fully controllable  
- Required once per device per load  

After unlocking:

- Audio playback works  
- Video playback works  
- Remote commands work  
- Timers work  

---

## **3.5 Display Expiration (Try‑Outs Only)**

A display’s role assignment expires automatically based on:

### **A. Scheduled End Time + 1 Hour**
If the session ends at 5:00 PM → displays expire at **6:00 PM**.

### **B. One Hour After Last Update**
If the session runs long, expiration occurs **1 hour after the last received update**.

### **Expiration Behavior**
When expired:

- The assigned role is removed from localStorage  
- The device returns to the `/display` waiting screen  
- The Display Code is shown again  

This ensures clean setup for the next Try‑Outs session.

---

# 🎬 **4. Act Display Role (Try‑Outs)**

## **4.1 Act Display Shows**
- Performer name  
- Act title  
- Soft timer  
- Progress bar  
- Act video (if provided)  
- Music playback (if provided)  
- “On Deck” or “Performing” status  

---

## **4.2 Remote Control**
Director Dashboard can send:

- Start Timer  
- Reset Timer  
- Play Song  
- Stop Song  
- Play Video  
- Stop Video  
- Set Volume (browser volume only)

---

## **4.3 Audio Output Options**
For any display role that supports audio:

### **Option A — Audio from the Display Device**
Ideal for:

- Act Display on stage  
- Rehearsals  
- Rooms with local speakers  

### **Option B — Audio from the Director Computer**
Ideal for:

- Centralized sound system  
- Live show audio control  

Director chooses:

```
Audio Output:
( ) Director Computer
( ) Display Device
```

---

# 🔄 **5. Try‑Outs Only (Future Phases Later)**

This Changes2 document applies **only to Try‑Outs**.

Rehearsals and Live Show will introduce:

- Different display roles  
- Different layouts  
- Different behaviors  
- Different expiration rules  

The Try‑Outs display system is designed to be extended later.

---

# 🎉 **6. Summary of Changes2**
- Removed Check‑In from Config  
- Rewrote Active Session behavior  
- Added Change Session  
- Added Current Performance block  
- Added queue tabs  
- Added universal `/display` provisioning  
- Added Display Codes  
- Added Director‑assigned roles  
- Added localStorage persistence  
- Added audio/video unlock  
- Added Act Display role  
- Added Director audio routing  
- Added automatic display expiration  
- Try‑Outs only; future phases will extend this system  

