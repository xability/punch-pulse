using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveEnemyInFront : MonoBehaviour
{
    public Transform enemy;  // The enemy GameObject to be moved, assign via Inspector
    public Transform player;  // The player's position (usually attach to the XR Rig or player object)
    public Camera playerCamera; // The player's camera (assign the main VR camera)

    public InputActionReference rightTriggerAction;  // Input action for the right controller trigger
    public float distanceInFront;  // Distance to place the enemy in front of the player
    public float moveSpeed = 2f;  // Speed at which the enemy moves
    public float rotationSpeed = 3f; // Speed at which the enemy rotates to face the player

    private Vector3 targetPosition;
    private bool shouldMove = false;
    private float initialXPosition;  // To store the enemy's initial X position (vertical)
    private float initialYPosition;


    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;  // Assign the main camera if none provided
        }

        // Store the initial X (vertical) position of the enemy
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

    // Sets the target position for the enemy in front of the player (only Z and Y change)
    void SetTargetPositionInFrontOfPlayer()
    {
        if (enemy == null || playerCamera == null)
        {
            Debug.LogWarning("Enemy or Player Camera is not assigned!");
            return;
        }

        // Calculate the target position in front of the player (only Z and Y change, X stays the same)
        Vector3 forwardDirection = playerCamera.transform.forward;
        forwardDirection.x = 0;  // Ignore X-axis direction to keep it horizontal
        forwardDirection.Normalize();  // Normalize the forward vector

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

    // Smoothly moves the enemy towards the target position (only in Z and Y)
    void MoveEnemyTowardsTarget()
    {
        if (enemy == null) return;

        // Move the enemy towards the target position (only Z and Y)
        Vector3 currentPosition = enemy.position;
        currentPosition = Vector3.MoveTowards(
            new Vector3(currentPosition.x, initialYPosition, currentPosition.z),
            targetPosition,
            moveSpeed * Time.deltaTime
        );
        enemy.position = currentPosition;

        // Rotate the enemy to face the player along the X-axis (vertical rotation)
        RotateEnemyTowardsPlayer();


    }

    // Rotates the enemy to face the player (only around X-axis)
    void RotateEnemyTowardsPlayer()
    {
        if (enemy == null || playerCamera == null) return;

        // Get the direction from the enemy to the player (ignoring X-axis difference)
        Vector3 directionToPlayer = playerCamera.transform.position - enemy.position;
        //directionToPlayer.x = 0;  // Ignore vertical (X-axis) difference
        directionToPlayer.z = 0;
        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);


        // Apply the rotation smoothly over time
        enemy.rotation = Quaternion.Slerp(enemy.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // enemy.rotation.x = enemy.rotation.x + 270;
        // Stop rotation once it's nearly complete
        if (Quaternion.Angle(enemy.rotation, targetRotation) < 0.1f)
        {
            shouldMove = false;  // Stop rotating once close enough
        }
    }

}
