using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TutorialManagerPrev : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI stepcount;
    public InputActionReference nextButtonAction;
    public InputActionReference exitTutorialAction;
    public AudioSource audioSource;
    public bool isTutorial = true;

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

    private int currentStep = 0;
    private int currentClip = 0;
    private bool waitingForAction = false;
    private bool tutorialStarted = false;
    private bool isAudioPlaying = false;

    public EnemyAttackBehavior enemyAttackBehavior;

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

            // Check if it's step 3 and the specific clips are about to play
            if (step.StepNum == 3 && (step.narration[currentClip].name == "08_duck_1" || step.narration[currentClip].name == "09_duck_2"))
            {
                StartCoroutine(SimulateEnemyAttack());
            }
        }
        else
        {
            NextStep();
        }
    }

    IEnumerator SimulateEnemyAttack()
    {
        yield return new WaitForSeconds(audioSource.clip.length); // Wait for the audio to finish
        yield return StartCoroutine(enemyAttackBehavior.PerformAttack());
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
            PlayNextClip();
        }
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
        if (isTutorial)
        {
            isTutorial = false;
            SceneManager.LoadScene("BoxingRing");
        }
    }

    void OnDisable()
    {
        nextButtonAction.action.performed -= OnNextButtonPressed;
        exitTutorialAction.action.performed -= ExitTutorial;
    }
}

