using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using TMPro; // Required for TextMeshPro

public class PlayAudioOnBoxing : MonoBehaviour
{
    // punching
    public AudioClip clip;
    private AudioSource source;
    public string targetTag;

    // cheering
    public AudioClip cheeringClip;
    public AudioSource cheeringSource;

    // Score and haptic feedback additions
    public XRBaseController leftController;  // Assign via Inspector
    public XRBaseController rightController; // Assign via Inspector
    public InputActionReference hapticAction;
    public int score = 0;  // Variable to keep track of the score
    public TMP_Text scoreText; // Assign via Inspector, TextMeshPro text element to display the score

    public bool useVelocity = true;
    public float minVelocity = 0;
    public float maxVelocity = 2;

    public bool randomizePitch = true;
    public float minPitch = 0.6f;
    public float maxPitch = 1.2f;

    private bool hasPlayed = false; // New flag to track if the sound has been played
    private bool isCheering = false; // Flag to check if impact sound is playing
    private bool canPlaycheer = true; // Flag to control impact sound cooldown
    public float cheerCooldownTime = 2f; // Cooldown time in seconds for impact sound


    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        UpdateScoreText(); // Initialize score display

    }

    // OnTriggerEnter
    void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag(targetTag))
        {
            hasPlayed = true; // Set the flag to true to prevent re-triggering
            // Play the audio clip
            PlaySound(other);

            // Increment the score
            score++;
            UpdateScoreText();

            // Trigger haptic feedback on both controllers
            SendHapticImpulse(leftController, 0.5f, 0.2f);
            SendHapticImpulse(rightController, 0.5f, 0.2f);

            if (!isCheering && canPlaycheer)
            {
                PlayCheerSound();
            }

        }
    }

    void PlaySound(Collider other)
    {
        VelocityEstimator estimator = other.GetComponent<VelocityEstimator>();

        if (estimator && useVelocity)
        {
            float v = estimator.GetVelocityEstimate().magnitude;
            Debug.Log("Velocity :" + v);
            float volume;
            if (v < minVelocity)
            {
                source.pitch = minPitch;
                volume = 0.8f;
            }
            else
            {
                source.pitch = maxPitch;
                volume = 1.2f;
            }
            source.PlayOneShot(clip, volume);
        }
        else
        {
            source.PlayOneShot(clip);
        }
    }

    void PlayCheerSound()
    {
        isCheering = true;
        canPlaycheer = false; // Disable playing the impact sound until the cooldown finishes
        cheeringSource.PlayOneShot(cheeringClip);

        // Start coroutine to reset the impact sound and handle the cooldown
        StartCoroutine(CheerSoundCooldown());
    }

    // Coroutine to handle impact sound and cooldown
    IEnumerator CheerSoundCooldown()
    {
        yield return new WaitForSeconds(cheeringClip.length); // Wait till duration ends
        isCheering = false;  // Reset the cheer sound flag

        // Start cooldown timer
        yield return new WaitForSeconds(cheerCooldownTime);
        canPlaycheer = true; // Allow impact sound to play again
    }

    // OnTriggerExit
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            hasPlayed = false; // Reset the flag when the player exits the collision box
        }
    }
    void SendHapticImpulse(XRBaseController controller, float amplitude, float duration)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

}