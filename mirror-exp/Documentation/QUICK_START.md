# Quick Start - Participant ID System

## What This Does

Automatically loads the correct conversation audio based on:
1. **Avatar Gender** (chosen by participant in-game)
2. **Group Number** (set by you in Inspector)

## 3-Step Setup

### 1. Create ParticipantIDManager
- Create empty GameObject: "ParticipantIDManager"
- Add component: `ParticipantIDManager`
- Set values in Inspector:
  - Participant ID: "P01"
  - Group Number: 1

### 2. Link to ExperimentManager
- Select ExperimentManager GameObject
- Drag ParticipantIDManager into "Participant ID Manager" field

### 3. Done!
- Run the game
- Choose an avatar
- System automatically loads correct audio

## How It Works

```
Inspector Settings:
├─ Participant ID: "P01" (for your records)
└─ Group Number: 1 (determines audio folder)

Avatar Choice:
└─ "participant-black-female" (chosen in-game)

Result:
└─ Loads audio from: conversation-audio/female-participant/Group1/
```

## Changing Settings

Before each test session:
1. Update Participant ID (P01, P02, P03...)
2. Change Group Number (1, 2, or 3)
3. Press Play

No code changes needed!

## Audio Folder Structure

Your audio files should be organized like this:

```
Assets/Resources/conversation-audio/
├── female-participant/
│   ├── Group1/
│   ├── Group2/
│   └── Group3/
└── male-participant/
    ├── Group1/
    ├── Group2/
    └── Group3/
```

## Example Scenarios

**Scenario 1:**
- Inspector: Group Number = 1
- Participant chooses: Female avatar
- Result: Loads from `female-participant/Group1/`

**Scenario 2:**
- Inspector: Group Number = 2
- Participant chooses: Male avatar
- Result: Loads from `male-participant/Group2/`

**Scenario 3:**
- Inspector: Group Number = 3
- Participant chooses: Female avatar (any ethnicity)
- Result: Loads from `female-participant/Group3/`

## That's It!

The system handles everything else automatically:
- Gender detection from avatar name
- Opposite gender interlocutor selection
- Audio path construction
- Audio file loading

For detailed setup instructions, see `INSPECTOR_SETUP.md`
For technical details, see `PARTICIPANT_ID_IMPLEMENTATION.md`
