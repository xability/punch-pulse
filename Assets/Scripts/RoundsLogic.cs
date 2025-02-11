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
    public bool isBreakOngoing = false;

    public BoxingRingMapping ringMapping;
    public Transform enemyTransform; // Reference to the enemy's transform
    public Transform playerTransform;
    private Vector3 initialEnemyPosition; // To store the initial position
    private Quaternion initialEnemyRotation; // To store the initial rotation
    public AudioClip difficultIncreased;
    public AudioClip boxingBellStart;


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

        isBreakOngoing = false;
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


        // Play round start audio
        yield return StartCoroutine(PlayRoundStartAudio(roundNumber));

        Debug.Log(roundName + " has started.");

        // Wait for the round duration
        yield return new WaitForSeconds(roundDuration);

        Debug.Log(roundName + " ended.");

        isBreakOngoing = true;
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

        isBreakOngoing = false;
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

    public Vector3 GenerateRandomPositionInRing()
    {
        if (ringMapping == null || enemyTransform == null || playerTransform == null)
        {
            Debug.LogWarning("RingMapping, EnemyTransform, or PlayerTransform not assigned!");
            return Vector3.zero;
        }

        // Generate a random position within the ring
        float randomX = Random.Range(-ringMapping.xLong / 2f, ringMapping.xLong / 2f);
        float randomZ = Random.Range(-ringMapping.yWide / 2f, ringMapping.yWide / 2f);
        Vector3 randomPosition = new Vector3(randomX, enemyTransform.position.y, randomZ);

        // Move the enemy to the random position
        enemyTransform.position = randomPosition;

        // Make the enemy face the player
        Vector3 directionToPlayer = playerTransform.position - enemyTransform.position;
        directionToPlayer.y = 0; // Ignore vertical difference
        Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);
        enemyTransform.rotation = rotationToPlayer;

        Debug.Log("Enemy moved to random position: " + randomPosition);

        return randomPosition;
    }

}

