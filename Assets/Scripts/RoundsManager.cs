using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RoundsManager : MonoBehaviour
{
    [Header("Round Settings")]
    public float warmUpDuration = 300f; // 5 minutes for warm-up
    public float roundDuration = 600f; // 10 minutes for each main round
    public int totalRounds = 3; // 3 main rounds after warm-up

    [Header("Audio Clips")]
    public AudioClip warmUpStartAudio;
    public AudioClip[] warmUpExercises; // Array of warm-up exercise instructions
    public AudioClip[] roundStartAudios;
    public AudioClip roundEndAudio;
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
    public BoxingRingMapping ringMapping;
    public Transform enemyTransform;
    public Transform playerTransform;
    public Camera playerCamera;
    public AccessibleMenu menu;

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
            yield return StartCoroutine(HandleMainRound(roundIndex));
            if (roundIndex < totalRounds)
            {
                yield return RoundBreak();
            }
        }

        // Play game over audio
        if (audioSource != null && gameOverAudio != null)
        {
            audioSource.PlayOneShot(gameOverAudio);
            yield return new WaitForSeconds(gameOverAudio.length);
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
        yield return new WaitForSeconds(120f);

        Debug.Log("Warm-Up round ended.");
    }


    private IEnumerator HandleMainRound(int roundNumber)
    {
        isRoundOngoing = true;
        AccessibleMenu.IsOffensiveMode = true;

        // Set difficulty
        SetDifficultyForRound(roundNumber);

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


    private void SetDifficultyForRound(int roundNumber)
    {
        AccessibleMenu.DifficultyLevel difficulty = AccessibleMenu.DifficultyLevel.Easy;

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
        }

        AccessibleMenu.SetDifficulty(difficulty);
        Debug.Log($"Difficulty set to {difficulty} for round {roundNumber}");

        if (audioSource != null && difficultIncreased != null)
        {
            audioSource.PlayOneShot(difficultIncreased);
        }
    }

    private IEnumerator RoundBreak()
    {
        Debug.Log("Round break. Duration: 60 seconds.");
        yield return new WaitForSeconds(60f);
        AccessibleMenu.ResetLeftTriggerCount();
        ScoreManager.ResetScores();
    }

    private void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        Debug.Log("All rounds completed. Game Over.");
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
            yield return StartCoroutine(scoreManager.AnnounceScore());
            yield return StartCoroutine(scoreManager.AnnounceEnemyScore());
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

/*    public void GetGameStats(string currentGameMode)
    {
        
        GameStats stats = GameStatsManager.GetStats(currentGameMode);

        this.currentLTCount = stats.leftTriggerCount;
        currentRTCount = stats.rightTriggerCount;
        currentDuckCount = stats.duckCount;
        currentPlayerHitCount = stats.playerHitCount;
        currentPlayerHeadPunchCount = stats.playerHeadPunchCount;
        currentPlayerBodyPunchCount = stats.playerBodyPunchCount;
        gameModePlayerScore = stats.playerScore;
        gameModeEnemyScore = stats.enemyScore;

        Debug.Log($"Game Mode: {currentGameMode}");
        Debug.Log($"Left trigger has been pressed {currentLTCount} times.");
        // ... (log other stats)
    }*/


}

