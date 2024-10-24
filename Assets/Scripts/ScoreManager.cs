using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public AudioClip[] numberClips; // Assign in the Inspector, e.g., index 0 = "zero", 1 = "one", etc.
    public AudioSource audioSource; // Assign the AudioSource component

    public static int Score { get; private set; }
    public TextMeshProUGUI scoreText;
    public InputActionReference rightButtonAction; // Assign via Inspector

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
        StartCoroutine(AnnounceScore());
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
    }

    public static void AddScore(int amount)
    {
        Score += amount;
        Instance.UpdateScoreDisplay();
    }

    public static void DecrementScore(int amount)
    {
        Score = Mathf.Max(0, Score - amount);
        Instance.UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString(); ;
        }
    }

    private IEnumerator AnnounceScore()
    {
        string scoreString = ScoreManager.Score.ToString();
        foreach (char digitChar in scoreString)
        {
            int digit = digitChar - '0'; // Convert char to int

            if (digit >= 0 && digit < numberClips.Length)
            {
                audioSource.PlayOneShot(numberClips[digit]);
                yield return new WaitForSeconds(numberClips[digit].length);
            }
            else
            {
                Debug.LogWarning("Number audio clip not found for digit: " + digit);
                yield break;
            }
        }
    }
}