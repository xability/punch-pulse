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
    public float moveSpeed = 2f;  // Speed at which the enemy moves
    public float rotationSpeed = 3f; // Speed at which the enemy rotates to face the player

    private Vector3 targetPosition;
    private bool shouldMove = false;
    private float initialYPosition;  // To store enemy's initial Y position

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;  // Assign the main camera if none provided
        }

        // Get and store the initial Y position of the enemy
        if (enemy != null)
        {
            initialYPosition = enemy.position.y;
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
        SetTargetPositionInFrontOfPlayer();
        shouldMove = true;  // Start moving the enemy
    }

    // Sets the target position for the enemy in front of the player (only x and z change)
    void SetTargetPositionInFrontOfPlayer()
    {
        if (enemy == null || playerCamera == null)
        {
            Debug.LogWarning("Enemy or Player Camera is not assigned!");
            return;
        }

        // Calculate the target position in front of the player, only change x and z
        Vector3 forwardDirection = playerCamera.transform.forward;
        forwardDirection.y = 0;  // Ignore vertical direction to keep it level
        forwardDirection.Normalize();  // Normalize the forward vector

        // Target position with only x and z being updated, y remains the same
        targetPosition = playerCamera.transform.position + forwardDirection * distanceInFront;
        targetPosition.y = initialYPosition;  // Keep the enemy's y-coordinate fixed
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldMove)
        {
            MoveEnemyTowardsTarget();
        }
    }

    // Smoothly moves the enemy towards the target position (only in x and z)
    void MoveEnemyTowardsTarget()
    {
        if (enemy == null) return;

        // Move the enemy towards the target position (only x and z)
        Vector3 currentPosition = enemy.position;
        currentPosition = Vector3.MoveTowards(
            new Vector3(currentPosition.x, initialYPosition, currentPosition.z),
            targetPosition,
            moveSpeed * Time.deltaTime
        );
        enemy.position = currentPosition;

        // Rotate the enemy to face the player along the y-axis (no other axis rotation)
        Vector3 directionToPlayer = playerCamera.transform.position - enemy.position;
        directionToPlayer.y = 0;  // Ignore vertical direction to keep rotation level
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        enemy.rotation = Quaternion.Slerp(enemy.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Stop moving when the enemy reaches the target position
        if (Vector3.Distance(enemy.position, targetPosition) < 0.1f)
        {
            shouldMove = false;  // Stop moving the enemy
        }
    }
}
