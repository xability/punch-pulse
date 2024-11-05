using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public InputActionReference pauseActionReference;
    private bool isPaused = false;
    public Transform playerCamera; // Assign the VR camera in the inspector
    public float menuDistance = 3f; // Distance from the camera

    private void OnEnable()
    {
        pauseActionReference.action.performed += TogglePause;
    }

    private void PositionMenuInFrontOfPlayer()
    {
        if (pauseMenuCanvas != null && playerCamera != null)
        {
            // Position the menu in front of the camera
            pauseMenuCanvas.transform.position = playerCamera.position + playerCamera.forward * menuDistance;

            // Make the menu face the camera
            pauseMenuCanvas.transform.rotation = Quaternion.LookRotation(pauseMenuCanvas.transform.position - playerCamera.position);
        }
    }

    private void OnDisable()
    {
        pauseActionReference.action.performed -= TogglePause;
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        pauseMenuCanvas.SetActive(isPaused);
        if (isPaused)
        {
            PositionMenuInFrontOfPlayer();
        }
        Time.timeScale = isPaused ? 0 : 1;
        AudioListener.pause = isPaused;
    }


    
}