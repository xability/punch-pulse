using UnityEngine;

public class GameModuleManager : MonoBehaviour
{
    public enum GameMode
    {
        LevelProgression,
        Manual,
        HardSurvival
    }

    [SerializeField]
    private GameMode currentMode = GameMode.LevelProgression;

    public static GameModuleManager Instance { get; private set; }

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

    public GameMode CurrentMode
    {
        get { return currentMode; }
        set { currentMode = value; }
    }

    public bool IsLevelProgressionMode => currentMode == GameMode.LevelProgression;
    public bool IsManualMode => currentMode == GameMode.Manual;
    public bool IsHardSurvivalMode => currentMode == GameMode.HardSurvival;
}