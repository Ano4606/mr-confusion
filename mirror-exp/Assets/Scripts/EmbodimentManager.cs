using UnityEngine;
using System;
using System.Collections;

public class EmbodimentManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("A GameObject with an AudioSource and the instruction AudioClip assigned")]
    public AudioSource audioSource;
    public AudioClip instructionClip;

    [Header("Lipsync / Avatar (set via BindToAvatar)")]
    [Tooltip("Populated automatically from the chosen avatar's AvatarBindings")]
    public OVRLipSyncContext lipSyncContext;
    public AudioSource avatarVoiceSource;

    public event Action OnEmbodimentFinished;

    /// <summary>
    /// Called by ExperimentManager after the avatar is selected so we know
    /// which lipsync context and voice source to drive with the microphone.
    /// </summary>
    public void BindToAvatar(GameObject avatar)
    {
        if (avatar == null) return;

        AvatarBindings bindings = avatar.GetComponent<AvatarBindings>();
        if (bindings == null) return;

        lipSyncContext    = bindings.lipSync;
        avatarVoiceSource = bindings.voiceSource;
    }

    public void PlayInstruction()
    {
        if (audioSource == null || instructionClip == null)
        {
            Debug.LogWarning("EmbodimentManager: AudioSource or instructionClip missing.");
            OnEmbodimentFinished?.Invoke();
            return;
        }

        StartCoroutine(PlayInstructionRoutine());
    }

    private IEnumerator PlayInstructionRoutine()
    {
        // --- Start microphone lipsync on the avatar ---
        string micName = null;
        AudioClip micClip = null;

        if (avatarVoiceSource != null && lipSyncContext != null && Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            micClip = Microphone.Start(micName, true, 300, 44100);

            // Wait until the mic buffer has data
            while (Microphone.GetPosition(micName) <= 0)
                yield return null;

            // Route mic audio through the avatar voice source so OVRLipSync picks it up.
            // OVRLipSyncContext uses OnAudioFilterRead on its own AudioSource — do NOT mute,
            // as muting zeros the DSP buffer before the filter runs, killing lipsync.
            // audioLoopback = false makes PostprocessAudioSamples zero the output instead,
            // so the mic is processed for visemes but the player never hears themselves.
            avatarVoiceSource.clip        = micClip;
            avatarVoiceSource.loop        = true;
            avatarVoiceSource.mute        = false;
            avatarVoiceSource.Play();

            lipSyncContext.audioLoopback = false;
            lipSyncContext.enabled       = true;
        }
        else
        {
            Debug.LogWarning("EmbodimentManager: No mic or avatar bindings — lipsync skipped.");
        }

        // --- Play the instruction clip ---
        audioSource.clip = instructionClip;
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);

        // --- Stop mic lipsync ---
        if (micName != null)
        {
            Microphone.End(micName);
            avatarVoiceSource.Stop();
            avatarVoiceSource.clip   = null;
            avatarVoiceSource.loop   = false;
            avatarVoiceSource.mute   = false;
            avatarVoiceSource.volume = 1f;
            lipSyncContext.audioLoopback = false;
        }

        OnEmbodimentFinished?.Invoke();
    }
}
