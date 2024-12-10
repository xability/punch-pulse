using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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
    public Button scoreNarrationButton;
    public Button enemyAudioCueButton;
    public Button exerciseLevelButton;

    [Header("Text Components")]
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI boxingModeText;
    public TextMeshProUGUI scoreNarrationText;
    public TextMeshProUGUI enemyAudioCueText;
    public TextMeshProUGUI exerciseLevelText;
    public TextMeshProUGUI LTCount;
    public TextMeshProUGUI RTCount;
    public TextMeshProUGUI duckCount;
    public TextMeshProUGUI playerHitCount;
    public TextMeshProUGUI playerHeadPunchCount;
    public TextMeshProUGUI playerBodyPunchCount;
    public TextMeshProUGUI TotalPunchesThrown;

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
    public AudioClip resumeHoverSound;
    public AudioClip pauseMenuActive;
    public AudioClip difficultyMediumHoverSound;


    [Header("UI")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;

    [Header("Controllers")]
    public XRBaseController leftController;
    public XRBaseController rightController;

    [Header("Input")]
    public InputActionReference joystickAction;
    public InputActionReference triggerAction;

    public AudioClip difficultyIncreaseSound;
    public AudioClip difficultyDecreaseSound;

    public static AccessibleMenu Instance { get; private set; }

    private Button[] menuButtons;
    private int currentButtonIndex = 0;
    private float lastJoystickYValue = 0f;

    private static DifficultyLevel currentDifficulty = DifficultyLevel.Easy;
    private static bool isOffensiveMode = true;
    private static bool scoreNarration = false;
    private static bool enemyAudioCue = false;

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public int currentLTCount;
    public int currentRTCount;
    public int currentDuckCount;
    public int currentPlayerHitCount;
    public int currentPlayerHeadPunchCount;
    public int currentPlayerBodyPunchCount;

    private bool isFirstActivation = true;

    public TutorialManager tutorialManager;

    private CustomButtonHighlight[] buttonHighlights;

    void Start()
    {
        SetupButtons();
        UpdateButtonTexts();

        if (menuUI == null)
        {
            Debug.LogError("MenuUI reference is not set in the Inspector.");
        }

        menuButtons = new Button[] { difficultyButton, tutorialButton, boxingModeButton, scoreNarrationButton, enemyAudioCueButton};

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
        isFirstActivation = true;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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
        SetupButton(boxingModeButton, ToggleScoreNarration, "score");
        SetupButton(boxingModeButton, ToggleEnemyAudioCue, "enemy");
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
        Debug.Log("Navigating menu...");
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

    void UpdateStats(int leftTrigCount, int rightTrigCount, int duckCount, int playerHitCount, int headPunchCount, int bodyPunchCount)
    {
        
        if (LTCount != null)
            LTCount.text = "# Left Trigger: " + leftTrigCount.ToString();

        if (RTCount != null)
            RTCount.text = "# Right Trigger: " + rightTrigCount.ToString();

        if (this.duckCount != null)
            this.duckCount.text = "# Ducks: " + duckCount.ToString();

        if (this.playerHitCount != null)
            this.playerHitCount.text = "# Hits By Enemy: " + playerHitCount.ToString();

        if (playerHeadPunchCount != null)
            playerHeadPunchCount.text = "# Head punches: " + headPunchCount.ToString();

        if (playerBodyPunchCount != null)
            playerBodyPunchCount.text = "# Body punches: " + bodyPunchCount.ToString();
    }

    public void OnPauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            currentButtonIndex = 0;
            UpdateButtonHighlights();
            currentLTCount = DirectionHelper.GetLeftTriggerPressCount();
            currentRTCount = MoveEnemyInFront.GetRightTriggerPressCount();
            currentDuckCount = EnemyAttackBehavior.GetPlayerDuckCount();
            currentPlayerHitCount = EnemyAttackBehavior.GetPlayerHitCount();
            currentPlayerHeadPunchCount = PlayAudioOnBoxing.GetPlayerHeadPunchCount();
            currentPlayerBodyPunchCount = PlayAudioOnBoxing.GetPlayerBodyPunchCount();
            UpdateStats(currentLTCount, currentRTCount, currentDuckCount, currentPlayerHitCount, currentPlayerHeadPunchCount, currentPlayerBodyPunchCount);
            if (isFirstActivation)
            {
                StartCoroutine(PlayPauseMenuActiveSound());
                isFirstActivation = false;
            }
        }
        else
        {
            // Reset first activation flag when menu is closed
            isFirstActivation = true;
        }
    }

    private IEnumerator PlayPauseMenuActiveSound()
    {
        audioSource.PlayOneShot(pauseMenuActive);
        yield return new WaitForSeconds(pauseMenuActive.length);

    }

    void SelectCurrentButton()
    {
        menuButtons[currentButtonIndex].onClick.Invoke();
        Debug.Log("Selecting current button...");
        PlayHoverSound(GetButtonID(menuButtons[currentButtonIndex]));
    }

    void PlayHoverSound(string buttonID)
    {
        SendHapticImpulse(leftController, 0.6f, 0.1f);

        // Stop any currently playing sound
        audioSource.Stop();

        switch (buttonID)
        {
            case "difficulty":
                Debug.Log("Hovering over difficulty button, : " + isFirstActivation);
                if (!isFirstActivation)
                {
                    switch (currentDifficulty)
                    {
                        case DifficultyLevel.Easy:
                            audioSource.PlayOneShot(difficultyEasyHoverSound);
                            break;
                        case DifficultyLevel.Medium:
                            audioSource.PlayOneShot(difficultyMediumHoverSound);
                            break;
                        case DifficultyLevel.Hard:
                            audioSource.PlayOneShot(difficultyHardHoverSound);
                            break;
                    }
                }
                break;
            case "tutorial":
                audioSource.PlayOneShot(tutorialHoverSound);
                break;
            case "boxing":
                audioSource.PlayOneShot(isOffensiveMode ? boxingOffensiveHoverSound : boxingDefensiveHoverSound);
                break;
            default:
                audioSource.PlayOneShot(hoverSound);
                break;
        }
    }

    void PlayClickSound()
    {
        //audioSource.PlayOneShot(clickSound);
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
        if (button == scoreNarrationButton) return "score";
        if (button == enemyAudioCueButton) return "enemy";
        return "";
    }

    // ... (rest of your existing methods like ToggleDifficulty, PlayTutorial, etc.)
    public static DifficultyLevel CurrentDifficulty
    {
        get { return currentDifficulty; }
        private set { currentDifficulty = value; }
    }

    public static void SetDifficulty(DifficultyLevel newDifficulty)
    {
        DifficultyLevel oldDifficulty = CurrentDifficulty;
        CurrentDifficulty = newDifficulty;
        Instance.UpdateButtonTexts();
        
        Debug.Log("Difficulty changed to: " + newDifficulty);

        // You might want to add additional logic here, like playing a sound or showing a notification
        if (Instance.audioSource != null)
        {
            if (newDifficulty > oldDifficulty)
            {
                Instance.audioSource.PlayOneShot(Instance.difficultyIncreaseSound);
            }
            else
            {
                Instance.audioSource.PlayOneShot(Instance.difficultyDecreaseSound);
            }
        }

    }



    public static bool IsOffensiveMode
    {
        get { return isOffensiveMode; }
    }


    void ToggleDifficulty()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                currentDifficulty = DifficultyLevel.Medium;
                break;
            case DifficultyLevel.Medium:
                currentDifficulty = DifficultyLevel.Hard;
                break;
            case DifficultyLevel.Hard:
                currentDifficulty = DifficultyLevel.Easy;
                break;
        }
        UpdateButtonTexts();
        PlayClickSound();
    }

    void PlayTutorial()
    {
        PlayClickSound();

        if (tutorialManager != null)
        {
            tutorialManager.RestartTutorial();
        }
        else
        {
            Debug.LogError("TutorialManager reference is not set!");
        }
        // SceneManager.LoadScene("TutorialScene"); --> commenting this out for now to avoid loading the tutorial scene
    }

    void ToggleBoxingMode()
    {
        isOffensiveMode = !isOffensiveMode;
        UpdateButtonTexts();
        PlayClickSound();
    }

    void ToggleScoreNarration()
    {
        // Toggle score narration
        PlayClickSound();
    }

    void ToggleEnemyAudioCue()
    {
        // Toggle enemy audio cue
        PlayClickSound();
    }

    void UpdateButtonTexts()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                difficultyText.text = "Difficulty: Easy";
                break;
            case DifficultyLevel.Medium:
                difficultyText.text = "Difficulty: Medium";
                break;
            case DifficultyLevel.Hard:
                difficultyText.text = "Difficulty: Hard";
                break;
        }
        boxingModeText.text = isOffensiveMode ? "Ducking: On" : "Ducking: Off";
        scoreNarrationText.text = "Score Narration: " + (scoreNarration ? "On" : "Off");
        enemyAudioCueText.text = "Enemy Audio Cue: " + (enemyAudioCue ? "On" : "Off");
    }

    void ResumeGame()
    {
        // Implement your resume game logic here
        Debug.Log("Resuming game...");
    }

    void GetGameStats()
    {
        currentLTCount = DirectionHelper.GetLeftTriggerPressCount();
        Debug.Log("Left trigger has been pressed " + currentLTCount + " times.");

        currentRTCount = MoveEnemyInFront.GetRightTriggerPressCount();
        Debug.Log("Right trigger has been pressed " + currentRTCount + " times.");

        currentDuckCount = EnemyAttackBehavior.GetPlayerDuckCount();
        Debug.Log("Player has ducked " + currentDuckCount + " times.");

        currentPlayerHitCount = EnemyAttackBehavior.GetPlayerHitCount();
        Debug.Log("Enemy has hit the player " + currentPlayerHitCount + " times.");

        currentPlayerHeadPunchCount = PlayAudioOnBoxing.GetPlayerHeadPunchCount();
        Debug.Log("Player has punched the enemy's head " + currentPlayerHeadPunchCount + " times.");

        currentPlayerBodyPunchCount = PlayAudioOnBoxing.GetPlayerBodyPunchCount();
        Debug.Log("Player has punched the enemy's body " + currentPlayerBodyPunchCount + " times.");

    }

    void ResetLeftTriggerCount()
    {
        DirectionHelper.ResetTriggerPressCount();
        MoveEnemyInFront.ResetTriggerPressCount();
    }

}

