using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DirectionHelper : MonoBehaviour
{
    public GameObject enemy;
    public float stepDistance; // Adjust this value based on your game's scale
    public InputActionReference leftTriggerAction;
    public Camera playerCamera; // The player's camera (assign the main VR camera)


    // Add Audio Source and Audio Clips
    public AudioSource audioSource;
    public AudioClip[] clockDirectionClips; // 12 audio clips for each hour direction
    public AudioClip[] stepClips; // Audio clips representing the number of steps away (e.g. 1 step, 2 steps, etc.)

    private bool isAudioPlaying = false;

    // Static variable to count trigger presses
    private static int triggerPressCount = 0;

    // Public static method to get the count
    public static int GetLeftTriggerPressCount()
    {
        return triggerPressCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;  // Assign the main camera if none provided
        }

        // Subscribe to trigger action
        leftTriggerAction.action.performed += OnLeftTriggerPressed;
        // Debug.Log("Left Trigger Action Performed");
    }

    private bool postTutorialFlag = false;
    void Update()
    {
        // Continuously check if the tutorial status has changed
        if (TutorialManager.TutorialCompleted && !postTutorialFlag)
        {
            triggerPressCount = 0;
            postTutorialFlag = true;
        }
    }


    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (leftTriggerAction != null && leftTriggerAction.action != null)
        {
            leftTriggerAction.action.performed -= OnLeftTriggerPressed;
        }
        else
        {
            Debug.Log("Left Trigger Action was already null on destroy");
        }
    }

    // Called when the right trigger is pressed
    private void OnLeftTriggerPressed(InputAction.CallbackContext context)
    {
        if (isAudioPlaying)
        {
            // Debug.Log("Audio is currently playing, skipping this trigger.");
            return;
        }
        triggerPressCount++;

        Vector3 playerPosition = playerCamera.transform.position;
        Vector3 enemyPosition = enemy.transform.position;

        // Calculate direction to enemy
        Vector3 directionToEnemy = enemyPosition - playerPosition;
        directionToEnemy.y = 0; // Project onto horizontal plane
        directionToEnemy.Normalize();

        // Use the negative of the camera's forward vector as the player's forward direction
        Vector3 playerForward = -playerCamera.transform.forward;
        playerForward.y = 0; // Project onto horizontal plane
        playerForward.Normalize();

        // Calculate the angle
        float angle = Vector3.SignedAngle(playerForward, directionToEnemy, Vector3.up);

        // Adjust angle to match clock face (12 o'clock is forward)
        angle = (angle + 180) % 360;

        // Debug.Log($"Adjusted angle: {angle}");

        // Determine clock direction
        string clockDirection = GetClockDirection(angle);

        // Calculate distance and steps
        // Debug.Log($"Player position: {playerPosition}, Enemy position: {enemyPosition}");

        // Calculate distance ignoring vertical component
        Vector3 playerPositionFlat = new Vector3(playerPosition.x, 0, playerPosition.z);
        Vector3 enemyPositionFlat = new Vector3(enemyPosition.x, 0, enemyPosition.z);
        float distance = Vector3.Distance(playerPositionFlat, enemyPositionFlat);
        // Debug.Log($"Step distance: {stepDistance}");
        // Debug.Log($"Distance to enemy: {distance}");

        int steps = Mathf.CeilToInt(distance / stepDistance);

        // Output the result
        // Debug.Log($"Enemy is at {clockDirection}. {steps} steps away.");

        // Visualize the directions (for debugging)
        // Debug.DrawRay(playerPosition, playerForward * 5f, Color.blue, 2f);
        // Debug.DrawRay(playerPosition, directionToEnemy * 5f, Color.red, 2f);
        // Play the corresponding audio

        int clockDirectionIndex = Mathf.RoundToInt(angle / 30f) % 12;
        StartCoroutine(PlayDirectionAndStepsAudio(clockDirectionIndex, steps));

    
    }

    private string GetClockDirection(float angle)
    {
        // Normalize angle to 0-360 range
        angle = (angle + 360) % 360;
        // Debug.Log($"Enemy angle on a scale of 0-360 : {angle}");

        // Convert angle to clock direction
        int clockHour = Mathf.RoundToInt(angle / 30f);
        // Debug.Log($"Enemy angle in clock hours : {clockHour}");

        // Adjust for 12 o'clock position
        if (clockHour == 0 || clockHour == 12)
            return "12 o'clock";
        else
            return $"{clockHour} o'clock";
    }
    
    private IEnumerator PlayDirectionAndStepsAudio(int clockDirectionIndex, int steps)
    {
        isAudioPlaying = true;
        // Play clock direction audio first
        if (clockDirectionClips != null && clockDirectionIndex >= 0 && clockDirectionIndex < clockDirectionClips.Length)
        {
            audioSource.PlayOneShot(clockDirectionClips[clockDirectionIndex]);

            // Wait for the clock direction clip to finish playing
            yield return new WaitForSeconds(clockDirectionClips[clockDirectionIndex].length);
        }
        else
        {
            Debug.LogWarning("Clock direction audio clip not found!");
            isAudioPlaying = false;
            yield break;
        }

        // Play steps audio next
        if (stepClips != null && steps > 0 && steps - 1 < stepClips.Length)
        {
            audioSource.PlayOneShot(stepClips[steps - 1]);

            // Wait for the steps clip to finish playing
            yield return new WaitForSeconds(stepClips[steps - 1].length);
        }
        else
        {
            Debug.LogWarning("Steps audio clip not found!");
            isAudioPlaying = false;
            yield break;
        }
        isAudioPlaying = false;
     
    }
    public static void ResetTriggerPressCount()
    {
        triggerPressCount = 0;
    }
}