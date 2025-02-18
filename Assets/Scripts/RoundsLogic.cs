using UnityEngine;
using UnityEngine.UI;    // If you need UI functionality
using UnityEngine.SceneManagement; // If you want to reload the scene at the end
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class RoundsManager : MonoBehaviour
{
    [Header("Round Settings")]
    [Tooltip("Duration of each round in seconds. For example, 120 = 2 minutes.")]
    public float roundDuration = 120f;

    [Tooltip("Duration of the break between rounds in seconds. For example, 30 = 30 seconds.")]
    public float roundBreakDuration = 30f;

    [Tooltip("Number of full rounds (excluding warm-up).")]
    public int totalRounds = 6;

    [Header("Round Start Audio Clips")]
    public AudioClip warmUpStartAudio;
    public AudioClip[] roundStartAudios; // Array for round 1 to 6 start audios

    [Header("Round End Audio Clips")]
    public AudioClip roundEndAudio;
    public AudioClip[] endOfRoundAudios; // Array for additional end-of-round audios

    [Header("References")]
    [Tooltip("Audio source to play the clips from (attach an AudioSource component to this GameObject or another).")]
    public AudioSource audioSource;

    [Tooltip("Game Over UI Canvas or Panel.")]
    public GameObject gameOverUI;
    public ScoreManager scoreManager;
    public GameModuleManager gameModuleManager;
    public bool isRoundOngoing = true;

    public BoxingRingMapping ringMapping;
    public Transform enemyTransform; // Reference to the enemy's transform
    public Transform playerTransform;
    private Vector3 initialEnemyPosition; // To store the initial position
    private Quaternion initialEnemyRotation; // To store the initial rotation
    public AudioClip difficultIncreased;
    public AudioClip boxingBellStart;
    public Camera playerCamera; // The player's camera (assign the main VR camera)


    void Start()
    {

        // Store the initial enemy position and rotation
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

    //  audioSource.PlayOneShot(boxingbell);
    // Call this method from the script controlling the tutorial once it is finished
    public void BeginRounds()
    {
        // Make sure the Game Over UI is hidden at the start
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        // Begin the coroutine that handles round logic
        StartCoroutine(RoundSequenceRoutine());
    }

    private IEnumerator RoundSequenceRoutine()
    {
        // Round 0: Warm-up round
        isRoundOngoing = true;
        yield return StartCoroutine(HandleOneRound("Warm-Up", 0));

        // Full rounds
        for (int roundIndex = 1; roundIndex <= totalRounds; roundIndex++)
        {
            yield return StartCoroutine(HandleOneRound("Round " + roundIndex, roundIndex));
        }

        // All rounds (including warm-up) are finished, show Game Over
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        // Display Game Over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Debug.Log("All rounds completed. Game Over.");
    }

    // Example method to restart the game (attach to a UI button if desired)
    public void RestartGame()
    {
        // Example: Reload the active scene

    }

    private IEnumerator HandleOneRound(string roundName, int roundNumber)
    {

        isRoundOngoing = true;
        AccessibleMenu.IsOffensiveMode = true;

        // Check if the game mode is Level Progression
        if (gameModuleManager.IsLevelProgressionMode)
        {
            // Set difficulty based on round number only in Level Progression mode
            if (roundNumber == 3) // Third round (index 2 + 1)
            {
                AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Medium);
                Debug.Log("Difficulty set to Medium");
                audioSource.PlayOneShot(difficultIncreased);
                yield return new WaitForSeconds(difficultIncreased.length);
            }
            else if (roundNumber == 5) // Fifth round (index 4 + 1)
            {
                AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.Hard);
                Debug.Log("Difficulty set to Hard");
                audioSource.PlayOneShot(difficultIncreased);
                yield return new WaitForSeconds(difficultIncreased.length);
            }
        }
        else if (gameModuleManager.IsHardSurvivalMode)
        {
            // Teleport the enemy to a random position at the start of each survival round
            AccessibleMenu.SetDifficulty(AccessibleMenu.DifficultyLevel.UltraHard);
            //Debug.Log("Difficulty set to Hard for Survival mode");
        }


        // Play round start audio
        yield return StartCoroutine(PlayRoundStartAudio(roundNumber));

        Debug.Log(roundName + " has started.");

        // Wait for the round duration
        yield return new WaitForSeconds(roundDuration);

        Debug.Log(roundName + " ended.");

        isRoundOngoing = false;
        AccessibleMenu.IsOffensiveMode = false;

        // Play round end audio
        if (audioSource != null && roundEndAudio != null)
        {
            audioSource.PlayOneShot(roundEndAudio);

            yield return new WaitForSeconds(roundEndAudio.length);
        }

        bool isLastRound = (roundNumber != 0 && roundNumber == totalRounds);
        if (!isLastRound)
        {
            Debug.Log("Break between rounds. Duration: " + roundBreakDuration + " seconds.");

            ResetEnemyPosition();

            // Play additional end-of-round audio clips during the break
            StartCoroutine(PlayEndOfRoundAudiosWithTimeout(roundBreakDuration));

            yield return StartCoroutine(PlayEndOfRoundAudios());
        }
        else
        {
            // For the last round, play end-of-round audios without a time limit
            yield return StartCoroutine(PlayEndOfRoundAudios());
        }
    }

    private IEnumerator PlayRoundStartAudio(int roundNumber)
    {
        if (audioSource != null)
        {
            if (roundNumber == 0 && warmUpStartAudio != null)
            {
                audioSource.PlayOneShot(warmUpStartAudio);
                yield return new WaitForSeconds(warmUpStartAudio.length);
            }
            else if (roundNumber > 0 && roundNumber <= roundStartAudios.Length)
            {
                AudioClip clip = roundStartAudios[roundNumber - 1];
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                    yield return new WaitForSeconds(clip.length);
                }
            }

            audioSource.PlayOneShot(boxingBellStart);
            yield return new WaitForSeconds(boxingBellStart.length);
        }
    }

    private IEnumerator PlayEndOfRoundAudios()
    {
        // Announce player score
        if (scoreManager != null)
        {
            yield return StartCoroutine(scoreManager.AnnounceScore());
        }

        // Announce enemy score
        if (scoreManager != null)
        {
            yield return StartCoroutine(scoreManager.AnnounceEnemyScore());
        }
    }

    private IEnumerator PlayEndOfRoundAudiosWithTimeout(float timeout)
    {
        float elapsedTime = 0f;

        foreach (AudioClip clip in endOfRoundAudios)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
                float clipDuration = Mathf.Min(clip.length, timeout - elapsedTime);
                yield return new WaitForSeconds(clipDuration);

                elapsedTime += clipDuration;
                if (elapsedTime >= timeout)
                    break;
            }
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
}

