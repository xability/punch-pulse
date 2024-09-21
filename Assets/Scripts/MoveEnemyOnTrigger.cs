using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveEnemyInFront : MonoBehaviour
{
    public Transform enemy;  // The enemy GameObject to be moved, assign via Inspector
    public Transform player;  // The player's position (usually attach to the XR Rig or player object)
    public Camera playerCamera; // The player's camera (usually assign to the main VR camera)

    public InputActionReference rightTriggerAction;  // Input action for the right controller trigger
    public float distanceInFront = 3f;  // Distance to place the enemy in front of the player
    public float heightOffset = 1.5f;   // Optional height offset for enemy placement

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;  // Assign the main camera if none provided
        }

        // Subscribe to trigger action
        rightTriggerAction.action.performed += OnRightTriggerPressed;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        rightTriggerAction.action.performed -= OnRightTriggerPressed;
    }

    // Called when the right trigger is pressed
    private void OnRightTriggerPressed(InputAction.CallbackContext context)
    {
        MoveEnemyInFrontOfPlayer();
    }

    // Moves the enemy in front of the player
    void MoveEnemyInFrontOfPlayer()
    {
        if (enemy == null || playerCamera == null)
        {
            Debug.LogWarning("Enemy or Player Camera is not assigned!");
            return;
        }

        // Calculate the new position in front of the player
        Vector3 forwardDirection = playerCamera.transform.forward;
        forwardDirection.y = 0;  // Ignore vertical direction to keep it level
        forwardDirection.Normalize();  // Normalize the forward vector

        Vector3 newEnemyPosition = playerCamera.transform.position + forwardDirection * distanceInFront;
        newEnemyPosition.y += heightOffset;  // Apply height offset if needed

        // Move the enemy to the new position
        enemy.position = newEnemyPosition;

        // Optionally, rotate the enemy to face the player
        enemy.LookAt(playerCamera.transform.position);

        Debug.Log("Enemy moved in front of player!");
    }
}
