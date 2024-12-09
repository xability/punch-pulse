using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public AudioClip[] numberClips;
    public AudioSource audioSource;
    public AudioClip playerScoreIs;

    public static int Score { get; private set; }
    public TextMeshProUGUI scoreText;
    public InputActionReference rightButtonAction;


    private bool isAnnouncingScore = false; // New flag to track if score is being announced

    private void OnEnable()
    {
        rightButtonAction.action.performed += OnRightButtonPressed;
    }

    private void OnDisable()
    {
        rightButtonAction.action.performed -= OnRightButtonPressed;
    }

    private void OnRightButtonPressed(InputAction.CallbackContext context)
    {
        if (!isAnnouncingScore) // Only start if not already announcing
        {
            StartCoroutine(AnnounceScore());
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreDisplay();
    }

    private static void CheckAndUpdateDifficulty()
    {
        if (Score >= 40 && AccessibleMenu.CurrentDifficulty != AccessibleMenu.DifficultyLevel.Hard)
        {
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Hard);
            //audioSource.PlayOneShot(difficultyIncreaseSound);
        }
        else if (Score >= 20 && AccessibleMenu.CurrentDifficulty == AccessibleMenu.DifficultyLevel.Easy)
        {
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Medium);
            //audioSource.PlayOneShot(difficultyIncreaseSound);
        }
        else if (Score <= 5 && AccessibleMenu.CurrentDifficulty == AccessibleMenu.DifficultyLevel.Medium)
        {
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Easy);
            //audioSource.PlayOneShot(difficultyDecreaseSound);
        }
        else if (Score <= 20 && AccessibleMenu.CurrentDifficulty == AccessibleMenu.DifficultyLevel.Hard)
        {
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Medium);
            //audioSource.PlayOneShot(difficultyDecreaseSound);
        }
    }

    public static void AddScore(int amount)
    {
        Score += amount;
        Instance.UpdateScoreDisplay();
        CheckAndUpdateDifficulty();
    }

    public static void DecrementScore(int amount)
    {
        Score = Mathf.Max(0, Score - amount);
        Instance.UpdateScoreDisplay();
        CheckAndUpdateDifficulty();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }
    }

    private IEnumerator AnnounceScore()
    {
        if (isAnnouncingScore) yield break; // Safety check

        isAnnouncingScore = true;

        audioSource.PlayOneShot(playerScoreIs);
        yield return new WaitForSeconds(playerScoreIs.length);

        string scoreString = Score.ToString();
        if (Score < 61)
        {
            Debug.Log("Score: " + Score);
            audioSource.PlayOneShot(numberClips[Score]);
            yield return new WaitForSeconds(numberClips[Score].length);
        }
        else
        {
            Debug.LogWarning("Number audio clip not found for digit: " + Score);  
        }

        /*
        foreach (char digitChar in scoreString)
        {
            int digit = digitChar - '0';

            if (digit >= 0 && digit < numberClips.Length)
            {
                audioSource.PlayOneShot(numberClips[digit]);
                yield return new WaitForSeconds(numberClips[digit].length);
            }
            else
            {
                Debug.LogWarning("Number audio clip not found for digit: " + digit);
                break;
            }
        }*/

        isAnnouncingScore = false;
    }
}