using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] public bool isHardMode = false;

    public bool IsHardMode
    {
        get { return isHardMode; }
        set
        {
            if (isHardMode != value)
            {
                isHardMode = value;
                OnDifficultyChanged?.Invoke(isHardMode);
            }
        }
    }

    public delegate void DifficultyChangedDelegate(bool isHard);
    public event DifficultyChangedDelegate OnDifficultyChanged;

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

    public void ToggleDifficulty()
    {
        IsHardMode = !IsHardMode;
    }

    public int GetDifficultyLevel()
    {
        return IsHardMode ? 1 : 0;
    }
}

/*
To change the difficulty from any script, you can use:
DifficultyManager.Instance.ToggleDifficulty();

Or set it directly:
DifficultyManager.Instance.IsHardMode = true; // Set to Hard mode
DifficultyManager.Instance.IsHardMode = false; // Set to Easy mode
 */ 