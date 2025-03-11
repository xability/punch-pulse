using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class RoundsManager : MonoBehaviour
{
    [Header("Round Settings")]
    public float warmUpDuration = 300f; // 5 minutes for warm-up
    public float roundDuration = 600f; // 10 minutes for each main round
    public int totalRounds = 3; // 3 main rounds after warm-up

    [Header("Audio Clips")]
    public AudioClip warmUpStartAudio;
    public AudioClip[] warmUpExercises; // Array of warm-up exercise instructions
    public AudioClip warmUpEndAudio;
    public AudioClip[] roundStartAudios;
    public AudioClip roundEndAudio;
    public AudioClip roundBreakAudio;
    public AudioClip[] endOfRoundAudios;
    public AudioClip difficultIncreased;
    public AudioClip gameOverAudio;
    public AudioClip boxingBellStart;

    [Header("References")]
    public AudioSource audioSource;
    public GameObject gameOverUI;
    public ScoreManager scoreManager;
    public GameModuleManager gameModuleManager;
    public bool isRoundOngoing = false;
    public int RoundNumber { get; private set; } = 0;
    public BoxingRingMapping ringMapping;
    public Transform enemyTransform;
    public Transform playerTransform;
    public Camera playerCamera;
    public AccessibleMenu menu;
    public bool skipWarmupRound = false;

    [Header("Panels for Results")]
    public GameObject panelRound1;
    public GameObject panelRound2;
    public GameObject panelRound3;

    [Header("Text Fields for Results")]
    public TextMeshProUGUI round1LTCountText;
    public TextMeshProUGUI round1RTCountText;
    public TextMeshProUGUI round1DuckCountText;
    public TextMeshProUGUI round1PlayerHitsText;
    public TextMeshProUGUI round1HeadPunchesText;
    public TextMeshProUGUI round1BodyPunchesText;
    public TextMeshProUGUI round1PlayerScoreText;
    public TextMeshProUGUI round1EnemyScoreText;

    public TextMeshProUGUI round2LTCountText;
    public TextMeshProUGUI round2RTCountText;
    public TextMeshProUGUI round2DuckCountText;
    public TextMeshProUGUI round2PlayerHitsText;
    public TextMeshProUGUI round2HeadPunchesText;
    public TextMeshProUGUI round2BodyPunchesText;
    public TextMeshProUGUI round2PlayerScoreText;
    public TextMeshProUGUI round2EnemyScoreText;

    public TextMeshProUGUI round3LTCountText;
    public TextMeshProUGUI round3RTCountText;
    public TextMeshProUGUI round3DuckCountText;
    public TextMeshProUGUI round3PlayerHitsText;
    public TextMeshProUGUI round3HeadPunchesText;
    public TextMeshProUGUI round3BodyPunchesText;
    public TextMeshProUGUI round3PlayerScoreText;
    public TextMeshProUGUI round3EnemyScoreText;


    [System.Serializable]
    public struct RoundData
    {
        public int roundNumber;
        public int leftTriggerCount;
        public int rightTriggerCount;
        public int duckCount;
        public int playerHitCount;
        public int playerHeadPunchCount;
        public int playerBodyPunchCount;
        public int playerScore;
        public int enemyScore;
    }

    public List<RoundData> roundDataList = new List<RoundData>();

    // Variables to store current stat values for each round, round1, round2 , round3...
    private int currentLTCount;
    private int currentRTCount;
    private int currentDuckCount;
    private int currentPlayerHitCount;
    private int currentPlayerHeadPunchCount;
    private int currentPlayerBodyPunchCount;

    [Header("Input")]
    public InputActionReference nextStepAction;

    private Vector3 initialEnemyPosition;
    private Quaternion initialEnemyRotation;

    void Start()
    {
        if (enemyTransform != null)
        {
            initialEnemyPosition = enemyTransform.position;
            initialEnemyRotation = enemyTransform.rotation;
        }
        else
        {
            Debug.LogWarning("Enemy transform not assigned in RoundsManager!");
        }

        if (gameModuleManager == null)
        {
            Debug.LogError("GameModuleManager reference not set in the Inspector for RoundsManager!");
        }


    }

    public void BeginRounds()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        StartCoroutine(HandleAllGameModes());
    }

    private IEnumerator HandleAllGameModes()
    {
        // Warm-up round
        ResetEnemyPosition();
        if (!skipWarmupRound)
        {
            yield return StartCoroutine(HandleWarmUpRound());
        }

        // Main rounds
        for (int roundIndex = 1; roundIndex <= totalRounds; roundIndex++)
        {
            RoundNumber += 1;
            Debug.Log("Round number : " + RoundNumber);

            
            yield return StartCoroutine(HandleMainRound(roundIndex));
            ResetEnemyPosition();
            yield return RoundBreak();
        }

        ShowGameOver();
    }

    private IEnumerator HandleWarmUpRound()
    {
        ScoreManager.ResetScores();
        DirectionHelper.SetTriggerPressCount(0);
        MoveEnemyInFront.SetRightTriggerPressCount(0);
        EnemyAttackBehavior.SetPlayerDuckCount(0);
        EnemyAttackBehavior.SetPlayerHitCount(0);
        PlayAudioOnBoxing.SetPlayerHeadPunchCount(0);
        PlayAudioOnBoxing.SetPlayerBodyPunchCount(0);

        Debug.Log("Warm-Up round started.");
        isRoundOngoing = false;
        AccessibleMenu.IsOffensiveMode = false;

        // Play warm-up start audio
        if (audioSource != null && warmUpStartAudio != null)
        {
            audioSource.PlayOneShot(warmUpStartAudio);
            yield return new WaitForSeconds(warmUpStartAudio.length);
        }

        // Play warm-up exercises
        for (int i = 0; i < warmUpExercises.Length; i++)
        {
            if (audioSource != null && warmUpExercises[i] != null)
            {
                audioSource.PlayOneShot(warmUpExercises[i]);
                yield return new WaitForSeconds(warmUpExercises[i].length);

                // Wait for user input to continue
                yield return StartCoroutine(WaitForUserInput());
            }
        }

        // 2-minute familiarization period
        Debug.Log("2-minute familiarization period started.");
        audioSource.PlayOneShot(boxingBellStart);
        yield return new WaitForSeconds(boxingBellStart.length);

        yield return new WaitForSeconds(120f);

        Debug.Log("Warm-Up round ended.");
        audioSource.PlayOneShot(warmUpEndAudio);
        yield return new WaitForSeconds(warmUpEndAudio.length);

        audioSource.PlayOneShot(roundBreakAudio);
        yield return new WaitForSeconds(roundBreakAudio.length);
        Debug.Log("Round break. Duration: 60 seconds.");
        yield return new WaitForSeconds(60f);
    }


    private IEnumerator HandleMainRound(int roundNumber)
    {


        // Set difficulty
        yield return StartCoroutine(SetDifficultyForRound(roundNumber));

        // Play round start audio
        yield return StartCoroutine(PlayRoundStartAudio(roundNumber));

        Debug.Log($"Round {roundNumber} has started.");
        isRoundOngoing = true;

        // add a delay here if needed for the player to get ready

        AccessibleMenu.IsOffensiveMode = true;
        // Wait for the round duration
        yield return new WaitForSeconds(roundDuration);

        Debug.Log($"Round {roundNumber} ended.");

        isRoundOngoing = false;
        AccessibleMenu.IsOffensiveMode = false;

        // Play round end audio
        if (audioSource != null && roundEndAudio != null)
        {
            audioSource.PlayOneShot(roundEndAudio);
            yield return new WaitForSeconds(roundEndAudio.length);
        }

        yield return StartCoroutine(PlayEndOfRoundAudios());
    }

    private IEnumerator WaitForUserInput()
    {
        if (nextStepAction == null)
        {
            Debug.LogError("Next Step Action is not assigned!");
            yield break;
        }

        nextStepAction.action.Enable();
        bool inputReceived = false;

        nextStepAction.action.performed += ctx => inputReceived = true;

        while (!inputReceived)
        {
            yield return null;
        }

        nextStepAction.action.performed -= ctx => inputReceived = true;
        nextStepAction.action.Disable();
    }


    private IEnumerator SetDifficultyForRound(int roundNumber)
    {
        AccessibleMenu.DifficultyLevel difficulty = AccessibleMenu.DifficultyLevel.Easy;

        Debug.Log($"Setting difficulty for round {roundNumber}...");
        switch (roundNumber)
        {
            case 1:
                difficulty = AccessibleMenu.DifficultyLevel.Medium;
                break;
            case 2:
                difficulty = AccessibleMenu.DifficultyLevel.Hard;
                break;
            case 3:
                difficulty = AccessibleMenu.DifficultyLevel.UltraHard;
                break;
            default:
                Debug.LogWarning($"Unexpected round number: {roundNumber}. Defaulting to Easy difficulty.");
                break;
        }

        AccessibleMenu.SetDifficulty(difficulty);
        Debug.Log($"Difficulty set to {difficulty} for round {roundNumber}");

        if (audioSource != null && difficultIncreased != null)
        {
            if (difficulty != AccessibleMenu.DifficultyLevel.Medium)
            {
                audioSource.PlayOneShot(difficultIncreased);
                yield return new WaitForSeconds(difficultIncreased.length);
            }
        }
    }

    private IEnumerator PlayRoundStartAudio(int roundNumber)
    {
        if (audioSource != null && roundNumber > 0 && roundNumber <= roundStartAudios.Length)
        {
            AudioClip clip = roundStartAudios[roundNumber - 1];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            audioSource.PlayOneShot(boxingBellStart);
            yield return new WaitForSeconds(boxingBellStart.length);
        }
    }

    private IEnumerator PlayEndOfRoundAudios()
    {
        if (scoreManager != null)
        {
            yield return StartCoroutine(scoreManager.AnnouncePlayerScore());
            yield return StartCoroutine(scoreManager.AnnounceEnemyScore());
        }
        Debug.Log(" Round number : " + RoundNumber);
        if (RoundNumber == 3)
        {
            audioSource.PlayOneShot(gameOverAudio);
            yield return new WaitForSeconds(gameOverAudio.length);
        }
        else
        {
            audioSource.PlayOneShot(roundBreakAudio);
            yield return new WaitForSeconds(roundBreakAudio.length);
        }

    }

    private IEnumerator RoundBreak()
    {
        Debug.Log("Round break. Duration: 60 seconds.");
        if (RoundNumber < 3)
            yield return new WaitForSeconds(60f);

        // Fetch the stats
        RoundData currentRoundData = new RoundData
        {
            roundNumber = RoundNumber,
            leftTriggerCount = DirectionHelper.GetLeftTriggerPressCount(),
            rightTriggerCount = MoveEnemyInFront.GetRightTriggerPressCount(),
            duckCount = EnemyAttackBehavior.GetPlayerDuckCount(),
            playerHitCount = EnemyAttackBehavior.GetPlayerHitCount(),
            playerHeadPunchCount = PlayAudioOnBoxing.GetPlayerHeadPunchCount(),
            playerBodyPunchCount = PlayAudioOnBoxing.GetPlayerBodyPunchCount(),
            playerScore = ScoreManager.Score,
            enemyScore = ScoreManager.EnemyScore
        };

        // Save the stats for this round in an object
        roundDataList.Add(currentRoundData);

        // Reset the stats for the next round
        ScoreManager.ResetScores();
        DirectionHelper.SetTriggerPressCount(0);
        MoveEnemyInFront.SetRightTriggerPressCount(0);
        EnemyAttackBehavior.SetPlayerDuckCount(0);
        EnemyAttackBehavior.SetPlayerHitCount(0);
        PlayAudioOnBoxing.SetPlayerHeadPunchCount(0);
        PlayAudioOnBoxing.SetPlayerBodyPunchCount(0);
        ScoreManager.ResetScores();
    }

    private void ShowGameOver()
    {
        Debug.Log("All rounds completed. Game Over.");

        // Activate panels
        if (panelRound1 != null) panelRound1.SetActive(true);
        if (panelRound2 != null) panelRound2.SetActive(true);
        if (panelRound3 != null) panelRound3.SetActive(true);

        // Populate data for Round 1
        RoundData round1Data = GetRoundData(1);
        if (round1Data.roundNumber == 1)
        {
            round1LTCountText.text = "LT Count: " + round1Data.leftTriggerCount.ToString();
            round1RTCountText.text = "RT Count: " + round1Data.rightTriggerCount.ToString();
            round1DuckCountText.text = "Ducks: " + round1Data.duckCount.ToString();
            round1PlayerHitsText.text = "Player Hits: " + round1Data.playerHitCount.ToString();
            round1HeadPunchesText.text = "Head Punches: " + round1Data.playerHeadPunchCount.ToString();
            round1BodyPunchesText.text = "Body Punches: " + round1Data.playerBodyPunchCount.ToString();
            round1PlayerScoreText.text = "Player Score: " + round1Data.playerScore.ToString();
            round1EnemyScoreText.text = "Enemy Score: " + round1Data.enemyScore.ToString();
        }

        // Populate data for Round 2
        RoundData round2Data = GetRoundData(2);
        if (round2Data.roundNumber == 2)
        {
            round2LTCountText.text = "LT Count: " + round2Data.leftTriggerCount.ToString();
            round2RTCountText.text = "RT Count: " + round2Data.rightTriggerCount.ToString();
            round2DuckCountText.text = "Ducks: " + round2Data.duckCount.ToString();
            round2PlayerHitsText.text = "Player Hits: " + round2Data.playerHitCount.ToString();
            round2HeadPunchesText.text = "Head Punches: " + round2Data.playerHeadPunchCount.ToString();
            round2BodyPunchesText.text = "Body Punches: " + round2Data.playerBodyPunchCount.ToString();
            round2PlayerScoreText.text = "Player Score: " + round2Data.playerScore.ToString();
            round2EnemyScoreText.text = "Enemy Score: " + round2Data.enemyScore.ToString();
        }

        // Populate data for Round 3
        RoundData round3Data = GetRoundData(3);
        if (round3Data.roundNumber == 3)
        {
            round3LTCountText.text = "LT Count: " + round3Data.leftTriggerCount.ToString();
            round3RTCountText.text = "RT Count: " + round3Data.rightTriggerCount.ToString();
            round3DuckCountText.text = "Ducks: " + round3Data.duckCount.ToString();
            round3PlayerHitsText.text = "Player Hits: " + round3Data.playerHitCount.ToString();
            round3HeadPunchesText.text = "Head Punches: " + round3Data.playerHeadPunchCount.ToString();
            round3BodyPunchesText.text = "Body Punches: " + round3Data.playerBodyPunchCount.ToString();
            round3PlayerScoreText.text = "Player Score: " + round3Data.playerScore.ToString();
            round3EnemyScoreText.text = "Enemy Score: " + round3Data.enemyScore.ToString();
        }
    }

    public void ResetEnemyPosition()
    {
        if (enemyTransform != null)
        {
            enemyTransform.position = initialEnemyPosition;
            enemyTransform.rotation = initialEnemyRotation;
            Debug.Log("Enemy position reset to initial position");
        }
        else
        {
            Debug.LogWarning("Cannot reset enemy position: Enemy transform not assigned!");
        }
    }

    private Vector3 GetRandomPositionInRing()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player Camera is not assigned!");
            return transform.position;
        }

        Debug.Log("Attack done, finding new position to move to");
        // Get a random distance within the specified range
        float randomDistance = 2f;

        // Calculate a random angle within a 180-degree arc in front of the player
        float randomAngle = Random.Range(-180f, 180f);

        // Calculate the forward direction of the player, ignoring vertical rotation
        Vector3 playerForward = playerCamera.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        // Rotate the forward vector by the random angle
        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * playerForward;

        // Calculate the new position
        Vector3 newPosition = playerCamera.transform.position + randomDirection * randomDistance;
        newPosition.y = initialEnemyPosition.y; // Keep the enemy's y-coordinate fixed

        // Check if the new position is within the specified rectangle
        if (newPosition.x < -3f || newPosition.x > 3f || newPosition.z < -3f || newPosition.z > 3f)
        {
            // If outside the rectangle, reset to the specified position
            newPosition = new Vector3(0.17f, 0.9f, 1.3f);
            Debug.Log("Position outside bounds. Reset to: " + newPosition);
        }
        else
        {
            Debug.Log("New position: " + newPosition);
        }

        return newPosition;
    }

    public void TeleportEnemyPositionSurvivalMode()
    {
        if (enemyTransform == null)
        {
            Debug.LogWarning("Cannot reset enemy position: Enemy transform not assigned!");
            return;
        }

        Vector3 newPosition = GetRandomPositionInRing();

        // Teleport the enemy to the new position
        enemyTransform.position = newPosition;

        // Calculate the direction from the enemy to the player
        Vector3 directionToPlayer = playerCamera.transform.position - newPosition;
        directionToPlayer.y = 0; // Ignore vertical difference

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(directionToPlayer);
            enemyTransform.rotation = newRotation;
        }

        Debug.Log("Enemy teleported to: " + newPosition + " and is facing the player.");
    }

    public RoundData GetRoundData(int roundNumber)
    {
        return roundDataList.Find(data => data.roundNumber == roundNumber);
    }

}

