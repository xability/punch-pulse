using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStats
{
    public int leftTriggerCount;
    public int rightTriggerCount;
    public int duckCount;
    public int playerHitCount;
    public int playerHeadPunchCount;
    public int playerBodyPunchCount;
    public int playerScore;
    public int enemyScore;
    // Add any other stats you want to track
}

public static class GameStatsManager
{
    private static Dictionary<string, GameStats> gameStatsByMode = new Dictionary<string, GameStats>();

    public static void SaveStats(string gameMode, GameStats stats)
    {
        if (gameStatsByMode.ContainsKey(gameMode))
        {
            gameStatsByMode[gameMode] = stats;
        }
        else
        {
            gameStatsByMode.Add(gameMode, stats);
        }
    }

    public static GameStats GetStats(string gameMode)
    {
        if (gameStatsByMode.TryGetValue(gameMode, out GameStats stats))
        {
            return stats;
        }
        return new GameStats(); // Return empty stats if game mode not found
    }

    public static void ResetStats(string gameMode)
    {
        if (gameStatsByMode.ContainsKey(gameMode))
        {
            gameStatsByMode[gameMode] = new GameStats();
        }
    }

}
