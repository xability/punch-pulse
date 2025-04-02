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
    public AudioClip Round;
    public AudioClip moreThan100;
    public static int Score { get; private set; }
    public TextMeshProUGUI scoreText;
    public static int EnemyScore { get; private set; }
    public int RoundNum = 1;
    public TextMeshProUGUI enemyScoreText;
    public InputActionReference rightButtonAction;
    public RoundsManager roundsManager;
    

    private bool isAnnouncing = false;

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
        if (!isAnnouncing)
        {
            StartCoroutine(AnnounceAllInfo());
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

    public static void ResetScores()
    {
        // Reset the scores on every game mode break
        Score = 0;
        EnemyScore = 0;
        Instance.UpdateScoreDisplay();
        Instance.UpdateEnemyScoreDisplay();
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


    private IEnumerator AnnounceAllInfo()
    {
        isAnnouncing = true;

        yield return StartCoroutine(AnnounceRound());
        yield return StartCoroutine(AnnouncePlayerScore());
        yield return StartCoroutine(AnnounceEnemyScore());

        isAnnouncing = false;
    }

    public IEnumerator AnnounceRound()
    {
        audioSource.PlayOneShot(Round);
        yield return new WaitForSeconds(Round.length);
        RoundNum = roundsManager.RoundNumber;
        audioSource.PlayOneShot(numberClips[RoundNum]);
        yield return new WaitForSeconds(numberClips[RoundNum].length);
        
    }

    public IEnumerator AnnouncePlayerScore()
    {
        audioSource.PlayOneShot(playerScoreIs);
        yield return new WaitForSeconds(playerScoreIs.length);

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
            yield return new WaitForSeconds(moreThan100.length);
        }
    }

    public IEnumerator AnnounceEnemyScore()
    {
        audioSource.PlayOneShot(enemyScoreIs);
        yield return new WaitForSeconds(enemyScoreIs.length);

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
            yield return new WaitForSeconds(moreThan100.length);
        }
    }

}