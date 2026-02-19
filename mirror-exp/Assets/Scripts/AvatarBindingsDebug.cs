using UnityEngine;

/// <summary>
/// Debug what AvatarBindings is finding
/// Attach to avatar with AvatarBindings component
/// </summary>
public class AvatarBindingsDebug : MonoBehaviour
{
    public AvatarBindings bindings;

    void Start()
    {
        if (bindings == null)
            bindings = GetComponent<AvatarBindings>();

        if (bindings == null)
        {
            Debug.LogError($"[AvatarBindingsDebug] No AvatarBindings on {gameObject.name}!");
            return;
        }

        Debug.Log($"\n=== AVATAR BINDINGS DEBUG: {gameObject.name} ===");
        
        // Check what was found
        Debug.Log($"Animator: {(bindings.animator != null ? bindings.animator.gameObject.name : "NULL")}");
        Debug.Log($"Retargeter: {(bindings.retargeter != null ? bindings.retargeter.gameObject.name : "NULL")}");
        Debug.Log($"LipSync: {(bindings.lipSync != null ? bindings.lipSync.gameObject.name : "NULL")}");
        Debug.Log($"VoiceSource: {(bindings.voiceSource != null ? bindings.voiceSource.gameObject.name : "NULL")}");
        
        // Check if lipSync is properly configured
        if (bindings.lipSync != null)
        {
            Debug.Log($"\n--- LipSync Details ---");
            Debug.Log($"  GameObject: {bindings.lipSync.gameObject.name}");
            Debug.Log($"  Enabled: {bindings.lipSync.enabled}");
            Debug.Log($"  AudioSource: {(bindings.lipSync.audioSource != null ? bindings.lipSync.audioSource.gameObject.name : "NULL")}");
            Debug.Log($"  Audio Loopback: {bindings.lipSync.audioLoopback}");
            Debug.Log($"  Provider: {bindings.lipSync.provider}");
            
            // Check if it matches voiceSource
            if (bindings.lipSync.audioSource != bindings.voiceSource)
            {
                Debug.LogWarning($"  ⚠ LipSync AudioSource ({bindings.lipSync.audioSource?.gameObject.name}) != VoiceSource ({bindings.voiceSource?.gameObject.name})");
            }
            else
            {
                Debug.Log($"  ✓ LipSync AudioSource matches VoiceSource");
            }
            
            // Check for OVRLipSyncContextMorphTarget
            OVRLipSyncContextMorphTarget morphTarget = bindings.lipSync.GetComponent<OVRLipSyncContextMorphTarget>();
            if (morphTarget == null)
            {
                Debug.LogError($"  ✗ NO OVRLipSyncContextMorphTarget on same GameObject!");
            }
            else
            {
                Debug.Log($"  ✓ OVRLipSyncContextMorphTarget found");
                Debug.Log($"    - SkinnedMeshRenderer: {(morphTarget.skinnedMeshRenderer != null ? morphTarget.skinnedMeshRenderer.gameObject.name : "NULL")}");
                
                if (morphTarget.skinnedMeshRenderer != null)
                {
                    int blendShapeCount = morphTarget.skinnedMeshRenderer.sharedMesh.blendShapeCount;
                    Debug.Log($"    - Blend Shapes: {blendShapeCount}");
                    
                    if (blendShapeCount == 0)
                    {
                        Debug.LogError($"    ✗ NO BLEND SHAPES on mesh!");
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"✗ LipSync is NULL!");
            
            // Search for all OVRLipSyncContext in children
            Debug.Log($"\nSearching for OVRLipSyncContext in children...");
            OVRLipSyncContext[] allLipSyncs = GetComponentsInChildren<OVRLipSyncContext>(true);
            Debug.Log($"Found {allLipSyncs.Length} OVRLipSyncContext components:");
            
            foreach (var ls in allLipSyncs)
            {
                Debug.Log($"  - {ls.gameObject.name} (Active: {ls.gameObject.activeInHierarchy}, Enabled: {ls.enabled})");
            }
        }
        
        // Check voiceSource
        if (bindings.voiceSource != null)
        {
            Debug.Log($"\n--- VoiceSource Details ---");
            Debug.Log($"  GameObject: {bindings.voiceSource.gameObject.name}");
            Debug.Log($"  Volume: {bindings.voiceSource.volume}");
            Debug.Log($"  Mute: {bindings.voiceSource.mute}");
            Debug.Log($"  Clip: {(bindings.voiceSource.clip != null ? bindings.voiceSource.clip.name : "None")}");
        }
        else
        {
            Debug.LogError($"✗ VoiceSource is NULL!");
        }
        
        Debug.Log($"=== END DEBUG ===\n");
    }

    [ContextMenu("Run Debug Now")]
    public void RunDebugNow()
    {
        Start();
    }
}
