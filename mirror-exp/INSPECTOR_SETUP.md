# Inspector Setup - Visual Guide

## ParticipantIDManager Component

When you add the ParticipantIDManager component to a GameObject, you'll see these fields in the Inspector:

```
┌─────────────────────────────────────────────┐
│ ParticipantIDManager (Script)              │
├─────────────────────────────────────────────┤
│ Script: ParticipantIDManager                │
│                                             │
│ Inspector Settings                          │
│ ┌─────────────────────────────────────────┐ │
│ │ Participant ID                          │ │
│ │ ┌─────────────────────────────────────┐ │ │
│ │ │ P01                                 │ │ │
│ │ └─────────────────────────────────────┘ │ │
│ │ Enter participant ID (e.g., P01, P02...)│ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ ┌─────────────────────────────────────────┐ │
│ │ Group Number                            │ │
│ │ ┌───────────────────────────────────┐   │ │
│ │ │ ●─────────────────────────────    │   │ │
│ │ │ 1                         2    3  │   │ │
│ │ └───────────────────────────────────┘   │ │
│ │ Group number (1-3) - determines which   │ │
│ │ audio folder to use                     │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

## Quick Setup Steps

### Step 1: Create the GameObject
1. Right-click in Hierarchy
2. Select "Create Empty"
3. Name it "ParticipantIDManager"

### Step 2: Add the Component
1. With ParticipantIDManager selected
2. Click "Add Component" in Inspector
3. Type "ParticipantIDManager"
4. Select it from the list

### Step 3: Configure Settings
1. **Participant ID**: Type your participant identifier
   - Examples: "P01", "P02", "P03", "Participant_001"
   - This is mainly for logging/record keeping
   
2. **Group Number**: Drag the slider or type a number (1-3)
   - 1 = Uses Group1 audio folder
   - 2 = Uses Group2 audio folder
   - 3 = Uses Group3 audio folder

### Step 4: Link to ExperimentManager
1. Select your ExperimentManager GameObject
2. Find the "Participant ID Manager" field
3. Drag the ParticipantIDManager GameObject into this field

## Example Configurations

### Participant 1, Group 1
```
Participant ID: P01
Group Number: 1
```
→ Will use: `conversation-audio/female-participant/Group1/` (if female avatar chosen)

### Participant 2, Group 2
```
Participant ID: P02
Group Number: 2
```
→ Will use: `conversation-audio/male-participant/Group2/` (if male avatar chosen)

### Participant 3, Group 3
```
Participant ID: P03
Group Number: 3
```
→ Will use: `conversation-audio/female-participant/Group3/` (if female avatar chosen)

## Testing Different Configurations

To test different participants/groups:
1. Stop the game if running
2. Change the Inspector values
3. Press Play again
4. No code changes needed!

## Common Scenarios

### Running Multiple Test Sessions
Before each session:
1. Update Participant ID (P01, P02, P03...)
2. Set appropriate Group Number (1, 2, or 3)
3. Run the application

### Switching Between Groups
Just change the Group Number slider:
- Group 1: Different conversation audio set
- Group 2: Different conversation audio set
- Group 3: Different conversation audio set

The gender (female/male) is determined by the avatar the participant chooses in-game.

## Validation

The system automatically validates:
- Empty Participant ID → Defaults to "P01"
- Group Number < 1 or > 3 → Defaults to 1
- Check Console for validation messages

## Debug Output

When the game starts, you'll see in the Console:
```
Participant ID: P01, Group: 1
ExperimentManager: Using Participant ID 'P01' with Group 1
```

This confirms your settings are being read correctly.
