# Debug Mode - Quick Testing Guide

## What is Debug Mode?

Debug Mode allows you to skip avatar selection and jump straight to the conversation phase for quick testing. Perfect for debugging audio and conversation issues without going through the full VR interaction flow.

## How to Enable Debug Mode

### Step 1: Select ExperimentManager
- In Hierarchy, find and select your `ExperimentManager` GameObject

### Step 2: Enable Debug Mode in Inspector
You'll see a new "Debug Mode" section at the top:

```
┌─────────────────────────────────────────────┐
│ Debug Mode                                  │
├─────────────────────────────────────────────┤
│ ☑ Debug Mode                                │
│   ┌─────────────────────────────────────┐   │
│   │ Debug Avatar                        │   │
│   │ ┌─────────────────────────────────┐ │   │
│   │ │ participant-black-female      ▼ │ │   │
│   │ └─────────────────────────────────┘ │   │
│   │                                     │   │
│   │ ☑ Skip Embodiment                   │   │
│   └─────────────────────────────────────┘   │
│                                             │
│ ℹ Debug Mode Active:                        │
│   • Avatar selection will be skipped        │
│   • Selected avatar: participant-black-...  │
│   • Embodiment: Skipped                     │
└─────────────────────────────────────────────┘
```

### Step 3: Configure Settings

**Debug Mode:** ☑ Check this box

**Debug Avatar:** Choose from dropdown:
- participant-black-female
- participant-white-female
- participant-asian-female
- participant-black-male
- participant-white-male
- participant-asian-male

**Skip Embodiment:** ☑ Check to go straight to conversation

### Step 4: Press Play!

The game will:
1. ✓ Skip avatar selection UI
2. ✓ Automatically select your chosen debug avatar
3. ✓ Skip embodiment phase (if enabled)
4. ✓ Start conversation immediately

## Testing Different Scenarios

### Test Female Avatar with Group 1
```
ParticipantIDManager:
  - Group Number: 1

ExperimentManager:
  - Debug Mode: ☑
  - Debug Avatar: participant-black-female
  - Skip Embodiment: ☑
```
→ Will load: `conversation-audio/female-participant/Group1/`

### Test Male Avatar with Group 2
```
ParticipantIDManager:
  - Group Number: 2

ExperimentManager:
  - Debug Mode: ☑
  - Debug Avatar: participant-white-male
  - Skip Embodiment: ☑
```
→ Will load: `conversation-audio/male-participant/Group2/`

### Test with Embodiment Phase
```
ExperimentManager:
  - Debug Mode: ☑
  - Debug Avatar: participant-asian-female
  - Skip Embodiment: ☐ (unchecked)
```
→ Will play embodiment audio, then start conversation

## Console Output in Debug Mode

When you press Play, you'll see:
```
[DEBUG MODE] Using avatar: participant-black-female
[DEBUG MODE] Skipping embodiment phase
Step 1: Event Received for participant-black-female
Extracted - Gender: female, Ethnicity: black
Step 2: Avatar Manager Done
ConversationManager: Gender set to female, Group set to 1
Step 3: Conversation Bound
[ConversationManager] Loading audio clips from: Resources/conversation-audio/female-participant/Group1
[ConversationManager] Found 20 clips at path: conversation-audio/female-participant/Group1
```

## Switching Back to Normal Mode

To return to normal VR mode:
1. Select ExperimentManager
2. Uncheck "Debug Mode"
3. Press Play

The game will work normally with avatar selection UI.

## Common Debug Workflows

### Workflow 1: Test Audio Loading
```
Goal: Check if audio files are loading correctly

Steps:
1. Enable Debug Mode
2. Select any avatar
3. Check Skip Embodiment
4. Press Play
5. Watch Console for [ConversationManager] messages
6. Look for "Found X clips" - should be > 0
```

### Workflow 2: Test Different Groups
```
Goal: Verify all 3 groups have audio

Steps:
1. Enable Debug Mode
2. Set Group Number to 1
3. Press Play → Check audio plays
4. Stop
5. Set Group Number to 2
6. Press Play → Check audio plays
7. Stop
8. Set Group Number to 3
9. Press Play → Check audio plays
```

### Workflow 3: Test Gender Detection
```
Goal: Verify male/female audio paths work

Steps:
1. Enable Debug Mode
2. Select participant-black-female
3. Press Play → Should load female-participant audio
4. Stop
5. Select participant-black-male
6. Press Play → Should load male-participant audio
```

### Workflow 4: Test Embodiment Phase
```
Goal: Test embodiment audio and flow

Steps:
1. Enable Debug Mode
2. Uncheck Skip Embodiment
3. Press Play
4. Embodiment audio should play
5. Then conversation starts automatically
```

## Troubleshooting Debug Mode

### Issue: Avatar Not Appearing
- Check that avatar name matches exactly (case-sensitive)
- Verify AvatarManager has the avatar in its list
- Look for error: "Participant not found: ..."

### Issue: Still Shows Avatar Selection UI
- Ensure Debug Mode checkbox is checked
- Save the scene (Ctrl+S)
- Try restarting Unity Editor

### Issue: Audio Still Not Playing
- Debug Mode doesn't fix audio issues, just skips UI
- Follow AUDIO_FIX_CHECKLIST.md
- Check Console for [Audio] messages

## Tips

1. **Quick Iteration:** Leave Debug Mode on while testing audio
2. **Test All Avatars:** Use dropdown to quickly switch between avatars
3. **Check Console:** Always watch Console for detailed logs
4. **Save Settings:** Unity remembers your debug settings between sessions
5. **Production Build:** Remember to disable Debug Mode before building!

## Keyboard Shortcuts (Future Enhancement)

You could add these to ExperimentManager:
- `D` key: Toggle Debug Mode
- `1-6` keys: Select avatar 1-6
- `E` key: Toggle Skip Embodiment

## Disabling for Production

Before building your final APK:
1. Select ExperimentManager
2. Uncheck "Debug Mode"
3. Build & Run

Or add this check in code:
```csharp
#if UNITY_EDITOR
    public bool debugMode = false;
#else
    public bool debugMode = false; // Always false in builds
#endif
```
