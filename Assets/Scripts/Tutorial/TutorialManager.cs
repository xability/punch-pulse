using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI stepcount;
    public InputActionReference nextButtonAction;
    public InputActionReference exitTutorialAction;
    public AudioSource audioSource;
    public GameObject objectToToggle0;
    public GameObject objectToToggle1;
    public AudioClip boxingbell;
    public bool skipToLastStep = false;

    [System.Serializable]
    public class TutorialStep
    {
        public string instruction;
        public AudioClip[] narration;
        public InputActionReference[] requiredActions;
        public int StepNum;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
    }

    public TutorialStep[] tutorialSteps;

    public int currentStep = 0;
    public int currentClip = 0;
    private bool waitingForAction = false;
    private bool tutorialStarted = false;
    private bool isAudioPlaying = false;

    public EnemyAttackBehavior enemyAttackBehavior;
    public RoundsManager roundsManager;
    public static bool TutorialCompleted = false;

    public static bool TutorialAttackFlag = false;

    public static bool GetTutorialStatus()
    {
        return TutorialCompleted;
    }

    public static bool GetTutorialAttackFlagStatus()
    {
        return TutorialAttackFlag;
    }


    void Start()
    {
        if (skipToLastStep)
        {
            SkipToLastStep();
        }
        else
        {
            instructionText.text = "Press right select button to start";
            stepcount.text = "0";
        }
        nextButtonAction.action.performed += OnNextButtonPressed;
        exitTutorialAction.action.performed += ExitTutorial;
    }

    void StartTutorial()
    {
        tutorialStarted = true;
        UpdateTutorialStep();
    }

    void Update()
    {
        if (waitingForAction)
        {
            CheckRequiredAction();
        }
        UpdateScoreVisibility();
    }

    private void UpdateScoreVisibility()
    {
        if (objectToToggle0 != null)
        {
            objectToToggle0.SetActive(TutorialCompleted);
        }
        if (objectToToggle1 != null)
        {
            objectToToggle1.SetActive(!TutorialCompleted);
        }
    }

    void UpdateTutorialStep()
    {
        var step = tutorialSteps[currentStep];
        instructionText.text = step.instruction;
        stepcount.text = $"{step.StepNum} of {tutorialSteps.Length}";

        foreach (var obj in step.objectsToActivate)
            obj.SetActive(true);
        foreach (var obj in step.objectsToDeactivate)
            obj.SetActive(false);

        /*if (skipToLastStep)
        {
            // If we've skipped to the last step, we might want to handle this differently
            // For example, we might not want to play any audio
            waitingForAction = false;
        }
        else
        {*/

        currentClip = 0;
        StartCoroutine(PlayNextClip());
        
    }

    IEnumerator PlayNextClip()
    {
        var step = tutorialSteps[currentStep];

        // If no more clips in this step, move on to next step
        if (currentClip >= step.narration.Length)
        {
            NextStep();
            yield return new WaitForSeconds(1);
        }

        isAudioPlaying = true;
        audioSource.clip = step.narration[currentClip];
        audioSource.Play();
        // Wait for the audio clip to finish
        yield return new WaitForSeconds(audioSource.clip.length);
        isAudioPlaying = false;

        // Only after the clip finishes do we require the player action:
        waitingForAction = true;

    }

    private void SkipToLastStep()
    {
        currentStep = tutorialSteps.Length - 1;
        tutorialStarted = true;
        UpdateTutorialStep();
        // Optionally, you might want to call ExitTutorial here if you want to immediately end the tutorial
        // ExitTutorial(new InputAction.CallbackContext());
    }

    void CheckRequiredAction()
    {
        var step = tutorialSteps[currentStep];
        // If the currentClip is still within array bounds, see if that clip has a required action:
        if (currentClip < step.requiredActions.Length)
        {
            if (step.requiredActions[currentClip].action.triggered)
            {
                waitingForAction = false;
                Debug.Log("Tutorial Step number " + currentClip + " completed");

                // Check if the current step requires waiting for audio
                if (StepRequiresAudioWait(step, currentClip - 1))
                {
                    StartCoroutine(WaitForAudioAndPlayNext(step, currentClip - 1));
                }
                else
                {
                    currentClip++;
                    StartCoroutine(PlayNextClip());
                }
            }
        }

    }

    bool StepRequiresAudioWait(TutorialStep step, int clipIndex)
    {
        // Define the steps and clips that require waiting for audio

        if (step.StepNum == 2 && (clipIndex == 0 || clipIndex == 2 || clipIndex == 5))
            return true;
        if (step.StepNum == 3)
            return true;

        return false;
    }

    IEnumerator WaitForAudioAndPlayNext(TutorialStep step, int clipIndex)
    {
        // Wait for the current audio clip to finish
        if (step.StepNum == 2)
        {
            if (clipIndex == 0)
            {
                Debug.Log("iNC wait time");
                yield return new WaitForSeconds(7);
            }
            else if (clipIndex == 2)
            {
                yield return new WaitForSeconds(2);
            }
            else if (clipIndex == 5)
            {
                Debug.Log("iNCREASED wait time");
                yield return new WaitForSeconds(11);
            }
        }
        // 2) If step == 3, handle enemy attacks:
        else if (step.StepNum == 3)
        {
            // Kick off the enemy attack
            TutorialAttackFlag = true;
            Debug.Log("Enemy attack set");
            yield return StartCoroutine(enemyAttackBehavior.PerformAttack());
            TutorialAttackFlag = false;

            // Then some extra wait
            if (clipIndex == 0)
            {
                yield return new WaitForSeconds(4f);
            }
            else if (clipIndex == 1)
            {
                yield return new WaitForSeconds(5f);
            }
        }
        Debug.Log("Waiting for audio to finish");
        currentClip++;
        StartCoroutine(PlayNextClip());

    }

    private void OnNextButtonPressed(InputAction.CallbackContext context)
    {
        // This method is now only used for debugging or skipping steps if needed
        if (isAudioPlaying)
        {
            // Ignore button press while audio is playing
            return;
        }

        if (!tutorialStarted)
        {
            StartTutorial();
        }
        else if (!waitingForAction)
        {
            NextStep();
        }
    }

    void NextStep()
    {
        if (currentStep < tutorialSteps.Length - 1)
        {
            currentStep++;
            UpdateTutorialStep();
        }
        else if (currentStep == tutorialSteps.Length - 1)
        {
            ExitTutorial(new InputAction.CallbackContext());
        }
    }

    private void ExitTutorial(InputAction.CallbackContext context)
    {
        if (!TutorialCompleted)
        {
            TutorialCompleted = true;
            TutorialAttackFlag = true;
            if (roundsManager != null)
            {
                roundsManager.BeginRounds();
            }
            else
            {
                Debug.LogError("RoundsManager reference is missing!");
            }
            // SceneManager.LoadScene("BoxingRing");
        }
    }

    void OnDisable()
    {
        nextButtonAction.action.performed -= OnNextButtonPressed;
        exitTutorialAction.action.performed -= ExitTutorial;
    }

    /// <summary>
    /// Public method to completely restart the tutorial from scratch.
    /// </summary>
    public void RestartTutorial()
    {
        currentStep = 0;
        currentClip = 0;
        waitingForAction = false;
        tutorialStarted = false;
        isAudioPlaying = false;
        TutorialCompleted = false;
        TutorialAttackFlag = false;

        // Re‐init UI to default
        instructionText.text = "Press right select button to start";
        stepcount.text = "0";

        StartTutorial();
    }

}

