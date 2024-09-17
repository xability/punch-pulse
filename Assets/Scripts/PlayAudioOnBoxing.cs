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
    private bool isCheering = false; // Flag to check if cheering is playing
    public float noCollisionTimeout = 5f; // Time in seconds before stopping the cheering sound

    private float timeSinceLastCollision = 0f; // Timer to track the last collision time

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        UpdateScoreText(); // Initialize score display
    }

    // Update is called once per frame
    void Update()
    {
        // Increment timer each frame
        timeSinceLastCollision += Time.deltaTime;

        // Stop the cheering sound if no collision happens for the timeout duration
        if (timeSinceLastCollision >= noCollisionTimeout && isCheering)
        {
            StopCheerSound();
        }
    }

    // OnTriggerEnter
    void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag(targetTag))
        {
            hasPlayed = true; // Set the flag to true to prevent re-triggering
            // Play the audio clip for punching
            PlaySound(other);

            // Increment the score
            score++;
            UpdateScoreText();
           
            
            // Reset the collision timer
            timeSinceLastCollision = 0f;

            // Play cheering sound if not already playing
            if (!isCheering)
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
            ApplyHapticFeedbackBasedOnVelocity(v);
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
        cheeringSource.Play(); // Start playing the cheering sound loop
    }

    void StopCheerSound()
    {
        isCheering = false;
        cheeringSource.Stop(); // Stop the cheering sound when no collisions happen for 5 seconds
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

    void ApplyHapticFeedbackBasedOnVelocity(float velocity)
    {
        // Normalize the velocity to a range between 0 and 1
        float intensity = velocity / maxVelocity;

        // Clamps the given value between the given minimum float and maximum float values.
        intensity = Mathf.Clamp(intensity, 0.25f, 0.75f);

        // Apply the intensity to the haptic feedback
        SendHapticImpulse(leftController, intensity, 0.2f);
        SendHapticImpulse(rightController, intensity, 0.2f);
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

}

