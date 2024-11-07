using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class MenuUI : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public InputActionReference pauseAction;
    private bool isPaused = false;
    public Transform playerCamera; // Assign the VR camera in the inspector
    public float menuDistance = 1f; // Distance from the camera

    // Audio Mixer references
    public AudioMixer gameplayMixer;
    public AudioMixer menuMixer;
    public float transitionDuration = 0.2f; // Duration of the fade transition
    public float activeVolume = 0f; // 0 dB, full volume
    public float inactiveVolume = -80f; // -80 dB, effectively muted

    private void Awake()
    {
        if (pauseAction == null)
        {
            Debug.LogError("Pause action reference is not set in the inspector");
        }
        pauseAction.action.Enable();
        pauseAction.action.performed += TogglePause;
        // Initialize audio
        SetMixerVolumes(gameplayMixer, activeVolume, menuMixer, inactiveVolume);
    }

    private void Destroy()
    {
        pauseAction.action.Disable();
        pauseAction.action.performed -= TogglePause;
    }

    private void OnEnable()
    {
        pauseAction.action.performed += TogglePause;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= TogglePause;
    }

    private void PositionMenuInFrontOfPlayer()
    {
        if (pauseMenuCanvas != null && playerCamera != null)
        {
            // Calculate the position in front of the camera
            Vector3 menuPosition = playerCamera.position + playerCamera.forward * menuDistance;

            // Add the Y offset to raise the menu
            menuPosition.y += 0.01f;

            // Position the menu
            pauseMenuCanvas.transform.position = menuPosition;

            // Make the menu face the player
            Vector3 lookDirection = playerCamera.transform.position - pauseMenuCanvas.transform.position;
            lookDirection.y = 0; // This keeps the menu vertical
            pauseMenuCanvas.transform.rotation = Quaternion.LookRotation(-lookDirection);
        }
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        pauseMenuCanvas.SetActive(isPaused);

        if (isPaused)
        {
            PositionMenuInFrontOfPlayer();
            StartCoroutine(TransitionAudio(gameplayMixer, inactiveVolume, menuMixer, activeVolume));
        }
        else
        {
            StartCoroutine(TransitionAudio(menuMixer, inactiveVolume, gameplayMixer, activeVolume));
        }

        Time.timeScale = isPaused ? 0 : 1;
    }

    private System.Collections.IEnumerator TransitionAudio(AudioMixer fromMixer, float fromVolume, AudioMixer toMixer, float toVolume)
    {
        float elapsedTime = 0f;
        float startVolumeFrom, startVolumeTo;

        fromMixer.GetFloat("MasterVolume", out startVolumeFrom);
        toMixer.GetFloat("MasterVolume", out startVolumeTo);

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / transitionDuration;

            float newVolumeFrom = Mathf.Lerp(startVolumeFrom, fromVolume, t);
            float newVolumeTo = Mathf.Lerp(startVolumeTo, toVolume, t);

            fromMixer.SetFloat("MasterVolume", newVolumeFrom);
            toMixer.SetFloat("MasterVolume", newVolumeTo);

            yield return null;
        }

        // Ensure we end at exactly the target volumes
        fromMixer.SetFloat("MasterVolume", fromVolume);
        toMixer.SetFloat("MasterVolume", toVolume);
    }

    private void SetMixerVolumes(AudioMixer mixer1, float volume1, AudioMixer mixer2, float volume2)
    {
        mixer1.SetFloat("MasterVolume", volume1);
        mixer2.SetFloat("MasterVolume", volume2);
    }

    private void onDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                Debug.Log("Device disconnected: " + device);
                pauseAction.action.Disable();
                pauseAction.action.performed -= TogglePause;
                break;
            case InputDeviceChange.Reconnected:
                pauseAction.action.Enable();
                pauseAction.action.performed += TogglePause;
                Debug.Log("Device reconnected: " + device);
                break;
            default:
                Debug.Log("Device change: " + device);
                break;
        }
    }
}



