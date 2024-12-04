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
        instructionText.text = "Press right select button to start";
        stepcount.text = "0";
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

        currentClip = 0;
        PlayNextClip();
    }

    void PlayNextClip()
    {
        var step = tutorialSteps[currentStep];
        if (currentClip < step.narration.Length)
        {
            audioSource.clip = step.narration[currentClip];
            audioSource.Play();
            isAudioPlaying = true;
            StartCoroutine(WaitForClipEnd());

        }
        else
        {
            NextStep();
        }
    }

    IEnumerator WaitForClipEnd()
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        isAudioPlaying = false;
        waitingForAction = true;
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
                PlayNextClip();
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
            if (clipIndex == 0 || clipIndex == 5)
            {
                yield return new WaitForSeconds(3);
            }
            else if (clipIndex == 2)
            {
                yield return new WaitForSeconds(1);
            }
        }
        else if (step.StepNum == 3)
        {
            if (clipIndex == 0) { 
                TutorialAttackFlag = true;
                yield return StartCoroutine(enemyAttackBehavior.PerformAttack());
                TutorialAttackFlag = false;
                yield return new WaitForSeconds(2);
            }
            if (clipIndex == 1)
            {
                TutorialAttackFlag = true;
                yield return StartCoroutine(enemyAttackBehavior.PerformAttack());
                TutorialAttackFlag = false;
                yield return new WaitForSeconds(2);
            }
        }
        Debug.Log("Waiting for audio to finish");
        PlayNextClip();
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
            audioSource.PlayOneShot(boxingbell);
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

