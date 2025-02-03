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
        // Play round start audio
        PlayRoundStartAudio(roundNumber);

        Debug.Log(roundName + " has started.");

        // Wait for the round duration
        yield return new WaitForSeconds(roundDuration);

        Debug.Log(roundName + " ended.");

        // Play round end audio
        if (audioSource != null && roundEndAudio != null)
        {
            audioSource.PlayOneShot(roundEndAudio);
        }

        // Play additional end-of-round audio clips
        yield return StartCoroutine(PlayEndOfRoundAudios());

        bool isLastRound = (roundNumber != 0 && roundNumber == totalRounds);
        if (!isLastRound)
        {
            Debug.Log("Break between rounds. Duration: " + roundBreakDuration + " seconds.");
            yield return new WaitForSeconds(roundBreakDuration);
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
}

