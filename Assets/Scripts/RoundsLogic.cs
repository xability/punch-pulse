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

    [Header("Audio Clips")]
    [Tooltip("Audio clip to play at the start of any round.")]
    public AudioClip roundStartAudio;

    [Tooltip("Audio clip to play at the end of any round.")]
    public AudioClip roundEndAudio;

    [Header("References")]
    [Tooltip("Audio source to play the clips from (attach an AudioSource component to this GameObject or another).")]
    public AudioSource audioSource;

    [Tooltip("Game Over UI Canvas or Panel.")]
    public GameObject gameOverUI;

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

    private System.Collections.IEnumerator HandleOneRound(string roundName, int roundNumber)
    {
        // Play round start audio
        if (audioSource != null && roundStartAudio != null)
        {
            audioSource.PlayOneShot(roundStartAudio);
        }

        // Optionally, display or announce the round name if you have UI or TTS
        Debug.Log(roundName + " has started.");

        // Wait for the round duration
        yield return new WaitForSeconds(roundDuration);

        // Round is over
        Debug.Log(roundName + " ended.");

        // Play round end audio
        if (audioSource != null && roundEndAudio != null)
        {
            audioSource.PlayOneShot(roundEndAudio);
        }

        // Wait for the break ONLY if this isn't the last round overall
        // If you want a break after the last round too, remove this check
        bool isLastRound = (roundNumber != 0 && roundNumber == totalRounds);
        if (!isLastRound)
        {
            Debug.Log("Break between rounds. Duration: " + roundBreakDuration + " seconds.");
            yield return new WaitForSeconds(roundBreakDuration);
        }
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
}
