using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class AccessibleMenu : MonoBehaviour
{

    [Header("Menu Buttons")]
    public Button difficultyButton;
    public Button tutorialButton;
    public Button boxingModeButton;
    public Button exerciseLevelButton;
    public Button resumeButton;

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

    // Score and haptic feedback
    public XRBaseController leftController;  // Assign via Inspector
    public XRBaseController rightController; // Assign via Inspector

    [Header("Input")]
    public InputActionReference moveAction;
    public InputActionReference selectAction;

    private CustomButton[] menuButtons;
    private int currentButtonIndex = 0;

    private bool isEasyDifficulty = true;
    private bool isOffensiveMode = true;
    private bool isLowExerciseLevel = true;


    void Start()
    {
        SetupButtons();
        UpdateButtonTexts();

        // Initialize buttons array
        buttons = new Button[] { difficultyButton, tutorialButton, boxingModeButton, exerciseLevelButton, resumeButton };

        // Get the specific actions
        // Get all buttons and store them in an array
        menuButtons = GetComponentsInChildren<CustomButton>();

        // Enable input actions
        moveAction.action.Enable();
        selectAction.action.Enable();

        // Subscribe to input events
        moveAction.action.performed += OnMove;
        selectAction.action.performed += OnSelect;
    }


    void SetupButtons()
    {
        SetupButton(difficultyButton, ToggleDifficulty, "difficulty");
        SetupButton(tutorialButton, PlayTutorial, "tutorial");
        SetupButton(boxingModeButton, ToggleBoxingMode, "boxing");
        SetupButton(exerciseLevelButton, ToggleExerciseLevel, "exercise");
        SetupButton(resumeButton, ResumeGame, "resume");
    }

    void SetupButton(Button button, UnityEngine.Events.UnityAction action, string buttonID)
    {
        button.onClick.AddListener(action);
        button.onClick.AddListener(PlayClickSound);
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Select;
        entry.callback.AddListener((data) => { PlayHoverSound(buttonID); });
        trigger.triggers.Add(entry);
    }


    string GetButtonID(Button button)
    {
        if (button == difficultyButton) return "difficulty";
        if (button == tutorialButton) return "tutorial";
        if (button == boxingModeButton) return "boxing";
        if (button == exerciseLevelButton) return "exercise";
        if (button == resumeButton) return "resume";
        return "";
    }

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        Vector2 navigateValue = context.ReadValue<Vector2>();
        // Use navigateValue.y for vertical navigation
        if (navigateValue.y > 0.5f)
        {
            NavigateButtons(-1); // Move up
        }
        else if (navigateValue.y < -0.5f)
        {
            NavigateButtons(1); // Move down
        }
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        // Invoke the currently selected button
        buttons[currentButtonIndex].onClick.Invoke();
    }

    void OnDisable()
    {
        // Unsubscribe from input events
        moveAction.action.performed -= OnMove;
        selectAction.action.performed -= OnSelect;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();

        if (movement.y > 0.5f)
        {
            NavigateMenu(-1); // Move up
        }
        else if (movement.y < -0.5f)
        {
            NavigateMenu(1); // Move down
        }
    }

    void NavigateMenu(int direction)
    {
        currentButtonIndex += direction;
        currentButtonIndex = Mathf.Clamp(currentButtonIndex, 0, menuButtons.Length - 1);

        // Highlight the current button
        HighlightButton(menuButtons[currentButtonIndex]);
    }

    void HighlightButton(CustomButton button)
    {
        // Deselect all buttons
        foreach (var btn in menuButtons)
        {
            btn.GetComponent<Selectable>().OnDeselect(null);
        }

        // Select the current button
        button.GetComponent<Selectable>().OnSelect(null);

        // Play hover sound
        PlayHoverSound(GetButtonID(button));
    }

    void OnSelect(InputAction.CallbackContext context)
    {
        // Trigger the current button's action
        menuButtons[currentButtonIndex].onClick.Invoke();
    }


    void PlayHoverSound(string buttonID)
    {
        SendHapticImpulse(leftController, 0.6f, 0.1f);
        switch (buttonID)
        {
            case "difficulty":
                audioSource.PlayOneShot(isEasyDifficulty ? difficultyEasyHoverSound : difficultyHardHoverSound);
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
            case "resume":
                audioSource.PlayOneShot(resumeHoverSound);
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