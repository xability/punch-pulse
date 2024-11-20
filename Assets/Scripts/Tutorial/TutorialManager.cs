using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI instructionText; // --> title
    public TextMeshProUGUI stepcount; // Tutorial Step count
    public Button nextButton; // go to next step
    public Button previousButton; // go to previous step
    public Button exitTutorialButton; // start the game
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

    void Start()
    {
        UpdateTutorialStep();
        nextButton.onClick.AddListener(NextStep);
        previousButton.onClick.AddListener(PreviousStep);
        // exitTutorialButton.onClick.AddListener(ExitTutorial);
    }

    void UpdateTutorialStep()
    {

        var step = tutorialSteps[currentStep];

        // Update instruction text
        instructionText.text = step.instruction;

        // Play narration
        if (step.narration != null)
        {
            //audioSource.clip = step.narration;
            // add for loop to loop through audio clip array

            audioSource.Play();
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

