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
        yield return StartCoroutine(HandleWarmUpRound());

        // Main rounds
        for (int roundIndex = 1; roundIndex <= totalRounds; roundIndex++)
        {
            RoundNumber += 1;
            Debug.Log("Round number : " + RoundNumber);


            yield return StartCoroutine(HandleMainRound(roundIndex));
            if (roundIndex < totalRounds)
            {
                yield return RoundBreak();
            }
        }

        ShowGameOver();
    }

    private IEnumerator HandleWarmUpRound()
    {
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
    }


    private IEnumerator HandleMainRound(int roundNumber)
    {
        isRoundOngoing = true;
        AccessibleMenu.IsOffensiveMode = true;

        // Set difficulty
        yield return StartCoroutine(SetDifficultyForRound(roundNumber));

        // Play round start audio
        yield return StartCoroutine(PlayRoundStartAudio(roundNumber));

        Debug.Log($"Round {roundNumber} has started.");

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
            audioSource.PlayOneShot(difficultIncreased);
            yield return new WaitForSeconds(difficultIncreased.length);
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
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);

            // Create three panels for each round
            for (int i = 0; i < 3; i++)
            {
                GameObject panel = CreateStatsPanel(i + 1);
                PositionPanelInFrontOfPlayer(panel, i);
                PopulateStatsPanel(panel, i + 1);
            }
        }
        Debug.Log("All rounds completed. Game Over.");
    }

    private GameObject CreateStatsPanel(int roundNumber)
    {
        GameObject panel = new GameObject($"Round {roundNumber} Stats Panel");
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Semi-transparent dark background

        rectTransform.sizeDelta = new Vector2(0.5f, 0.7f); // Set size in meters
        panel.transform.SetParent(gameOverUI.transform, false);

        return panel;
    }

    private void PositionPanelInFrontOfPlayer(GameObject panel, int index)
    {
        float xOffset = (index - 1) * 0.6f; // Spread panels horizontally
        panel.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2f + playerCamera.transform.right * xOffset;
        panel.transform.rotation = playerCamera.transform.rotation;
    }

    private void PopulateStatsPanel(GameObject panel, int roundNumber)
    {
        RoundData data = GetRoundData(roundNumber);
        if (data.roundNumber == 0) return; // No data for this round

        string statsText = $"Round {roundNumber} Stats:\n\n" +
                           $"Left Trigger Count: {data.leftTriggerCount}\n" +
                           $"Right Trigger Count: {data.rightTriggerCount}\n" +
                           $"Duck Count: {data.duckCount}\n" +
                           $"Player Hit Count: {data.playerHitCount}\n" +
                           $"Head Punch Count: {data.playerHeadPunchCount}\n" +
                           $"Body Punch Count: {data.playerBodyPunchCount}\n" +
                           $"Player Score: {data.playerScore}\n" +
                           $"Enemy Score: {data.enemyScore}";

        GameObject textObject = new GameObject("Stats Text");
        textObject.transform.SetParent(panel.transform, false);

        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = statsText;
        tmpText.fontSize = 0.02f; // Adjust as needed
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;

        RectTransform textRectTransform = tmpText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;
        textRectTransform.anchoredPosition = Vector2.zero;
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

