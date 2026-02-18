# Quick Debug Setup - 30 Second Guide

## Enable Debug Mode (3 Steps)

### 1. Select ExperimentManager in Hierarchy

### 2. Check "Debug Mode" in Inspector
```
Debug Mode
☑ Debug Mode
  Debug Avatar: [participant-black-female ▼]
  ☑ Skip Embodiment
```

### 3. Press Play ▶

Done! Conversation starts immediately.

---

## What Each Setting Does

| Setting | What It Does |
|---------|--------------|
| **Debug Mode** | Skips avatar selection UI |
| **Debug Avatar** | Which avatar to use (male/female, ethnicity) |
| **Skip Embodiment** | Skip embodiment phase, go straight to conversation |

---

## Quick Test Scenarios

### Test Female Audio, Group 1
```
ParticipantIDManager → Group Number: 1
ExperimentManager → Debug Avatar: participant-black-female
Press Play ▶
```

### Test Male Audio, Group 2
```
ParticipantIDManager → Group Number: 2
ExperimentManager → Debug Avatar: participant-white-male
Press Play ▶
```

### Test All Groups (Female)
```
1. Group Number: 1 → Play → Stop
2. Group Number: 2 → Play → Stop
3. Group Number: 3 → Play → Stop
```

---

## Console Messages to Look For

### ✓ Good
```
[DEBUG MODE] Using avatar: participant-black-female
ConversationManager: Gender set to female, Group set to 1
[ConversationManager] Found 20 clips at path: ...
[Audio] ✓ Audio is playing
```

### ✗ Bad
```
[ConversationManager] Found 0 clips at path: ...
[Audio] ✗ Audio clip not found
[Audio] AudioSource is null
```

---

## Turn Off Debug Mode

Uncheck "Debug Mode" → Normal VR mode with avatar selection

---

## Remember

- Debug Mode = Testing only
- Disable before building APK
- Check Console for detailed logs
- Audio issues? See AUDIO_FIX_CHECKLIST.md
