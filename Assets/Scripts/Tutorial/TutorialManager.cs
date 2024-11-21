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
    public bool isTutorial = true;

    [System.Serializable]
    public class TutorialStep
    {
        public string instruction;
        public AudioClip[] narration;
        public int StepNum;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public UnityEngine.Events.UnityEvent customAction;
    }

    public TutorialStep[] tutorialSteps;

    private int currentStep = 0;
    private bool canProceed = true;

    void Start()
    {
        UpdateTutorialStep();
        nextButtonAction.action.performed += OnNextButtonPressed;
        exitTutorialAction.action.performed += ExitTutorial;
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

        step.customAction?.Invoke();

        StartCoroutine(PlayNarrationClips(step.narration));
    }

    private IEnumerator PlayNarrationClips(AudioClip[] clips)
    {
        canProceed = false;

        foreach (var clip in clips)
        {
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }

        canProceed = true;
    }

    private void OnNextButtonPressed(InputAction.CallbackContext context)
    {
        if (canProceed)
            NextStep();
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

