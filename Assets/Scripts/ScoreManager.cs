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
    public AudioClip enemyScoreIs;
    public AudioClip moreThan100;
    public static int Score { get; private set; }
    public TextMeshProUGUI scoreText;
    public static int EnemyScore { get; private set; }
    public TextMeshProUGUI enemyScoreText;
    public InputActionReference rightButtonAction;
    public RoundsManager roundsManager;


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
        UpdateEnemyScoreDisplay();

        if (roundsManager == null)
        {
            Debug.LogWarning("RoundsManager not assigned in the inspector!");
        }

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
        /*        
        else if (Score <= 5 && AccessibleMenu.CurrentDifficulty == AccessibleMenu.DifficultyLevel.Medium)
        {
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Easy);
            //audioSource.PlayOneShot(difficultyDecreaseSound);
        }
        else if (Score <= 20 && AccessibleMenu.CurrentDifficulty == AccessibleMenu.DifficultyLevel.Hard)
        {
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Medium);
            //audioSource.PlayOneShot(difficultyDecreaseSound);
        }*/
    }

    /*    
    public static void AddScore(int amount)
    {
        
            // Debug.Log("Round is ongoing");
            // Perform round-specific actions
            Score += amount;
            Instance.UpdateScoreDisplay();
        

        //CheckAndUpdateDifficulty();
    }

    public static void DecrementScore(int amount)
    {
        
            // Debug.Log("Round is ongoing");
            Score = Mathf.Max(0, Score - amount);
            Instance.UpdateScoreDisplay();
        

        //CheckAndUpdateDifficulty();
    }

    public static void AddEnemyScore(int amount)
    {
         // Debug.Log("Round is ongoing");
            EnemyScore += amount;
            Instance.UpdateEnemyScoreDisplay();
        
    }*/

    public static void AddScore(int amount)
    {
        if (Instance.roundsManager != null && Instance.roundsManager.isRoundOngoing)
        {
            Score += amount;
            Instance.UpdateScoreDisplay();
        } 
        else if (Instance.roundsManager == null)
        {
            Debug.LogWarning("RoundsManager not assigned in the inspector!");
        }
    }

    public static void DecrementScore(int amount)
    {
        if (Instance.roundsManager != null && Instance.roundsManager.isRoundOngoing)
        {
            Score = Mathf.Max(0, Score - amount);
            Instance.UpdateScoreDisplay();
        }
        else if(Instance.roundsManager == null)
        {
            Debug.LogWarning("RoundsManager not assigned in the inspector!");

        }
    }

    public static void AddEnemyScore(int amount)
    {
        if (Instance.roundsManager != null && Instance.roundsManager.isRoundOngoing)
        {
            EnemyScore += amount;
            Instance.UpdateEnemyScoreDisplay();
        }
        else if(Instance.roundsManager == null)
        {
            Debug.LogWarning("RoundsManager not assigned in the inspector!");
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }
    }

    private void UpdateEnemyScoreDisplay()
    {
        if (enemyScoreText != null)
        {
            enemyScoreText.text = EnemyScore.ToString();
        }
    }

    public IEnumerator AnnounceScore()
    {
        if (isAnnouncingScore) yield break; // Safety check

        isAnnouncingScore = true;

        audioSource.PlayOneShot(playerScoreIs);
        yield return new WaitForSeconds(playerScoreIs.length);

        string scoreString = Score.ToString();
        if (Score < 101)
        {
            Debug.Log("Score: " + Score);
            audioSource.PlayOneShot(numberClips[Score]);
            yield return new WaitForSeconds(numberClips[Score].length);
        }
        else
        {
            Debug.LogWarning("Number audio clip not found for digit: " + Score);
            audioSource.PlayOneShot(moreThan100);
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
    public IEnumerator AnnounceEnemyScore()
    {
        if (isAnnouncingScore) yield break; // Safety check

        isAnnouncingScore = true;

        audioSource.PlayOneShot(enemyScoreIs);
        yield return new WaitForSeconds(enemyScoreIs.length);

        string scoreString = EnemyScore.ToString();
        if (EnemyScore < 101)
        {
            Debug.Log("Enemy Score: " + EnemyScore);
            audioSource.PlayOneShot(numberClips[EnemyScore]);
            yield return new WaitForSeconds(numberClips[EnemyScore].length);
        }
        else
        {
            Debug.LogWarning("Number audio clip not found for digit: " + EnemyScore);
            audioSource.PlayOneShot(moreThan100);
        }
        isAnnouncingScore = false;
    }
}