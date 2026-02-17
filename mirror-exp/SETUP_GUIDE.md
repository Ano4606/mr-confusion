# Quick Setup Guide for Participant ID System

## Unity Inspector Setup (Simple Version)

### 1. Create ParticipantIDManager GameObject

1. In your scene, create empty GameObject: `GameObject > Create Empty`
2. Name it: "ParticipantIDManager"
3. Add component: `ParticipantIDManager`
4. In the Inspector, set:
   - **Participant ID**: "P01" (or P02, P03, etc.)
   - **Group Number**: 1 (slider: 1-3)

That's it! No UI setup needed.

### 2. Update ExperimentManager

Find your ExperimentManager GameObject and:
1. Assign `ParticipantIDManager` reference in the Inspector
2. Ensure all other references are still assigned:
   - Choose Phase
   - Embodiment Phase
   - Conversation Phase
   - Avatar Manager
   - Mirror

### 3. Verify Audio Folder Structure

Ensure your audio files are organized as:

```
Assets/Resources/conversation-audio/
├── female-participant/
│   ├── Group1/
│   │   ├── I_10.mp3
│   │   ├── SA_1.mp3
│   │   └── ...
│   ├── Group2/
│   │   └── ...
│   └── Group3/
│       └── ...
└── male-participant/
    ├── Group1/
    │   └── ...
    ├── Group2/
    │   └── ...
    └── Group3/
        └── ...
```

## Testing Flow

1. **Set Inspector Values**
   - In ParticipantIDManager: Set Participant ID to "P01" and Group Number to 1

2. **Start Application**
   - Avatar selection panel appears immediately

3. **Select Avatar**
   - Choose gender and ethnicity
   - Example: "participant-black-female"

4. **System Automatically:**
   - Extracts gender: "female"
   - Uses group from Inspector: 1
   - Loads audio from: `conversation-audio/female-participant/Group1/`
   - Selects opposite gender interlocutor: "interlocutor-black-male"

5. **Embodiment Phase** → **Conversation Phase**
   - Audio plays from correct gender/group folder

## Changing Participant Settings

Simply update the Inspector values before running:
- **Participant ID**: Change to P01, P02, P03, etc. (for record keeping)
- **Group Number**: Change slider to 1, 2, or 3 (determines audio folder)

No need to rebuild or modify code!

## Troubleshooting

### Audio Not Loading
- Check Debug Console for path: `Loading audio clips from: conversation-audio/...`
- Verify folder structure matches exactly
- Ensure audio files are in Resources folder
- Check file names match CSV references

### Gender Not Detected
- Verify avatar names follow format: `participant-{ethnicity}-{gender}`
- Check Debug Console for: `Extracted - Gender: ..., Ethnicity: ...`

### Participant ID Not Working
- Ensure ParticipantIDManager is assigned in ExperimentManager
- Check Inspector values are set correctly
- Verify group number is between 1-3

## Debug Logs to Watch

```
Participant ID: P01, Group: 1
ExperimentManager: Using Participant ID 'P01' with Group 1
Extracted - Gender: female, Ethnicity: black
ConversationManager: Gender set to female, Group set to 1
Loading audio clips from: conversation-audio/female-participant/Group1
Preloaded 20 audio clips from conversation-audio/female-participant/Group1
```

## Optional: Runtime Updates

If you need to change settings at runtime (via code):

```csharp
ParticipantIDManager.Instance.UpdateSettings("P02", 2);
```

This will update the participant ID and group number dynamically.
