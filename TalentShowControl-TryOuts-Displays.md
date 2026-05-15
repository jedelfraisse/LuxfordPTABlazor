# 📘 **TalentShowControl‑TryOuts‑Displays.md**  
*Display Roles, Behaviors, Layouts, Scaling, and Real‑Time Requirements for Try‑Outs*

---

# 🎯 **1. Overview**

This document defines all **display surfaces** used during **Try‑Outs**, including:

- Device registration  
- Role assignment  
- Visual layout  
- Branding  
- Font scaling  
- Real‑time updates  
- Timer and media behavior  
- Expiration rules  

This specification applies **only to Try‑Outs**.  
Rehearsals and Live Show displays will be defined separately.

---

# 🖥️ **2. Display Provisioning Architecture**

## **2.1 Universal Entry Point**
All display devices begin setup by visiting:

```
/display
```

This page is a **gateway** for device registration and role assignment.

Devices do **not** choose their own role.

---

## **2.2 Display Registration**
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
fontSizePercent
```

### **Purpose**
- Survive refresh  
- Survive accidental reload  
- Survive iPad sleep/wake  
- Maintain role **during the current Try‑Outs session only**  
- Maintain font scaling  

---

## **2.3 Director Role Assignment**
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

## **2.4 Audio/Video Unlock (iPad & iOS Browsers)**
Because iOS requires a user gesture:

- `/display` shows **Tap to Enable Audio/Video**  
- Once tapped, the device is fully controllable  
- Required once per device per load  

---

## **2.5 Display Expiration (Try‑Outs Only)**

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

---

# 🎬 **3. Try‑Outs Display Roles**

Try‑Outs uses four display roles:

1. **TryOuts.MainBoard**  
2. **TryOuts.JudgePage**  
3. **TryOuts.SignIn**  
4. **TryOuts.ActDisplay** (new)

Each role is defined below.

---

# 🟦 **4. TryOuts.MainBoard — Layout & Behavior**

## **4.1 Purpose**
The MainBoard is the public‑facing display visible to the room.  
It must be visually polished, branded, and readable from a distance.

---

## **4.2 Layout Structure**

### **Top Bar (Full Width)**
- Background: PTA or school color  
- Left: PTA or school logo  
- Center: Session title  
  - Example: **“Try‑Outs — Session 2 (1:00 PM – 3:00 PM)”**  
- Right: Current time  

---

### **Main Content Area**

#### **A. Current Performer (Large Block)**
- Large performer name  
- Act title  
- Optional: music/video icon  
- “Now Performing” banner  
- Background: light color or subtle gradient  

#### **B. On Deck (Medium Block)**
- Next performer  
- Act title  
- “On Deck” label  
- Background: slightly darker shade  

---

### **Bottom Bar (Static Queue Preview)**
- Shows next **3–5** performers  
- Static list (no scrolling)  
- Background: muted color  

---

## **4.3 Visual Style**
- Clean, modern, high‑contrast  
- Large fonts  
- PTA or school branding  
- Optional subtle background image  
- No animation  

---

## **4.4 Font Scaling (Audience Display)**
The MainBoard supports **remote font scaling** controlled by the Director.

### **Controls (Director Dashboard)**
```
Font Size:
[ - Smaller ]   [ Bigger + ]
Slider: 50% — 150% (5% increments)
Reset to 100%
```

### **Behavior**
- Scaling adjusts a global `fontSizePercent`  
- Applied via CSS `transform: scale()`  
- Smooth transitions (`transition: transform 0.2s ease`)  
- Persisted in localStorage  
- Applied live via SignalR (`ApplyFontSize`)  

---

# 🟩 **5. TryOuts.JudgePage — Layout & Behavior**

## **5.1 Purpose**
Functional, minimal, readable on tablets or laptops.

## **5.2 Layout**
- Header: Performer name + Act title  
- Timer (read‑only)  
- “On Deck” performer  
- Judge completion button  

## **5.3 Scaling**
- Same scaling system as MainBoard  
- Controlled remotely  
- Applied via CSS transform  

---

# 🟧 **6. TryOuts.SignIn — Layout & Behavior**

## **6.1 Purpose**
Used by volunteers at the check‑in table.

## **6.2 Layout**
- Search bar  
- List of performers not checked in  
- Each row:
  - Performer name  
  - Act title  
  - Check‑In button  

## **6.3 Scaling**
- Same scaling system  
- Controlled remotely  

---

# 🟥 **7. TryOuts.ActDisplay — Layout & Behavior**

## **7.1 Purpose**
A performer‑facing display on stage.

---

## **7.2 Layout Structure**

### **Top Bar (Timer Bar)**
- Full‑width bar  
- Color transitions:
  - Green → Yellow → Red  
- Optional time text inside or above the bar  

---

### **Center Area (Video or Timer‑Only Mode)**

#### **If video exists:**
- Video takes **most of the screen**  
- Timer bar remains at top  
- No overlay on video  

#### **If no video:**
- Large centered timer  
- Progress bar below  

---

### **Bottom Area**
- Performer name  
- Act title  
- “You’re Performing” or “On Deck”  
- Optional PTA logo  

---

## **7.3 Remote Control**
Director can send:

- Start Timer  
- Reset Timer  
- Play Song  
- Stop Song  
- Play Video  
- Stop Video  
- Set Volume  

---

## **7.4 Audio Output Options**
Director chooses:

```
Audio Output:
( ) Director Computer
( ) Display Device
```

---

## **7.5 Scaling**
- Same scaling system  
- Video scales to fill available space  
- Timer bar height scales  
- Text scales proportionally  

---

# 🔄 **8. Real‑Time Update Model**

Displays receive updates via SignalR.

### **Message Types**
- `UpdateCurrentPerformer`  
- `UpdateOnDeck`  
- `UpdateQueue`  
- `StartTimer`  
- `ResetTimer`  
- `PlayMedia`  
- `StopMedia`  
- `SetVolume`  
- `ApplyFontSize`  
- `ExpireDisplay`  

---

# 🧹 **9. Clearing Displays**

At the end of a Try‑Outs session:

- Director may click **Clear Displays**  
- OR displays auto‑expire  
- Devices return to `/display` waiting mode  
- Roles and font scaling are wiped  

---

# 🎉 **10. Summary**

This document defines:

- Display registration  
- Role assignment  
- Layouts  
- Branding  
- **Font scaling (50%–150%, remote‑controlled)**  
- Timer behavior  
- Media behavior  
- Real‑time updates  
- Expiration rules  

This completes the Try‑Outs display architecture.

