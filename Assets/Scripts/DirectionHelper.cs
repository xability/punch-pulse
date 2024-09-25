using UnityEngine;
using System;

public class DirectionHelper : MonoBehaviour
{
    public static float CalculateAngle(Vector3 playerPosition, Vector3 enemyPosition)
    {
        Vector2 playerPos2D = new Vector2(playerPosition.x, playerPosition.z);
        Vector2 enemyPos2D = new Vector2(enemyPosition.x, enemyPosition.z);
        Vector2 vectorToEnemy = enemyPos2D - playerPos2D;

        float angle = Mathf.Atan2(vectorToEnemy.y, vectorToEnemy.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    public static string GetClockDirection(float angle)
    {
        if (angle >= 337.5f || angle < 22.5f)
            return "12 o'clock (Straight)";
        else if (angle >= 22.5f && angle < 67.5f)
            return "1 o'clock (Diagonal Right)";
        else if (angle >= 67.5f && angle < 112.5f)
            return "3 o'clock (Right)";
        else if (angle >= 112.5f && angle < 157.5f)
            return "4 o'clock (Diagonal Right)";
        else if (angle >= 157.5f && angle < 202.5f)
            return "6 o'clock (Back)";
        else if (angle >= 202.5f && angle < 247.5f)
            return "7 o'clock (Diagonal Left)";
        else if (angle >= 247.5f && angle < 292.5f)
            return "9 o'clock (Left)";
        else if (angle >= 292.5f && angle < 337.5f)
            return "10 o'clock (Diagonal Left)";
        else
            return "Unknown";
    }
}