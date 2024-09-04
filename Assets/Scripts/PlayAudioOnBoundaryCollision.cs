using UnityEngine;

public class PlayAudioOnBoundaryCollision : MonoBehaviour
{
    public AudioSource audioSource; // The AudioSource that will play the sound
    public AudioClip exitSound; // The sound clip to play when the player leaves the collision box
    private bool isPlayerInBox = true; // Tracks whether the player is in the box

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.loop = true; // Ensure the sound loops
            audioSource.clip = exitSound;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player has left the collision box
        if (other.CompareTag("Player") && isPlayerInBox)
        {
            isPlayerInBox = false;
            PlayExitSound();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player has re-entered the collision box
        if (other.CompareTag("Player") && !isPlayerInBox)
        {
            isPlayerInBox = true;
            StopExitSound();
        }
    }

    void PlayExitSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void StopExitSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
