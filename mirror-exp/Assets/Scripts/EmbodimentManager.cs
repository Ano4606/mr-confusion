using UnityEngine;
using System;
using System.Collections;

public class EmbodimentManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip instructionClip;

    public event Action OnEmbodimentFinished;

    public void PlayInstruction()
    {
        if (audioSource == null || instructionClip == null)
        {
            Debug.LogWarning("AudioSource or instructionClip missing in EmbodimentManager.");
            return;
        }

        StartCoroutine(PlayInstructionRoutine());
    }

    private IEnumerator PlayInstructionRoutine()
    {
        audioSource.clip = instructionClip;
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);

        OnEmbodimentFinished?.Invoke();
    }
}


// using UnityEngine;

// public class EmbodimentManager : MonoBehaviour
// {
//     [Header("Audio Settings")]
//     public AudioSource audioSource;
//     public AudioClip instructionClip;

//     [Header("Experiment Manager")]
//     public ExperimentManager experimentManager; // Assign in Inspector

//     private bool hasAudioFinished = false;

//     public void PlayInstruction()
//     {
//         if (audioSource != null && instructionClip != null)
//         {
//             audioSource.clip = instructionClip;
//             audioSource.Play();
//             hasAudioFinished = false; // Reset flag
//         }
//         else
//         {
//             Debug.LogWarning("AudioSource or AudioClip not assigned!");
//         }
//     }

//     void Update()
//     {
//         if (audioSource != null && !audioSource.isPlaying && !hasAudioFinished)
//         {
//             hasAudioFinished = true;

//             if (experimentManager != null)
//             {
//                 experimentManager.EndTrainingAndStartTrials(); // Launch the method directly
//             }
//             else
//             {
//                 Debug.LogWarning("ExperimentManager not assigned!");
//             }
//         }
//     }
// }
