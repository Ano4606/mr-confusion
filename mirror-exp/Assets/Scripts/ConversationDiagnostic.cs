using UnityEngine;

public class ConversationDiagnostic : MonoBehaviour
{
    public ConversationManager conversationManager;

    void Start()
    {
        if (conversationManager == null)
        {
            conversationManager = FindObjectOfType<ConversationManager>();
        }

        if (conversationManager == null)
        {
            Debug.LogError("[Diagnostic] ConversationManager not found!");
            return;
        }

        Debug.Log("========== CONVERSATION MANAGER DIAGNOSTIC ==========");
        
        // Check AudioSources
        Debug.Log("--- AUDIO SOURCES ---");
        if (conversationManager.AudioAvatar != null)
        {
            Debug.Log($"✓ AudioAvatar: {conversationManager.AudioAvatar.gameObject.name}");
            Debug.Log($"  - Volume: {conversationManager.AudioAvatar.volume}");
            Debug.Log($"  - Mute: {conversationManager.AudioAvatar.mute}");
            Debug.Log($"  - Enabled: {conversationManager.AudioAvatar.enabled}");
            Debug.Log($"  - Spatial Blend: {conversationManager.AudioAvatar.spatialBlend}");
        }
        else
        {
            Debug.LogError("✗ AudioAvatar is NULL in Inspector!");
        }

        if (conversationManager.AudioInterlocutor != null)
        {
            Debug.Log($"✓ AudioInterlocutor: {conversationManager.AudioInterlocutor.gameObject.name}");
            Debug.Log($"  - Volume: {conversationManager.AudioInterlocutor.volume}");
            Debug.Log($"  - Mute: {conversationManager.AudioInterlocutor.mute}");
            Debug.Log($"  - Enabled: {conversationManager.AudioInterlocutor.enabled}");
            Debug.Log($"  - Spatial Blend: {conversationManager.AudioInterlocutor.spatialBlend}");
        }
        else
        {
            Debug.LogError("✗ AudioInterlocutor is NULL in Inspector!");
        }

        // Check Lip Sync Contexts
        Debug.Log("--- LIP SYNC CONTEXTS ---");
        if (conversationManager.avatarLipsync != null)
        {
            Debug.Log($"✓ avatarLipsync: {conversationManager.avatarLipsync.gameObject.name}");
            Debug.Log($"  - Enabled: {conversationManager.avatarLipsync.enabled}");
            Debug.Log($"  - AudioSource: {(conversationManager.avatarLipsync.audioSource != null ? conversationManager.avatarLipsync.audioSource.gameObject.name : "NULL")}");
            Debug.Log($"  - Audio Loopback: {conversationManager.avatarLipsync.audioLoopback}");
        }
        else
        {
            Debug.LogError("✗ avatarLipsync is NULL in Inspector!");
        }

        if (conversationManager.interlocutorLipsync != null)
        {
            Debug.Log($"✓ interlocutorLipsync: {conversationManager.interlocutorLipsync.gameObject.name}");
            Debug.Log($"  - Enabled: {conversationManager.interlocutorLipsync.enabled}");
            Debug.Log($"  - AudioSource: {(conversationManager.interlocutorLipsync.audioSource != null ? conversationManager.interlocutorLipsync.audioSource.gameObject.name : "NULL")}");
            Debug.Log($"  - Audio Loopback: {conversationManager.interlocutorLipsync.audioLoopback}");
        }
        else
        {
            Debug.LogError("✗ interlocutorLipsync is NULL in Inspector!");
        }

        // Check Animator and Retargeter
        Debug.Log("--- AVATAR CONTROL ---");
        if (conversationManager.selfAvatarAnimator != null)
        {
            Debug.Log($"✓ selfAvatarAnimator: {conversationManager.selfAvatarAnimator.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ selfAvatarAnimator is NULL");
        }

        if (conversationManager.retargeter != null)
        {
            Debug.Log($"✓ retargeter: {conversationManager.retargeter.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ retargeter is NULL");
        }

        Debug.Log("====================================================");
    }
}
