using UnityEngine;
using System.Linq;

/// <summary>
/// Helper script to debug audio issues in the scene
/// Attach to any GameObject and it will log all AudioSource information
/// </summary>
public class AudioDebugHelper : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool logOnStart = true;
    public bool logAudioSources = true;
    public bool logAudioListeners = true;
    public bool checkResourcesAudio = true;
    
    [Header("Manual Check")]
    public AudioSource testAudioSource;
    
    void Start()
    {
        if (logOnStart)
        {
            DebugAudioSetup();
        }
    }

    [ContextMenu("Debug Audio Setup")]
    public void DebugAudioSetup()
    {
        Debug.Log("=== AUDIO DEBUG START ===");
        
        if (logAudioListeners)
        {
            CheckAudioListeners();
        }
        
        if (logAudioSources)
        {
            CheckAudioSources();
        }
        
        if (checkResourcesAudio)
        {
            CheckResourcesAudio();
        }
        
        Debug.Log("=== AUDIO DEBUG END ===");
    }

    void CheckAudioListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        Debug.Log($"[AudioDebug] Found {listeners.Length} AudioListener(s) in scene");
        
        if (listeners.Length == 0)
        {
            Debug.LogError("[AudioDebug] ✗ NO AUDIO LISTENER FOUND! Add one to Main Camera or player.");
        }
        else if (listeners.Length > 1)
        {
            Debug.LogWarning($"[AudioDebug] ⚠ Multiple AudioListeners found ({listeners.Length})! Only one should be active.");
            foreach (var listener in listeners)
            {
                Debug.Log($"  - {listener.gameObject.name} (Enabled: {listener.enabled})");
            }
        }
        else
        {
            Debug.Log($"[AudioDebug] ✓ AudioListener on: {listeners[0].gameObject.name}");
        }
    }

    void CheckAudioSources()
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        Debug.Log($"[AudioDebug] Found {sources.Length} AudioSource(s) in scene");
        
        foreach (var source in sources)
        {
            string status = source.enabled ? "✓" : "✗";
            Debug.Log($"[AudioDebug] {status} AudioSource on: {source.gameObject.name}");
            Debug.Log($"    - Enabled: {source.enabled}");
            Debug.Log($"    - Volume: {source.volume}");
            Debug.Log($"    - Mute: {source.mute}");
            Debug.Log($"    - Spatial Blend: {source.spatialBlend} (0=2D, 1=3D)");
            Debug.Log($"    - Clip: {(source.clip != null ? source.clip.name : "None")}");
            Debug.Log($"    - Playing: {source.isPlaying}");
            
            if (source.volume == 0)
            {
                Debug.LogWarning($"[AudioDebug] ⚠ Volume is 0 on {source.gameObject.name}!");
            }
            
            if (source.mute)
            {
                Debug.LogWarning($"[AudioDebug] ⚠ AudioSource is muted on {source.gameObject.name}!");
            }
            
            if (!source.enabled)
            {
                Debug.LogWarning($"[AudioDebug] ⚠ AudioSource is disabled on {source.gameObject.name}!");
            }
        }
    }

    void CheckResourcesAudio()
    {
        Debug.Log("[AudioDebug] Checking Resources/conversation-audio/...");
        
        // Check female-participant folders
        for (int i = 1; i <= 3; i++)
        {
            string path = $"conversation-audio/female-participant/Group{i}";
            AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
            Debug.Log($"[AudioDebug] {path}: {clips.Length} clips");
            if (clips.Length > 0)
            {
                Debug.Log($"    Sample clips: {string.Join(", ", System.Array.ConvertAll(clips, c => c.name).Take(3))}");
            }
        }
        
        // Check male-participant folders
        for (int i = 1; i <= 3; i++)
        {
            string path = $"conversation-audio/male-participant/Group{i}";
            AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
            Debug.Log($"[AudioDebug] {path}: {clips.Length} clips");
            if (clips.Length > 0)
            {
                Debug.Log($"    Sample clips: {string.Join(", ", System.Array.ConvertAll(clips, c => c.name).Take(3))}");
            }
        }
    }

    [ContextMenu("Test Play Audio")]
    public void TestPlayAudio()
    {
        if (testAudioSource == null)
        {
            Debug.LogError("[AudioDebug] No test AudioSource assigned!");
            return;
        }
        
        if (testAudioSource.clip == null)
        {
            Debug.LogError("[AudioDebug] Test AudioSource has no clip assigned!");
            return;
        }
        
        Debug.Log($"[AudioDebug] Testing audio playback on {testAudioSource.gameObject.name}");
        testAudioSource.Play();
        
        if (testAudioSource.isPlaying)
        {
            Debug.Log("[AudioDebug] ✓ Audio is playing!");
        }
        else
        {
            Debug.LogError("[AudioDebug] ✗ Audio failed to play!");
        }
    }
}
