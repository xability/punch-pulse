using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public Button nextButton;
    public Button previousButton;
    public Button exitTutorialButton;
    public AudioSource audioSource;

    [System.Serializable]
    public class TutorialStep
    {
        public string instruction;
        public AudioClip narration;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public UnityEngine.Events.UnityEvent customAction;
    }

    public TutorialStep[] tutorialSteps;

    private int currentStep = 0;

    void Start()
    {
        UpdateTutorialStep();
        nextButton.onClick.AddListener(NextStep);
        previousButton.onClick.AddListener(PreviousStep);
        exitTutorialButton.onClick.AddListener(ExitTutorial);
    }

    void UpdateTutorialStep()
    {
        // Deactivate all objects from the previous step
        if (currentStep > 0)
        {
            foreach (var obj in tutorialSteps[currentStep - 1].objectsToDeactivate)
            {
                obj.SetActive(false);
            }
        }

        var step = tutorialSteps[currentStep];

        // Update instruction text
        instructionText.text = step.instruction;

        // Play narration
        if (step.narration != null)
        {
            audioSource.clip = step.narration;
            audioSource.Play();
        }

        // Activate/deactivate objects
        foreach (var obj in step.objectsToActivate)
        {
            obj.SetActive(true);
        }
        foreach (var obj in step.objectsToDeactivate)
        {
            obj.SetActive(false);
        }

        // Invoke custom action
        step.customAction?.Invoke();

        // Update button interactability
        previousButton.interactable = (currentStep > 0);
        nextButton.interactable = (currentStep < tutorialSteps.Length - 1);
    }

    void NextStep()
    {
        if (currentStep < tutorialSteps.Length - 1)
        {
            currentStep++;
            UpdateTutorialStep();
        }
    }

    void PreviousStep()
    {
        if (currentStep > 0)
        {
            currentStep--;
            UpdateTutorialStep();
        }
    }

    void ExitTutorial()
    {
        SceneManager.LoadScene("BoxingRing"); // Replace with your main scene name
    }
}

