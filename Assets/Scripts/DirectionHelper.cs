using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DirectionHelper : MonoBehaviour
{
    public GameObject enemy;
  public float stepDistance = 1f; // Adjust this value based on your game's scale
    public InputActionReference leftTriggerAction;
    public Camera playerCamera; // The player's camera (assign the main VR camera)

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;  // Assign the main camera if none provided
        }

        // Subscribe to trigger action
        leftTriggerAction.action.performed += OnLeftTriggerPressed;
        Debug.Log("Left Trigger Action Performed");
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
        Vector3 playerPosition = playerCamera.transform.position;
        Debug.Log($"Player position = {playerPosition}");
        Vector3 enemyPosition = enemy.transform.position;
        Debug.Log($"Enemy position = {enemyPosition}");

        // Calculate direction and angle
        Vector3 directionToEnemy = enemyPosition - playerPosition;
        Debug.Log($"Direction to enemy = {directionToEnemy}");
        float angle = Vector3.SignedAngle(playerCamera.transform.forward, directionToEnemy, Vector3.up);
        Debug.Log($"Enemy angle = {angle}");

        // Determine clock direction
        string clockDirection = GetClockDirection(angle);

        // Calculate distance and steps
        float distance = Vector3.Distance(playerPosition, enemyPosition);

        Debug.Log($"Enemy distance = {distance}");  
        int steps = Mathf.CeilToInt(distance / stepDistance);

        // Output the result
        Debug.Log($"Enemy is at {clockDirection}. {steps} steps away.");
    }

    private string GetClockDirection(float angle)
    {
        // Normalize angle to 0-360 range
        angle = (angle + 360) % 360;
        Debug.Log($"Enemy angle on a scale of 0-360 : {angle}");

        // Convert angle to clock direction
        int clockHour = Mathf.RoundToInt(angle / 30f);
        Debug.Log($"Enemy angle in clock hours : {clockHour}");

        // Adjust for 12 o'clock position
        if (clockHour == 0 || clockHour == 12)
            return "12 o'clock";
        else
            return $"{clockHour} o'clock";
    }

    //test, and check if this implementation works
    public static string GetClockDirectionText(float angle)
    {
        if (angle >= 337.5f || angle < 22.5f)
            return "12 o'clock (Straight)";
        else if (angle >= 22.5f && angle < 67.5f)
            return "1 o'clock (Diagonal Right)";
        else if (angle >= 67.5f && angle < 112.5f)
            return "3 o'clock (Right)";
        else if (angle >= 112.5f && angle < 157.5f)
            return "4 o'clock (Diagonal Right)";
        else if (angle >= 157.5f && angle < 202.5f)
            return "6 o'clock (Back)";
        else if (angle >= 202.5f && angle < 247.5f)
            return "7 o'clock (Diagonal Left)";
        else if (angle >= 247.5f && angle < 292.5f)
            return "9 o'clock (Left)";
        else if (angle >= 292.5f && angle < 337.5f)
            return "10 o'clock (Diagonal Left)";
        else
            return "Unknown";
    }
}