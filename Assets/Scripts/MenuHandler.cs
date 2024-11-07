using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class AccessibleMenu : MonoBehaviour
{

    [Header("Menu Buttons")]
    public CustomButton difficultyButton;
    public CustomButton tutorialButton;
    public CustomButton boxingModeButton;
    public CustomButton exerciseLevelButton;
    public CustomButton resumeButton;

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

    private bool isEasyDifficulty = true;
    private bool isOffensiveMode = true;
    private bool isLowExerciseLevel = true;

    void Start()
    {
        SetupButtons();
        UpdateButtonTexts();
    }

    void SetupButtons()
    {
        SetupButton(difficultyButton, ToggleDifficulty, "difficulty");
        SetupButton(tutorialButton, PlayTutorial, "tutorial");
        SetupButton(boxingModeButton, ToggleBoxingMode, "boxing");
        SetupButton(exerciseLevelButton, ToggleExerciseLevel, "exercise");
    }

    void SetupButton(CustomButton button, UnityEngine.Events.UnityAction action, string buttonID)
    {
        button.onClick.AddListener(action);
        button.onPointerEnter.AddListener(() => PlayHoverSound(buttonID));
    }


    /*
      
     void SetupButtons()
        {
            SetupButton(difficultyButton.GetComponent<XRSimpleInteractable>(), ToggleDifficulty, "difficulty");
            SetupButton(tutorialButton.GetComponent<XRSimpleInteractable>(), PlayTutorial, "tutorial");
            SetupButton(boxingModeButton.GetComponent<XRSimpleInteractable>(), ToggleBoxingMode, "boxing");
            SetupButton(exerciseLevelButton.GetComponent<XRSimpleInteractable>(), ToggleExerciseLevel, "exercise");
            SetupButton(resumeButton.GetComponent<XRSimpleInteractable>(), ResumeGame, "resume");
        }

        void SetupButton(XRSimpleInteractable button, UnityEngine.Events.UnityAction action, string buttonID)
        {
            button.selectEntered.AddListener((args) => PlayHoverSound(buttonID));
            button.activated.AddListener((args) => action());
        }
     */


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
}