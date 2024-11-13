using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class AccessibleMenu : MonoBehaviour
{

    [Header("Menu UI")]
    public MenuUI menuUI;

    private bool IsMenuActive => menuUI != null && menuUI.IsPaused;

    [Header("Menu Buttons")]
    public Button difficultyButton;
    public Button tutorialButton;
    public Button boxingModeButton;
    public Button exerciseLevelButton;

    [Header("Text Components")]
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI boxingModeText;
    public TextMeshProUGUI exerciseLevelText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Button Hover Sounds")]
    public AudioClip difficultyEasyHoverSound;
    public AudioClip difficultyHardHoverSound;
    public AudioClip tutorialHoverSound;
    public AudioClip boxingOffensiveHoverSound;
    public AudioClip boxingDefensiveHoverSound;
    public AudioClip exerciseLowHoverSound;
    public AudioClip exerciseHighHoverSound;
    public AudioClip resumeHoverSound;
    public AudioClip pauseMenuActive;


    [Header("UI")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;

    [Header("Controllers")]
    public XRBaseController leftController;
    public XRBaseController rightController;

    [Header("Input")]
    public InputActionReference joystickAction;
    public InputActionReference triggerAction;

    private Button[] menuButtons;
    private int currentButtonIndex = 0;
    private float lastJoystickYValue = 0f;

    private bool isEasyDifficulty = true;
    private bool isOffensiveMode = true;
    private bool isLowExerciseLevel = true;


    private bool isFirstActivation = true;

    private CustomButtonHighlight[] buttonHighlights;

    void Start()
    {
        SetupButtons();
        UpdateButtonTexts();

        if (menuUI == null)
        {
            Debug.LogError("MenuUI reference is not set in the Inspector.");
        }

        menuButtons = new Button[] { difficultyButton, tutorialButton, boxingModeButton, exerciseLevelButton };

        if (joystickAction == null)
        {
            Debug.LogError("Joystick action is not assigned. Please assign it in the Inspector.");
        }
        else
        {
            joystickAction.action.performed += OnJoystickMoved;
        }
        triggerAction.action.performed += OnTriggerPressed;

        buttonHighlights = new CustomButtonHighlight[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            buttonHighlights[i] = menuButtons[i].GetComponent<CustomButtonHighlight>();
            if (buttonHighlights[i] == null)
            {
                Debug.LogError($"CustomButtonHighlight component missing on button {i}");
            }
        }
    }

    void OnDisable()
    {
        joystickAction.action.performed -= OnJoystickMoved;
        triggerAction.action.performed -= OnTriggerPressed;
    }

    void SetupButtons()
    {
        SetupButton(difficultyButton, ToggleDifficulty, "difficulty");
        SetupButton(tutorialButton, PlayTutorial, "tutorial");
        SetupButton(boxingModeButton, ToggleBoxingMode, "boxing");
        SetupButton(exerciseLevelButton, ToggleExerciseLevel, "exercise");
    }

    void SetupButton(Button button, UnityEngine.Events.UnityAction action, string buttonID)
    {
        button.onClick.AddListener(action);
        button.onClick.AddListener(PlayClickSound);

        // Add EventTrigger component if it doesn't exist
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Add select event
        EventTrigger.Entry selectEntry = new EventTrigger.Entry();
        selectEntry.eventID = EventTriggerType.Select;
        selectEntry.callback.AddListener((data) => { PlayHoverSound(buttonID); });
        eventTrigger.triggers.Add(selectEntry);
    }

    void OnJoystickMoved(InputAction.CallbackContext context)
    {
        if (!IsMenuActive) return;

        Vector2 joystickValue = context.ReadValue<Vector2>();
        float newJoystickYValue = joystickValue.y;

        // Check if the joystick has moved enough to trigger a new selection
        if (newJoystickYValue > 0.5f && lastJoystickYValue <= 0.5f)
        {
            NavigateMenu(-1); // Move up
            lastJoystickYValue = newJoystickYValue;
        }
        else if (newJoystickYValue < -0.5f && lastJoystickYValue >= -0.5f)
        {
            NavigateMenu(1); // Move down
            lastJoystickYValue = newJoystickYValue;
        }
        else if (Mathf.Abs(newJoystickYValue) < 0.1f)
        {
            // Reset when joystick is near neutral position
            lastJoystickYValue = 0f;
        }
    }

    void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!IsMenuActive) return;

        if (context.performed)
        {
            SelectCurrentButton();
        }
    }

    void NavigateMenu(int direction)
    {
        currentButtonIndex += direction;
        if (currentButtonIndex < 0) currentButtonIndex = menuButtons.Length - 1;
        if (currentButtonIndex >= menuButtons.Length) currentButtonIndex = 0;

        UpdateButtonHighlights();
        PlayHoverSound(GetButtonID(menuButtons[currentButtonIndex]));
    }

    void UpdateButtonHighlights()
    {
        for (int i = 0; i < buttonHighlights.Length; i++)
        {
            if (buttonHighlights[i] != null)
            {
                buttonHighlights[i].SetHighlighted(i == currentButtonIndex);
            }
        }
        EventSystem.current.SetSelectedGameObject(menuButtons[currentButtonIndex].gameObject);
    }

    public void OnPauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            if (isFirstActivation)
            {
                audioSource.PlayOneShot(pauseMenuActive);
                isFirstActivation = false;
            }
            currentButtonIndex = 0;
            UpdateButtonHighlights();
        }
        else
        {
            // Reset first activation flag when menu is closed
            isFirstActivation = true;
        }
    }

    void SelectCurrentButton()
    {
        menuButtons[currentButtonIndex].onClick.Invoke();
    }

    void PlayHoverSound(string buttonID)
    {
        SendHapticImpulse(leftController, 0.6f, 0.1f);
        switch (buttonID)
        {
            case "difficulty":
                Debug.Log("Hovering over difficulty button");
                if (!isFirstActivation)
                { 
                    audioSource.PlayOneShot(isEasyDifficulty ? difficultyEasyHoverSound : difficultyHardHoverSound);
                }
                break;
            case "tutorial":
                audioSource.PlayOneShot(tutorialHoverSound);
                break;
            case "boxing":
                audioSource.PlayOneShot(isOffensiveMode ? boxingOffensiveHoverSound : boxingDefensiveHoverSound);
                break;
            case "exercise":
                audioSource.PlayOneShot(isLowExerciseLevel ? exerciseLowHoverSound : exerciseHighHoverSound);
                break;
            default:
                audioSource.PlayOneShot(hoverSound);
                break;
        }
    }

    void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    void SendHapticImpulse(XRBaseController controller, float amplitude, float duration)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }

    string GetButtonID(Button button)
    {
        if (button == difficultyButton) return "difficulty";
        if (button == tutorialButton) return "tutorial";
        if (button == boxingModeButton) return "boxing";
        if (button == exerciseLevelButton) return "exercise";
        return "";
    }

    // ... (rest of your existing methods like ToggleDifficulty, PlayTutorial, etc.)

    void ToggleDifficulty()
    {
        isEasyDifficulty = !isEasyDifficulty;
        UpdateButtonTexts();
        PlayClickSound();
    }

    void PlayTutorial()
    {
        PlayClickSound();
        SceneManager.LoadScene("TutorialScene");
    }

    void ToggleBoxingMode()
    {
        isOffensiveMode = !isOffensiveMode;
        UpdateButtonTexts();
        PlayClickSound();
    }

    void ToggleExerciseLevel()
    {
        isLowExerciseLevel = !isLowExerciseLevel;
        UpdateButtonTexts();
        PlayClickSound();
    }


    void UpdateButtonTexts()
    {
        Debug.Log("Updating button TEXTSs...");
        difficultyText.text = isEasyDifficulty ? "Difficulty: Easy" : "Difficulty: Hard";
        boxingModeText.text = isOffensiveMode ? "Mode: Offensive" : "Mode: Defensive";
        exerciseLevelText.text = isLowExerciseLevel ? "Exercise: Low" : "Exercise: High";
    }

    void ResumeGame()
    {
        // Implement your resume game logic here
        Debug.Log("Resuming game...");
    }
}