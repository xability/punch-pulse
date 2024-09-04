using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using TMPro; // Required for TextMeshPro

public class PlayAudioOnBoxing : MonoBehaviour
{
    public AudioClip clip;
    private AudioSource source;
    public string targetTag;

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
        }
    }

    void PlaySound(Collider other)
    {
        VelocityEstimator estimator = other.GetComponent<VelocityEstimator>();

        if (estimator && useVelocity)
        {
            float v = estimator.GetVelocityEstimate().magnitude;
            Debug.Log(v);
            float volume = Mathf.InverseLerp(minVelocity, maxVelocity, v);
            if (v < minVelocity)
            {
                source.pitch = minPitch;
            }
            else if (v > maxVelocity)
            {
                source.pitch = maxPitch;
            }
            else
            {
                source.pitch = Random.Range(minPitch, maxPitch);
            }
            source.PlayOneShot(clip, volume);
        }
        else
        {
            if (randomizePitch)
            {
                source.pitch = Random.Range(minPitch, maxPitch);
            }

            source.PlayOneShot(clip);
        }
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