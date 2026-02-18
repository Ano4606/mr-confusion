# Adjusting Player Speaking Time

## Quick Setup

In the Unity Inspector, select your ConversationManager GameObject and adjust:

```
┌─────────────────────────────────────────────┐
│ Microphone Settings                         │
├─────────────────────────────────────────────┤
│ Player Speaking Time: 10                    │
│ Use Text Based Duration: ☐                  │
└─────────────────────────────────────────────┘
```

## Settings Explained

### Player Speaking Time
**Default: 10 seconds**

This is how long the microphone will record when it's the player's turn.

- **5 seconds**: Quick responses
- **10 seconds**: Normal conversation (recommended)
- **15 seconds**: Longer, detailed responses
- **20 seconds**: Very long responses

### Use Text Based Duration
**Default: Unchecked (☐)**

When **unchecked** (recommended):
- Uses fixed duration from "Player Speaking Time"
- Every player turn gets the same amount of time
- Easier to predict and consistent

When **checked** (☑):
- Calculates time based on text length (0.2s per character)
- Shorter text = less time, longer text = more time
- Maximum time is still limited by "Player Speaking Time"

## Examples

### Example 1: Fixed 10 Seconds (Recommended)
```
Player Speaking Time: 10
Use Text Based Duration: ☐
```
Result: Every player turn gets exactly 10 seconds to speak.

### Example 2: Fixed 15 Seconds
```
Player Speaking Time: 15
Use Text Based Duration: ☐
```
Result: Every player turn gets exactly 15 seconds to speak.

### Example 3: Text-Based (Variable)
```
Player Speaking Time: 10
Use Text Based Duration: ☑
```
Result:
- Short text (20 chars): 4 seconds (20 × 0.2)
- Medium text (50 chars): 10 seconds (50 × 0.2, capped at max)
- Long text (100 chars): 10 seconds (would be 20s, but capped at max)

## Recommended Settings

### For Quick Testing
```
Player Speaking Time: 5
Use Text Based Duration: ☐
```
Fast testing, short responses.

### For Normal Conversation
```
Player Speaking Time: 10
Use Text Based Duration: ☐
```
Good balance, enough time for most responses.

### For Detailed Responses
```
Player Speaking Time: 15
Use Text Based Duration: ☐
```
More time for complex answers.

### For Very Long Responses
```
Player Speaking Time: 20
Use Text Based Duration: ☐
```
Maximum time for detailed explanations.

## Console Output

When a player turn starts, you'll see:
```
[Microphone] Starting recording from: Built-in Microphone
[Microphone] Recording for 10.0 seconds (muted)
[Microphone] Recording ended
```

The number shows how long the recording lasted.

## Tips

1. **Start with 10 seconds** - Good default for most conversations
2. **Increase if needed** - If you feel rushed, increase to 15 or 20
3. **Don't go too high** - Very long times can make the conversation feel slow
4. **Test different values** - Find what feels natural for your content
5. **Consider your CSV text** - Longer prompts might need more time

## Changing During Development

You can change these values at any time:
1. Stop the game if running
2. Select ConversationManager
3. Change "Player Speaking Time"
4. Press Play again

No code changes needed!

## Advanced: Per-Line Duration

If you need different durations for different lines, you could:

1. **Use Text-Based Duration** - Longer text = more time automatically
2. **Modify CSV** - Add a duration column (requires code changes)
3. **Use Multiple Scenes** - Different scenes with different settings

## Troubleshooting

### Issue: Time Too Short
**Solution:** Increase "Player Speaking Time" to 15 or 20 seconds

### Issue: Time Too Long
**Solution:** Decrease "Player Speaking Time" to 5 or 7 seconds

### Issue: Inconsistent Times
**Check:** Is "Use Text Based Duration" checked?
- If yes: Time varies based on text length
- If no: Time should be consistent

### Issue: Recording Cuts Off
**Possible Causes:**
- Player Speaking Time is too short
- Microphone buffer is too small (handled automatically now)

**Solution:** Increase Player Speaking Time

## Technical Details

### How It Works

```csharp
if (useTextBasedDuration)
{
    // Variable: Based on text length
    duration = Min(playerSpeakingTime, textLength × 0.2)
}
else
{
    // Fixed: Always the same
    duration = playerSpeakingTime
}
```

### Microphone Buffer

The system automatically creates a microphone buffer large enough for your recording:
```csharp
int bufferLength = CeilToInt(duration) + 1
```

So if you set 10 seconds, it creates an 11-second buffer (with 1 second safety margin).

## Summary

- **Default: 10 seconds** - Good for most conversations
- **Adjust in Inspector** - No code changes needed
- **Fixed duration recommended** - More predictable
- **Test and adjust** - Find what feels natural for your content
