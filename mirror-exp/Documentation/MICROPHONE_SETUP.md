# Microphone Setup for Player Speech

## How It Works

When it's the Player's turn (Speaker = "P" in CSV), the system:
1. Starts recording from the microphone
2. Feeds the audio to OVRLipSync for lip animation
3. **Mutes the AudioSource** so you don't hear yourself
4. Records for the duration specified in the CSV
5. Stops recording and restores audio settings

## Key Features

### No Audio Loopback
The microphone audio is **muted** during recording:
- You won't hear yourself speaking
- Lip sync still works (OVRLipSync reads from the AudioSource)
- Other players/avatars can still be heard

### Automatic Duration
Recording duration is calculated from the text length:
```csharp
duration = Mathf.Min(5f, line.Word.Length * 0.2f)
```
- Maximum: 5 seconds
- Or: 0.2 seconds per character in the text
- Whichever is shorter

## CSV Format for Player Lines

```csv
"Participant","Group","Line","Speaker","Text","AudioFile"
"P01","Group1",3,"P","Right now, local politics is becoming the main headline.",NA
"P01","Group1",5,"P","And they both promised to fix the roads before next winter.",NA
```

- **Speaker**: "P" (Player)
- **Text**: What the player should say (shown on screen)
- **AudioFile**: "NA" (no pre-recorded audio needed)

## Console Output

When microphone recording starts:
```
[Microphone] Starting recording from: Built-in Microphone
[Microphone] Recording for 3.5 seconds (muted)
[Microphone] Recording ended
```

## Troubleshooting

### Issue 1: Still Hearing Yourself
**Possible Causes:**
- Another AudioSource is playing the microphone
- VR headset has passthrough audio enabled
- System audio monitoring is on

**Fix:**
- Check only one AudioSource is assigned to AudioAvatar
- Disable system audio monitoring in OS settings
- Check VR audio settings

### Issue 2: No Lip Sync During Recording
**Possible Causes:**
- OVRLipSync not properly configured
- AudioSource not assigned to OVRLipSync
- Microphone not detected

**Fix:**
1. Check OVRLipSyncContext component on avatar
2. Verify AudioSource is assigned
3. Check Console for microphone detection:
   ```
   No microphone detected or AudioSource is null!
   ```

### Issue 3: Recording Too Short/Long
**Adjust Duration:**

In ConversationManager.cs, modify the calculation:
```csharp
// Current: Max 5 seconds, or 0.2s per character
yield return new WaitForSeconds(Mathf.Min(5f, line.Word.Length * 0.2f));

// Longer: Max 10 seconds, or 0.3s per character
yield return new WaitForSeconds(Mathf.Min(10f, line.Word.Length * 0.3f));

// Fixed duration: Always 5 seconds
yield return new WaitForSeconds(5f);
```

### Issue 4: Microphone Not Detected
**Check:**
1. Microphone is connected and enabled
2. Unity has microphone permissions
3. Check available devices:
   ```csharp
   foreach (var device in Microphone.devices)
   {
       Debug.Log($"Microphone: {device}");
   }
   ```

## Advanced Configuration

### Change Microphone Device
By default, uses first microphone (`Microphone.devices[0]`).

To use a specific microphone:
```csharp
string micName = "Oculus Virtual Audio Device"; // VR headset mic
// or
string micName = Microphone.devices[1]; // Second microphone
```

### Change Sample Rate
Default: 44100 Hz

For lower quality/bandwidth:
```csharp
int sampleRate = 22050; // Lower quality, less data
```

For higher quality:
```csharp
int sampleRate = 48000; // Higher quality
```

### Change Recording Length
Default: 5 seconds max

To allow longer recordings:
```csharp
AudioClip micClip = Microphone.Start(micName, true, 10, sampleRate); // 10 seconds
```

## OVRLipSync Configuration

Ensure your avatar has:
1. **OVRLipSyncContext** component
2. **AudioSource** assigned to it
3. **Audio Loopback** disabled (handled by code)

### Inspector Settings:
```
OVRLipSyncContext
├─ Audio Source: [AudioAvatar]
├─ Audio Loopback: ☐ (unchecked)
├─ Provider: Enhanced
└─ Enable Visemes: ☑
```

## Testing Microphone

### Quick Test:
1. Enable Debug Mode
2. Set Debug Avatar to any participant
3. Press Play
4. Wait for a "P" line
5. Speak into microphone
6. Check Console for "[Microphone]" messages
7. Verify you DON'T hear yourself
8. Verify avatar lips move

### Expected Behavior:
- ✅ Text appears on screen
- ✅ Microphone starts recording
- ✅ Avatar lips move with your speech
- ✅ You DON'T hear your own voice
- ✅ Recording stops after duration
- ✅ Conversation continues

### Not Working?
Check Console for:
```
No microphone detected or AudioSource is null!
```

This means:
- No microphone connected, OR
- AudioAvatar is not assigned

## VR-Specific Notes

### Quest/Meta Headsets:
- Built-in microphone is usually `Microphone.devices[0]`
- Check audio permissions in Quest settings
- Ensure app has microphone permission

### PC VR:
- May have multiple microphones (headset + desktop)
- Check which device is being used in Console
- Can specify device by name if needed

## Performance Tips

1. **Lower Sample Rate**: Use 22050 Hz instead of 44100 Hz
2. **Shorter Clips**: Reduce max recording time
3. **Disable Loopback**: Already done in code
4. **Optimize Lip Sync**: Use "Enhanced" provider in OVRLipSync

## Security/Privacy

- Microphone audio is NOT saved to disk
- Audio is only used for real-time lip sync
- Recording stops when player turn ends
- No audio is transmitted or stored

## Summary

The microphone system:
- ✅ Records your voice during "P" lines
- ✅ Drives lip sync animation
- ✅ Mutes playback (no echo)
- ✅ Automatically stops after duration
- ✅ Restores audio settings for next line
