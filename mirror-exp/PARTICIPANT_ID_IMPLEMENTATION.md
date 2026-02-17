# Participant ID and Audio Path Implementation

## Overview
This implementation adds participant ID management and gender-based audio path selection to the mirror experiment.

## Components Created/Modified

### 1. ParticipantIDManager.cs (NEW)
- Manages participant ID and group number via Inspector fields
- Simple public fields: `participantID` (string) and `groupNumber` (int, 1-3)
- Validates settings on Awake
- Auto-confirms settings on Start
- Singleton pattern for global access
- Fires `OnParticipantIDConfirmed` event automatically
- Optional `UpdateSettings()` method for runtime changes

**Usage:**
- Set values in Unity Inspector before running
- Participant ID: "P01", "P02", "P03" (for record keeping)
- Group Number: 1, 2, or 3 (determines audio subfolder)
- No UI setup required!

### 2. AvatarManager.cs (MODIFIED)
- Added `SelectedGender` property (female/male)
- Added `SelectedEthnicity` property (black/white/asian)
- New method `ExtractGenderAndEthnicity()` parses avatar name
- Extracts gender from participant name format: "participant-{ethnicity}-{gender}"

**Example:**
- "participant-black-female" → Gender: "female", Ethnicity: "black"
- "participant-white-male" → Gender: "male", Ethnicity: "white"

### 3. ConversationManager.cs (MODIFIED)
- Added `participantGender` and `groupNumber` fields
- New method `SetGenderAndGroup()` to configure audio path
- Modified `PreloadAudioClips()` to load from gender-specific paths

**Audio Path Logic:**
```
Resources/conversation-audio/{gender}-participant/Group{number}/
```

**Examples:**
- Female participant, Group 1: `conversation-audio/female-participant/Group1/`
- Male participant, Group 2: `conversation-audio/male-participant/Group2/`

### 4. ExperimentManager.cs (MODIFIED)
- Added reference to `ParticipantIDManager`
- Subscribes to `OnParticipantIDConfirmed` event
- Flow: Avatar Selection → Embodiment → Conversation
- Passes gender and group info to ConversationManager
- ParticipantIDManager settings are read from Inspector

## Flow Sequence

1. **App Start** → ParticipantIDManager reads Inspector values
2. **Settings Auto-Confirmed** → Avatar selection phase appears
3. **Avatar Selected** (e.g., "participant-black-female")
   - AvatarManager extracts gender: "female"
   - AvatarManager selects opposite gender interlocutor
4. **ConversationManager configured**
   - Gender: "female"
   - Group: 1
   - Audio path: `conversation-audio/female-participant/Group1/`
5. **Embodiment phase** → **Conversation phase** with correct audio

## Audio Folder Structure

```
Assets/Resources/conversation-audio/
├── female-participant/
│   ├── Group1/
│   │   ├── I_10.mp3
│   │   ├── SA_1.mp3
│   │   └── ...
│   ├── Group2/
│   └── Group3/
└── male-participant/
    ├── Group1/
    ├── Group2/
    └── Group3/
```

## Setup Instructions

1. **Add ParticipantIDManager to Scene:**
   - Create empty GameObject named "ParticipantIDManager"
   - Add ParticipantIDManager component
   - Set Inspector values:
     - Participant ID: "P01" (or P02, P03, etc.)
     - Group Number: 1 (slider: 1-3)

2. **Update ExperimentManager:**
   - Assign ParticipantIDManager reference in inspector

3. **Audio Files:**
   - Ensure audio files are organized in the correct folder structure
   - Female participant audio: `Resources/conversation-audio/female-participant/Group{1-3}/`
   - Male participant audio: `Resources/conversation-audio/male-participant/Group{1-3}/`

## Key Features

- **Gender-based audio selection:** Automatically selects correct audio folder based on chosen avatar gender
- **Group-based audio selection:** Uses participant ID to determine which group's audio to load
- **Ethnicity-agnostic:** Gender selection ignores ethnicity (as requested)
- **Flexible ID format:** Accepts various formats (P01, Group1, 1, etc.)
- **Error handling:** Validates input and provides feedback
- **Fallback:** If audio not found in specific path, falls back to default

## Notes

- Participant ID is stored globally via singleton pattern
- Group numbers are 1-3 (configurable via minGroupID/maxGroupID)
- Gender is extracted from avatar name format
- Interlocutor is always opposite gender, same ethnicity
