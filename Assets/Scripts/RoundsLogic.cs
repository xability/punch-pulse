using UnityEngine;
using UnityEngine.UI;    // If you need UI functionality
using UnityEngine.SceneManagement; // If you want to reload the scene at the end

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


    public Transform enemyTransform; // Reference to the enemy's transform
    private Vector3 initialEnemyPosition; // To store the initial position
    private Quaternion initialEnemyRotation; // To store the initial rotation


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

    private System.Collections.IEnumerator RoundSequenceRoutine()
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

    private System.Collections.IEnumerator HandleOneRound(string roundName, int roundNumber)
    {

        AccessibleMenu.IsOffensiveMode = true;

        // Play round start audio
        PlayRoundStartAudio(roundNumber);

        Debug.Log(roundName + " has started.");

        // Wait for the round duration
        yield return new WaitForSeconds(roundDuration);

        Debug.Log(roundName + " ended.");

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

            yield return new WaitForSeconds(roundBreakDuration);
        }
        else
        {
            // For the last round, play end-of-round audios without a time limit
            yield return StartCoroutine(PlayEndOfRoundAudios());
        }
    }

    private void PlayRoundStartAudio(int roundNumber)
    {
        if (audioSource != null)
        {
            if (roundNumber == 0 && warmUpStartAudio != null)
            {
                audioSource.PlayOneShot(warmUpStartAudio);
            }
            else if (roundNumber > 0 && roundNumber <= roundStartAudios.Length)
            {
                AudioClip clip = roundStartAudios[roundNumber - 1];
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }
    }

    private System.Collections.IEnumerator PlayEndOfRoundAudios()
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

    private System.Collections.IEnumerator PlayEndOfRoundAudiosWithTimeout(float timeout)
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

    private void ResetEnemyPosition()
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
}

