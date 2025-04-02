using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using TMPro; // Required for TextMeshPro

public class PlayAudioOnBoxing : MonoBehaviour
{
    // Punching
    public AudioClip clip;
    private AudioSource source;
    public string targetTag;

    // Cheering
    public AudioClip cheeringClip;
    public AudioSource cheeringSource;

    // Score and haptic feedback
    public XRBaseController leftController;  // Assign via Inspector
    public XRBaseController rightController; // Assign via Inspector
    public InputActionReference hapticAction;

    public bool useVelocity = true;
    public float minVelocity = 0;
    public float maxVelocity = 2;

    public bool randomizePitch = true;
    public float minPitch = 0.6f;
    public float maxPitch = 1.2f;
    private Transform hittingGlove;
    private Vector3 hittingGloveVelocity;

    public bool hasPlayed = false; // Flag to track if the sound has been played
    private bool isCheering = false; // Flag to check if cheering is playing
    public float noCollisionTimeout = 5f; // Time in seconds before stopping the cheering sound
    public float cheerFadeOutDuration = 1.5f;

    private float timeSinceLastCollision = 0f; // Timer to track the last collision time
    private Coroutine cheeringCoroutine;
    public Camera playerCamera;
    public Transform leftControllerTransform;
    public Transform rightControllerTransform;
    public float minForwardDistance = 0.2f; // Minimum distance the controller needs to be in front of the player

    public Animator modelAnimator;
    // New variables for hit detection
    public string headColliderTag = "Head_Collider";
    public string bodyColliderTag = "Torso_Collider";
    public string component;

    private static int playerHeadPunchCount = 0;
    private static int playerBodyPunchCount = 0;

    public static int GetPlayerHeadPunchCount()
    {
        return playerHeadPunchCount;
    }

    public static void SetPlayerHeadPunchCount(int value)
    {
        playerHeadPunchCount = value;
    }

    public static int GetPlayerBodyPunchCount()
    {
        return playerBodyPunchCount;
    }

    public static void SetPlayerBodyPunchCount(int value)
    {
        playerBodyPunchCount = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (leftControllerTransform == null || rightControllerTransform == null)
            Debug.LogWarning("Controller transforms not assigned in PlayAudioOnBoxing script!");

        // Ensure the cheering source is not playing at start
        if (cheeringSource != null)
        {
            cheeringSource.Stop();
            cheeringSource.loop = true;
        }
    }
    private bool postTutorialFlag = false;
    // Update is called once per frame
    void Update()
    {
        
        if (isCheering)
        {
            // Increment timer each frame
            timeSinceLastCollision += Time.deltaTime;

            if (timeSinceLastCollision >= noCollisionTimeout)
            {
                StopCheerSound();
            }
        }

        if (TutorialManager.TutorialCompleted && !postTutorialFlag)
        {
            playerHeadPunchCount = 0;
            playerBodyPunchCount = 0;
            postTutorialFlag = true;
        }
    }

    protected virtual bool IsControllerInFront(Transform controllerTransform)
    {
        if (controllerTransform == null || playerCamera == null)
            return false;

        Vector3 playerForward = playerCamera.transform.forward;
        playerForward.y = 0; // Ignore vertical difference
        playerForward.Normalize();

        Vector3 controllerDirection = controllerTransform.position - playerCamera.transform.position;
        controllerDirection.y = 0; // Ignore vertical difference
        controllerDirection.Normalize();

        float dotProduct = Vector3.Dot(playerForward, controllerDirection);
        float forwardDistance = Vector3.Project(controllerTransform.position - playerCamera.transform.position, playerForward).magnitude;

        return dotProduct > 0 && forwardDistance >= minForwardDistance;
    }

    void TriggerBodyHitAnimation()
    {
        if (modelAnimator != null)
        {
            modelAnimator.SetTrigger("Hit_body");
        }
        else
        {
            Debug.LogWarning("Animator not assigned!");
        }
    }

    protected virtual void TriggerHeadHitAnimation()
    {
        if (modelAnimator != null)
        {
            modelAnimator.SetTrigger("Hit_Head");
        }
        else
        {
            Debug.LogWarning("Animator not assigned!");
        }
    }

    // OnTriggerEnter
    protected virtual void OnTriggerEnter(Collider other)
    {
        
        if (!hasPlayed && (other.CompareTag(headColliderTag) || other.CompareTag(bodyColliderTag)))
        {
            // Debug.Log("Punch hit! Component : " + component + " Tag : " + other.gameObject.tag);
            // Debug.Log("Gloves : " + other.CompareTag(targetTag) + " Head punch : " + other.CompareTag(headColliderTag) + " Body punch : " + other.CompareTag(bodyColliderTag));
            bool isLeftControllerInFront = IsControllerInFront(leftControllerTransform);
            bool isRightControllerInFront = IsControllerInFront(rightControllerTransform);

            hittingGlove = isLeftControllerInFront ? leftControllerTransform : rightControllerTransform;
            VelocityEstimator estimator = hittingGlove.GetComponent<VelocityEstimator>();
            hittingGloveVelocity = estimator.GetVelocityEstimate();
            float velocityMagnitude = hittingGloveVelocity.magnitude;
            

            if (isLeftControllerInFront || isRightControllerInFront)
            {
                hasPlayed = true;
                PlaySound(velocityMagnitude);

                // Determine which animation to play based on hit location
                if (other.CompareTag(headColliderTag))
                {
                    TriggerHeadHitAnimation();
                    ScoreManager.AddScore(2);
                    playerHeadPunchCount++; 
                }
                else
                {
                    TriggerBodyHitAnimation();
                    ScoreManager.AddScore(1);
                    playerBodyPunchCount++;
                }
                
            }
        }
    }


    protected virtual void PlaySound(float v)
    {
       
        if (useVelocity)
        {
            Debug.Log("Hitting glove velocity: " + v);
            ApplyHapticFeedbackBasedOnVelocity(v);

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
        if (cheeringSource == null || cheeringClip == null)
        {
            Debug.LogWarning("Cheering source or cheering clip is not assigned!");
            return;
        }

        // Stop any ongoing fading coroutine
        if (cheeringCoroutine != null)
        {
            StopCoroutine(cheeringCoroutine);
        }

        // Reset the volume and play the cheering sound
        cheeringSource.volume = 1f;
        if (!cheeringSource.isPlaying)
        {
            cheeringSource.clip = cheeringClip;
            cheeringSource.Play();
        }

        isCheering = true;
        timeSinceLastCollision = 0f;
    }

    void StopCheerSound()
    {
        if (isCheering)
        {
            cheeringCoroutine = StartCoroutine(FadeOutCheering());
        }
    }


    // OnTriggerExit
    void OnTriggerExit(Collider other)
    {
       
            hasPlayed = false; // Reset the flag when the player exits the collision box
        
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
        intensity = Mathf.Clamp(intensity, 0.4f, 0.75f);

        // Apply the intensity to the haptic feedback
        SendHapticImpulse(leftController, intensity, 0.1f);
        SendHapticImpulse(rightController, intensity, 0.1f);
    }


    private IEnumerator FadeOutCheering()
    {
        float startVolume = cheeringSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < cheerFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / cheerFadeOutDuration);
            cheeringSource.volume = newVolume;
            yield return null;
        }

        cheeringSource.Stop();
        cheeringSource.volume = startVolume; // Reset volume for next use
        isCheering = false;
    }
}
