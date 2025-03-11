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
        if (currentClip < step.narration.Length)
        {
            audioSource.clip = step.narration[currentClip];
            audioSource.PlayOneShot(step.narration[currentClip]);
            isAudioPlaying = true;
            yield return new WaitForSeconds(step.narration[currentClip].length);
            isAudioPlaying = false;
            waitingForAction = true;

        }
        else
        {
            NextStep();
            yield return new WaitForSeconds(1);
        }
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
        if (step.requiredActions[currentClip].action.triggered)
        {
            waitingForAction = false;
            Debug.Log("Tutorial Step number " + currentClip + " completed");
            currentClip++;

            // Check if the current step requires waiting for audio
            if (StepRequiresAudioWait(step, currentClip - 1))
            {
                StartCoroutine(WaitForAudioAndPlayNext(step, currentClip - 1));
            }
            else
            {
                StartCoroutine(PlayNextClip());
            }
        }
    }

    bool StepRequiresAudioWait(TutorialStep step, int clipIndex)
    {
        // Define the steps and clips that require waiting for audio
        
        return (step.StepNum == 2 && (clipIndex == 0 || clipIndex == 2 || clipIndex == 5)) || (step.StepNum == 3);
            //||
            // (step.StepNum == 5 && clipIndex == 0);
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
        else if (step.StepNum == 3)
        {
            if (clipIndex == 0) { 
                TutorialAttackFlag = true;
                Debug.Log("Enemy attack set , called in clipindex 0");
                yield return StartCoroutine(enemyAttackBehavior.PerformAttack());
                TutorialAttackFlag = false;
                yield return new WaitForSeconds(4);
            }
            else if (clipIndex == 1)
            {
                TutorialAttackFlag = true;
                Debug.Log("Enemy attack set , called in clipindex 0");
                yield return StartCoroutine(enemyAttackBehavior.PerformAttack());
                TutorialAttackFlag = false;
                yield return new WaitForSeconds(5);
            }
        }
        Debug.Log("Waiting for audio to finish");
        StartCoroutine(PlayNextClip());
        yield return new WaitForSeconds(2);
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

    public void RestartTutorial()
    {
        currentStep = 0;
        currentClip = 0;
        waitingForAction = false;
        tutorialStarted = false;
        isAudioPlaying = false;
        TutorialCompleted = false;
        TutorialAttackFlag = false;

        // Reset any other necessary variables

        instructionText.text = "Press right select button to start";
        stepcount.text = "0";

        StartTutorial();
    }

}