/*
 * 
 *    private Coroutine currentAudioCoroutine;

    void PlayHoverSound(string buttonID)
    {
        SendHapticImpulse(leftController, 0.6f, 0.1f);

        // Stop the current audio coroutine if it's running
        if (currentAudioCoroutine != null)
        {
            StopCoroutine(currentAudioCoroutine);
        }

        // Start a new audio coroutine
        currentAudioCoroutine = StartCoroutine(PlayHoverSoundCoroutine(buttonID));
    }

    IEnumerator PlayHoverSoundCoroutine(string buttonID)
    {
        // Stop any currently playing sound
        audioSource.Stop();

        // Wait for a frame to ensure the audio has stopped
        yield return null;

        AudioClip clipToPlay = null;

        switch (buttonID)
        {
            case "difficulty":
                Debug.Log("Hovering over difficulty button, : " + isFirstActivation);
                if (!isFirstActivation)
                {
                    switch (currentDifficulty)
                    {
                        case DifficultyLevel.Easy:
                            clipToPlay = difficultyEasyHoverSound;
                            break;
                        case DifficultyLevel.Medium:
                            clipToPlay = difficultyMediumHoverSound;
                            break;
                        case DifficultyLevel.Hard:
                            clipToPlay = difficultyHardHoverSound;
                            break;
                    }
                }
                break;
            case "tutorial":
                clipToPlay = tutorialHoverSound;
                break;
            case "boxing":
                clipToPlay = isOffensiveMode ? boxingOffensiveHoverSound : boxingDefensiveHoverSound;
                break;
            default:
                clipToPlay = hoverSound;
                break;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
            yield return new WaitForSeconds(clipToPlay.length);
        }

        currentAudioCoroutine = null;
    }

*/