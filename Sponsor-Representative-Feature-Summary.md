# Sponsor Representative Feature Implementation

## Overview
Added a "Sponsor Representative" checkbox to the profile management system, allowing sponsor representatives to publicly identify themselves within the PTA community. This enhancement improves community networking and sponsor recognition.

## Changes Made

### 1. **ApplicationUser Model Enhancement**
**File**: `LuxfordPTAWeb.Shared/Models/ApplicationUser.cs`

Added the `IsSponsor` field to track sponsor representatives:
```csharp
public bool IsSponsor { get; set; } = false; // Sponsor representative
```

**Benefits**:
- Persistent storage of sponsor representative status
- Consistent with existing PTA role fields (IsParent, IsTeacher, IsStaff)
- Default value of `false` ensures non-breaking change

### 2. **Profile Management Interface**
**File**: `LuxfordPTAWeb/Components/Account/Pages/Manage/Index.razor`

#### Enhanced UI Layout
- **Improved Responsive Grid**: Changed from 3-column to 4-column layout using `col-lg-3 col-md-4 col-sm-6` classes
- **Added Sponsor Checkbox**: New "Sponsor Representative" checkbox with proper binding
- **Enhanced Information**: Added helpful text explaining the purpose of role selection

#### Updated Form Logic
```csharp
// Initialization (preserves form submissions)
if (!HasFormData())
{
    Input.IsParent = user.IsParent;
    Input.IsTeacher = user.IsTeacher;
    Input.IsStaff = user.IsStaff;
    Input.IsSponsor = user.IsSponsor; // New field
}

// Save logic (detects changes)
if (Input.IsSponsor != user.IsSponsor)
{
    user.IsSponsor = Input.IsSponsor;
    hasChanges = true;
}
```

#### InputModel Update
```csharp
public bool IsSponsor { get; set; } = false;
```

### 3. **Public DTO Enhancement**
**File**: `LuxfordPTAWeb.Shared/DTOs/AssignedUserDTO.cs`

#### Added Community Role Fields
```csharp
public bool IsParent { get; set; }
public bool IsTeacher { get; set; }
public bool IsStaff { get; set; }
public bool IsSponsor { get; set; } // New field
```

#### Smart Display Property
```csharp
public string CommunityRoles
{
    get
    {
        var roles = new List<string>();
        
        if (IsParent) roles.Add("Parent/Guardian");
        if (IsTeacher) roles.Add("Teacher");
        if (IsStaff) roles.Add("School Staff");
        if (IsSponsor) roles.Add("Sponsor Representative"); // New role
        
        return roles.Any() ? string.Join(", ", roles) : "Community Member";
    }
}
```

#### Updated Extension Method
```csharp
public static AssignedUserDTO? ToAssignedUserDTO(this BoardPosition boardPosition)
{
    // ... existing code ...
    // Include public community role information
    IsParent = boardPosition.AssignedUser.IsParent,
    IsTeacher = boardPosition.AssignedUser.IsTeacher,
    IsStaff = boardPosition.AssignedUser.IsStaff,
    IsSponsor = boardPosition.AssignedUser.IsSponsor // New field
}
```

### 4. **Home Page Display Enhancement**
**File**: `LuxfordPTAWeb.Client/Pages/Home.razor`

Enhanced the board member modal to display community roles:
```razor
<h6 class="mb-0 opacity-75">
    <i class="bi bi-person me-2"></i>@selectedMember.DisplayName
</h6>
@if (!string.IsNullOrWhiteSpace(selectedMember.CommunityRoles))
{
    <div class="mt-1">
        <small class="opacity-75">
            <i class="bi bi-people me-1"></i>@selectedMember.CommunityRoles
        </small>
    </div>
}
```

## User Experience Features

### Profile Management
- **Responsive Layout**: 4 checkboxes arrange intelligently on different screen sizes:
  - Large screens: 4 columns side-by-side
  - Medium screens: 2 rows of 2 columns
  - Small screens: 4 rows stacked
- **Clear Labeling**: "Sponsor Representative" is descriptive and professional
- **Helpful Guidance**: Added explanatory text about role selection purpose

### Public Display
- **Board Member Modals**: Community roles appear under the member's name
- **Professional Format**: "Parent/Guardian, Sponsor Representative" style display
- **Context Appropriate**: Only shows when roles are selected

### Privacy & Security
- **Public Information**: Community roles are considered appropriate for public display
- **No Sensitive Data**: Does not expose any contact or personal information
- **User Choice**: Members opt-in to displaying their sponsor representative status

## Benefits for PTA Organization

### Sponsor Recognition
- **Visibility**: Sponsor representatives can be easily identified by community
- **Networking**: Facilitates connections between sponsors and PTA members
- **Appreciation**: Public recognition of sponsor participation in governance

### Community Building
- **Transparency**: Clear view of board composition and community connections
- **Diversity**: Shows range of stakeholders involved in PTA leadership
- **Engagement**: Encourages sponsor participation in PTA activities

### Administrative Benefits
- **Data Quality**: Structured tracking of sponsor representative status
- **Reporting**: Can generate reports on sponsor involvement in governance
- **Future Features**: Foundation for sponsor-specific functionality

## Example Scenarios

### Sponsor Representative Profile
```
John Smith - Treasurer
Parent/Guardian, Sponsor Representative
```

This clearly shows that John serves as Treasurer, is a parent, AND represents a sponsor organization.

### Multi-Role Display
```
Sarah Johnson - Vice President
Parent/Guardian, Teacher, Sponsor Representative
```

Shows comprehensive community involvement and multiple connections to the school.

### Single Role Display
```
Mike Davis - Secretary
Sponsor Representative
```

Clean display for sponsor representatives who may not have other direct school connections.

## Technical Implementation

### Database Considerations
- **Migration**: New `IsSponsor` field added to ApplicationUser table
- **Indexing**: Consider adding index if filtering/reporting by sponsor status is needed
- **Default Values**: Existing users default to `false` for non-breaking change

### API Endpoints
- **Public Endpoints**: Include sponsor status in AssignedUserDTO responses
- **Admin Endpoints**: Full ApplicationUser model includes sponsor field
- **Consistent**: All existing endpoints automatically include new field

### Security & Privacy
- **Public Display**: Only shows community role status, not personal sponsor details
- **User Control**: Users opt-in to displaying sponsor representative status
- **No Linking**: Does not expose which specific sponsor organization they represent

## Files Modified
- ? **LuxfordPTAWeb.Shared/Models/ApplicationUser.cs** - Added IsSponsor field
- ? **LuxfordPTAWeb/Components/Account/Pages/Manage/Index.razor** - Added sponsor checkbox and logic
- ? **LuxfordPTAWeb.Shared/DTOs/AssignedUserDTO.cs** - Added community roles display
- ? **LuxfordPTAWeb.Client/Pages/Home.razor** - Added community roles to modal display

## Build Status
- ? **No compilation errors**
- ? **Backwards compatible changes**
- ? **Enhanced user experience**
- ? **Professional sponsor recognition**

The sponsor representative feature is now fully integrated into the profile management system and will be displayed publicly on the home page board member information, providing appropriate recognition for sponsors who participate in PTA governance while maintaining privacy and professional presentation.