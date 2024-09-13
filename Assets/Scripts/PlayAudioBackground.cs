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
            audioSource.Play(); // Start playing the music
        }
    }
}