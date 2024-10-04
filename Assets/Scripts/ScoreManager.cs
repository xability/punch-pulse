using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public static int Score { get; private set; }
    public TextMeshProUGUI scoreText;

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
}