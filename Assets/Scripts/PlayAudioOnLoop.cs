using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnLoop : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource audioSource;

    public bool randomizePitch = true;
    public float minPitch = 0.6f;
    public float maxPitch = 1.2f;
    public float interval = 5.0f;   // Interval time in seconds

    private float timeSinceLastPlay;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        timeSinceLastPlay = 0f;
    }

    void Update()
    {
        timeSinceLastPlay += Time.deltaTime;

        if (timeSinceLastPlay >= interval)
        {
            PlayAudioClip();
            timeSinceLastPlay = 0f;
        }
    }

    void PlayAudioClip()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }


}
