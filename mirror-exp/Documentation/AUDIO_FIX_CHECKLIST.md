# Audio Not Playing - Quick Fix Checklist

## Most Common Issues (Check These First!)

### 1. ✓ AudioListener Missing
**Problem:** No AudioListener in scene = No audio at all

**Check:**
- Look at your Main Camera (or XR Rig camera)
- Should have "Audio Listener" component

**Fix:**
- Select Main Camera
- Add Component → Audio → Audio Listener

---

### 2. ✓ AudioSource Volume/Mute Settings
**Problem:** AudioSource exists but volume is 0 or muted

**Check:**
- Find AudioInterlocutor and AudioAvatar GameObjects in scene
- Look at AudioSource component in Inspector

**Fix:**
- Volume: Set to 1.0
- Mute: Uncheck
- Enabled: Check the checkbox

---

### 3. ✓ Gender/Group Not Set
**Problem:** Audio path is wrong because gender/group are empty

**Check Console for:**
```
ConversationManager: Gender not set!
ConversationManager: Group number not set!
```

**Fix:**
- Ensure ParticipantIDManager is assigned in ExperimentManager Inspector
- Set Group Number in ParticipantIDManager (1-3)
- Make sure you select an avatar before conversation starts

---

### 4. ✓ Audio Files Not in Resources Folder
**Problem:** Audio files aren't where Unity expects them

**Required Structure:**
```
Assets/
└── Resources/
    └── conversation-audio/
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

**Check:**
- Files MUST be in `Assets/Resources/conversation-audio/...`
- Folder names are case-sensitive
- Use exactly: `female-participant` and `male-participant`

---

### 5. ✓ 3D Audio Settings (VR Issue)
**Problem:** Audio is 3D and avatars are too far away

**Check:**
- Select AudioInterlocutor or AudioAvatar
- Look at AudioSource → Spatial Blend

**Fix:**
- Set Spatial Blend to 0 (2D audio - always audible)
- OR set to 0.5 (hybrid)
- OR if keeping 3D:
  - Min Distance: 1-2
  - Max Distance: 10-20
  - Ensure avatars are close to player

---

## Quick Debug Steps

### Step 1: Check Console When Game Starts

You should see:
```
✓ Participant ID: P01, Group: 1
✓ ConversationManager: Gender set to female, Group set to 1
✓ [ConversationManager] Found 20 clips at path: conversation-audio/female-participant/Group1
✓ [ConversationManager] Cached audio: I_10
✓ [ConversationManager] Cached audio: SA_1
```

If you see:
```
✗ [ConversationManager] Found 0 clips at path: ...
```
→ Audio files are not in the right folder!

---

### Step 2: Check Console During Conversation

You should see:
```
✓ [Audio] Looking for clip: 'I_10' for speaker: I
✓ [Audio] Playing clip: I_10 on InterlocutorAudioSource, Volume: 1, Mute: False
✓ [Audio] ✓ Audio is playing
```

If you see:
```
✗ [Audio] AudioSource is null for speaker: I
```
→ AudioInterlocutor or AudioAvatar is not assigned!

```
✗ [Audio] ✗ Audio clip not found: 'I_10'
```
→ Audio file name doesn't match CSV or wrong folder!

---

### Step 3: Manual Test

1. **Find AudioSource in Scene:**
   - Look for GameObject with AudioSource component
   - Could be on Interlocutor or Participant avatar

2. **Assign Test Clip:**
   - In Inspector, drag any audio file to "Audio Clip" field
   - Set Volume to 1.0

3. **Press Play:**
   - While game is running, click "Play" button in AudioSource Inspector
   - Can you hear it?
     - YES → Audio system works, issue is with file loading
     - NO → Check AudioListener, volume, mute settings

---

## Inspector Settings to Verify

### ParticipantIDManager
```
Participant ID: P01
Group Number: 1 (slider between 1-3)
```

### ExperimentManager
```
Participant ID Manager: [Assigned]
Choose Phase: [Assigned]
Embodiment Phase: [Assigned]
Conversation Phase: [Assigned]
Avatar Manager: [Assigned]
```

### AudioSource (on Interlocutor/Avatar)
```
Audio Clip: (will be set at runtime)
Volume: 1.0
Mute: ☐ (unchecked)
Spatial Blend: 0 (2D) or 0.5 (hybrid)
Min Distance: 1-2 (if 3D)
Max Distance: 10-20 (if 3D)
```

---

## Still Not Working?

### Add AudioDebugHelper

1. Create empty GameObject: "AudioDebugger"
2. Add Component: `AudioDebugHelper`
3. Press Play
4. Check Console for detailed diagnostics

### Check These Advanced Issues

1. **Multiple AudioListeners**
   - Only ONE should be active
   - Disable extras

2. **Audio Mixer**
   - If using Audio Mixer, check it's not muted
   - Check volume isn't at -80dB

3. **VR Spatializer**
   - Meta XR Audio might be interfering
   - Try disabling spatializer temporarily

4. **Build vs Editor**
   - Does it work in Editor but not in build?
   - Ensure audio files are in Resources folder

---

## What to Report if Still Broken

Copy these from Console and share:
1. All lines starting with `[ConversationManager]`
2. All lines starting with `[Audio]`
3. Screenshot of AudioSource Inspector
4. Screenshot of Resources folder structure
5. Which avatar you selected (male/female)
6. ParticipantIDManager settings
