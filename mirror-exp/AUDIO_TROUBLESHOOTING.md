# Audio Troubleshooting Guide

## Common Audio Issues and Solutions

### Issue 1: No Audio Playing At All

**Possible Causes:**

1. **No AudioListener in Scene**
   - Unity requires exactly ONE active AudioListener (usually on Main Camera)
   - Check: Look for AudioListener component on your camera
   - Fix: Add AudioListener to Main Camera if missing

2. **AudioSource Volume is 0 or Muted**
   - Check Inspector: AudioInterlocutor and AudioAvatar volumes
   - Fix: Set volume to 1.0, ensure "Mute" is unchecked

3. **AudioSource is Disabled**
   - Check: AudioSource component is enabled (checkbox checked)
   - Fix: Enable the AudioSource component

4. **Gender/Group Not Set Before Audio Loading**
   - Check Console for: "Gender not set!" or "Group number not set!"
   - Fix: Ensure ParticipantIDManager is properly linked in ExperimentManager

### Issue 2: Audio Files Not Loading

**Check Console Logs:**

Look for these messages:
```
[ConversationManager] Loading audio clips from: Resources/conversation-audio/female-participant/Group1
[ConversationManager] Found 0 clips at path: conversation-audio/female-participant/Group1
```

**If you see "Found 0 clips":**

1. **Verify Folder Structure**
   ```
   Assets/Resources/conversation-audio/
   ├── female-participant/
   │   ├── Group1/
   │   │   ├── I_10.mp3
   │   │   └── SA_1.mp3
   │   ├── Group2/
   │   └── Group3/
   └── male-participant/
       ├── Group1/
       ├── Group2/
       └── Group3/
   ```

2. **Check Audio Import Settings**
   - Select audio file in Unity
   - Inspector → Load Type: Should be "Decompress On Load" or "Compressed In Memory"
   - Ensure "Preload Audio Data" is checked

3. **Verify File Names Match CSV**
   - Audio files should match the names in your CSV file
   - Example: If CSV says "I_10.mp3", file should be named "I_10.mp3"

### Issue 3: Some Audio Plays, Some Doesn't

**Check Console for:**
```
[Audio] ✗ Audio clip not found: 'I_10' (searched for: 'I_10')
[Audio] Available clips in cache: SA_1, SA_2, I_11, I_12
```

**Solutions:**

1. **File Name Mismatch**
   - CSV references "I_10" but file is named "I_10_final"
   - Fix: Rename files to match CSV exactly

2. **Wrong Group Folder**
   - Audio is in Group2 but system is loading Group1
   - Fix: Check ParticipantIDManager Group Number setting

3. **Case Sensitivity**
   - File is "i_10.mp3" but CSV says "I_10.mp3"
   - The code handles this, but check if issue persists

### Issue 4: Audio Plays But Can't Hear It

**3D Audio Settings:**

1. **Check Spatial Blend**
   - Select AudioSource in Inspector
   - Spatial Blend: 0 = 2D (always audible), 1 = 3D (position-based)
   - For conversation, use 0 (2D) or low value (0.2-0.5)

2. **Check 3D Sound Settings**
   - Min Distance: How close you need to be to hear at full volume
   - Max Distance: How far before audio is inaudible
   - For VR conversations, set Min Distance: 1-2, Max Distance: 10-20

3. **Avatar Position**
   - If using 3D audio, ensure avatars are close to the player/camera
   - Check avatar positions in Scene view during playback

## Debugging Steps

### Step 1: Use AudioDebugHelper

1. Create empty GameObject: "AudioDebugger"
2. Add component: `AudioDebugHelper`
3. Press Play
4. Check Console for detailed audio setup info

### Step 2: Check Console Logs

Run the game and look for these key messages:

```
✓ Good:
Participant ID: P01, Group: 1
ConversationManager: Gender set to female, Group set to 1
[ConversationManager] Found 20 clips at path: conversation-audio/female-participant/Group1
[Audio] Playing clip: I_10 on InterlocutorAudioSource, Volume: 1, Mute: False
[Audio] ✓ Audio is playing

✗ Bad:
[ConversationManager] Found 0 clips at path: conversation-audio/female-participant/Group1
[Audio] ✗ Audio clip not found: 'I_10'
[Audio] AudioSource is null for speaker: I
```

### Step 3: Manual Audio Test

1. Select an AudioSource in Hierarchy
2. Assign any audio clip in Inspector
3. Press Play
4. In Inspector, click the "Play" button on AudioSource
5. If you hear it → AudioSource works, issue is with loading
6. If you don't hear it → Check AudioListener, volume, mute settings

### Step 4: Verify Audio Path

Add this temporary code to ConversationManager.Start():

```csharp
void Start()
{
    // Test audio loading
    AudioClip[] testClips = Resources.LoadAll<AudioClip>("conversation-audio/female-participant/Group1");
    Debug.Log($"TEST: Found {testClips.Length} clips in female-participant/Group1");
    foreach (var clip in testClips)
    {
        Debug.Log($"  - {clip.name}");
    }
}
```

## Quick Checklist

- [ ] AudioListener exists in scene (on Main Camera)
- [ ] Only ONE AudioListener is active
- [ ] AudioSource components are enabled
- [ ] AudioSource volume is > 0
- [ ] AudioSource is not muted
- [ ] ParticipantIDManager is assigned in ExperimentManager
- [ ] ParticipantIDManager has Group Number set (1-3)
- [ ] Audio files are in correct Resources folder structure
- [ ] Audio file names match CSV references
- [ ] SetGenderAndGroup() is called before StartTask()
- [ ] Console shows "Found X clips" where X > 0

## Still Not Working?

Check these advanced issues:

1. **Audio Mixer Groups**
   - If using Audio Mixer, check volume levels
   - Ensure mixer isn't muted or volume at -80dB

2. **VR Audio Settings**
   - Check Meta XR Audio settings
   - Ensure spatializer isn't interfering

3. **Build Settings**
   - Audio files must be in Resources folder to work in builds
   - Check if audio works in Editor but not in build

4. **CSV File Issues**
   - Verify CSV is named correctly (should be "rando" not "rando_balanced")
   - Check CSV format matches expected columns

## Getting Help

When asking for help, provide:
1. Console logs (especially [ConversationManager] and [Audio] messages)
2. Screenshot of AudioSource Inspector settings
3. Screenshot of Resources folder structure
4. ParticipantIDManager Inspector settings
5. Which speaker isn't working (I = Interlocutor, SA = Self Avatar)
