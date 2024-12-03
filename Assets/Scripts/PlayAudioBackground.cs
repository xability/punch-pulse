using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayBackgroundAudio : MonoBehaviour
{
    public AudioSource audioSource; // The AudioSource component
    public AudioClip backgroundMusic; // The background audio clip

    void Start()
    {
        // Ensure the AudioSource is attached
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Assign the audio clip and play it
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true; // Loop the background music

            // Check TutorialCompleted status
            if (TutorialManager.TutorialCompleted)
            {
                audioSource.Play();
            }
        }

    }

    void Update()
    {
        // Continuously check if the tutorial status has changed
        if (TutorialManager.TutorialCompleted && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!TutorialManager.TutorialCompleted && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

}