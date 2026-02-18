# SA Audio Files Not Loading - Troubleshooting

## Problem
Interlocutor audio (I_*.mp3) loads fine, but Self-Avatar audio (SA_*.mp3) doesn't load.

## Quick Diagnostic Steps

### Step 1: Use AudioPathTester
1. Create empty GameObject: "AudioTester"
2. Add component: `AudioPathTester`
3. Set Test Path: `conversation-audio/female-participant/Group1`
4. Press Play
5. Check Console - does it show SA clips?

### Step 2: Check Console Output
Look for this in ConversationManager logs:
```
[ConversationManager] Breakdown - I: 10, SA: 0, Other: 0
```

If SA count is 0, there's a loading issue.

### Step 3: Verify Files Exist
Navigate to: `Assets/Resources/conversation-audio/female-participant/Group1/`
- Do you see SA_1.mp3, SA_4.mp3, etc.?
- Are they in the SAME folder as I_10.mp3, I_11.mp3?

## Common Causes

### Cause 1: Files in Wrong Folder
**Problem:** SA files might be in a different subfolder

**Check:**
```
Assets/Resources/conversation-audio/
├── female-participant/
│   └── Group1/
│       ├── I_10.mp3  ← Here
│       └── SA_1.mp3  ← Should be here too
```

**NOT:**
```
Assets/Resources/conversation-audio/
├── female-participant/
│   ├── Group1/
│   │   └── I_10.mp3
│   └── SA/  ← Wrong! SA files in separate folder
│       └── SA_1.mp3
```

### Cause 2: File Import Issues
**Problem:** SA files might not be imported as AudioClips

**Fix:**
1. Select all SA_*.mp3 files in Unity
2. Inspector → Audio Importer
3. Check "Preload Audio Data"
4. Click "Apply"
5. Right-click → Reimport

### Cause 3: File Name Issues
**Problem:** Files might have hidden characters or wrong extensions

**Check:**
- File names should be exactly: `SA_1.mp3`, `SA_4.mp3`, etc.
- No spaces: `SA _1.mp3` ✗
- No extra extensions: `SA_1.mp3.mp3` ✗
- Case sensitive: `sa_1.mp3` might not match `SA_1`

### Cause 4: Unity Cache Issue
**Problem:** Unity's asset database might be corrupted

**Fix:**
1. Close Unity
2. Delete `Library` folder in project root
3. Reopen Unity (will reimport everything)

### Cause 5: Resources Folder Structure
**Problem:** Files might not be in a Resources folder

**Verify:**
- Path MUST include "Resources" folder
- Correct: `Assets/Resources/conversation-audio/...`
- Wrong: `Assets/Audio/conversation-audio/...`

## Testing Individual Files

Add this to ConversationManager.Start() temporarily:

```csharp
void Start()
{
    // Test loading SA_1 directly
    AudioClip testSA = Resources.Load<AudioClip>("conversation-audio/female-participant/Group1/SA_1");
    if (testSA != null)
    {
        Debug.Log($"✓ SA_1 loaded directly: {testSA.name}");
    }
    else
    {
        Debug.LogError("✗ SA_1 failed to load directly!");
    }
    
    // Test loading I_10 directly
    AudioClip testI = Resources.Load<AudioClip>("conversation-audio/female-participant/Group1/I_10");
    if (testI != null)
    {
        Debug.Log($"✓ I_10 loaded directly: {testI.name}");
    }
    else
    {
        Debug.LogError("✗ I_10 failed to load directly!");
    }
}
```

## Expected Console Output (Good)

```
[ConversationManager] === LISTING ALL LOADED CLIPS ===
[ConversationManager] Raw clip name: 'I_10'
[ConversationManager] ✓ Cached Interlocutor audio: I_10
[ConversationManager] Raw clip name: 'I_11'
[ConversationManager] ✓ Cached Interlocutor audio: I_11
[ConversationManager] Raw clip name: 'SA_1'
[ConversationManager] ✓ Cached Self-Avatar audio: SA_1
[ConversationManager] Raw clip name: 'SA_4'
[ConversationManager] ✓ Cached Self-Avatar audio: SA_4
[ConversationManager] === SUMMARY ===
[ConversationManager] Breakdown - I: 10, SA: 10, Other: 0
```

## Bad Console Output (Problem)

```
[ConversationManager] === LISTING ALL LOADED CLIPS ===
[ConversationManager] Raw clip name: 'I_10'
[ConversationManager] ✓ Cached Interlocutor audio: I_10
[ConversationManager] Raw clip name: 'I_11'
[ConversationManager] ✓ Cached Interlocutor audio: I_11
[ConversationManager] === SUMMARY ===
[ConversationManager] Breakdown - I: 10, SA: 0, Other: 0
⚠ WARNING: No SA clips loaded but 10 I clips found!
```

## Manual Verification

1. **In Unity Project Window:**
   - Navigate to: `Assets/Resources/conversation-audio/female-participant/Group1/`
   - Count SA files: Should see SA_1, SA_4, SA_7, etc.
   - Select one SA file
   - Inspector should show: "Audio Clip" with waveform

2. **In File Explorer/Finder:**
   - Navigate to project folder
   - Go to: `Assets/Resources/conversation-audio/female-participant/Group1/`
   - Verify SA_*.mp3 files exist
   - Check file sizes (should be > 0 KB)

## Nuclear Option: Reimport All Audio

If nothing works:

1. Select `Assets/Resources/conversation-audio/` folder
2. Right-click → Reimport
3. Wait for Unity to finish
4. Check Console for import errors
5. Try running again

## Still Not Working?

Share these details:
1. Console output showing "Raw clip name" lines
2. Screenshot of Group1 folder in Unity Project window
3. Screenshot of Group1 folder in File Explorer
4. Result of AudioPathTester
5. Do I files load? (yes/no)
6. Do SA files load? (yes/no)
