using UnityEngine;

/// <summary>
/// Quick test script to verify what audio files Unity can load
/// Attach to any GameObject and check Console
/// </summary>
public class AudioPathTester : MonoBehaviour
{
    [Header("Test Settings")]
    public string testPath = "conversation-audio/female-participant/Group1";
    
    [ContextMenu("Test Load Audio")]
    void Start()
    {
        TestLoadAudio();
    }

    public void TestLoadAudio()
    {
        Debug.Log($"=== TESTING AUDIO LOAD FROM: {testPath} ===");
        
        AudioClip[] clips = Resources.LoadAll<AudioClip>(testPath);
        
        Debug.Log($"Total clips found: {clips.Length}");
        
        int iCount = 0;
        int saCount = 0;
        
        foreach (var clip in clips)
        {
            if (clip.name.StartsWith("I_"))
            {
                iCount++;
                Debug.Log($"✓ I clip: {clip.name} (Length: {clip.length}s, Channels: {clip.channels})");
            }
            else if (clip.name.StartsWith("SA_"))
            {
                saCount++;
                Debug.Log($"✓ SA clip: {clip.name} (Length: {clip.length}s, Channels: {clip.channels})");
            }
            else
            {
                Debug.Log($"? Other clip: {clip.name}");
            }
        }
        
        Debug.Log($"=== SUMMARY ===");
        Debug.Log($"I clips: {iCount}");
        Debug.Log($"SA clips: {saCount}");
        Debug.Log($"Total: {clips.Length}");
        
        if (saCount == 0 && iCount > 0)
        {
            Debug.LogWarning("⚠ SA clips not loading! Checking possible issues...");
            
            // Try loading SA_1 directly
            AudioClip testClip = Resources.Load<AudioClip>($"{testPath}/SA_1");
            if (testClip != null)
            {
                Debug.Log($"✓ Direct load of SA_1 worked: {testClip.name}");
            }
            else
            {
                Debug.LogError("✗ Direct load of SA_1 failed!");
            }
        }
    }
}
