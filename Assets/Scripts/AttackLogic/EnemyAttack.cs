using UnityEngine;
using System.Collections;
using UnityEngine.XR;


public class EnemyAttackBehavior : MonoBehaviour
{
    public float minAttackInterval;
    public float maxAttackInterval;
    public float cooldownAfterAttack;
    public float reflex_time_duration;
    private AudioClip attackIncomingSound;
    public AudioClip attackIncomingSoundEasy;
    public AudioClip attackIncomingSoundHard;
    public AudioClip attackHitSound;
    public AudioClip attackMissSound;
    public Light warningLight;
    public float flashDuration = 0.5f;
    public int flashCount = 4;
    public float duckingThresholdPercentage = 0.75f;
    public Camera playerCamera; // The player's camera (assign the main VR camera)
    public float distanceInFront;

    private bool canAttack = true;
    public AudioSource audioSource;
    private float initialHeadsetHeight;
    private float duckingThreshold;
    public MoveEnemyInFront MoveEnemyInFront;
    private float initialYPosition;
    private Vector3 targetPosition;

    public GameObject enemyObject;
    public float safeDistance; // The distance at which the player is considered safe
    public Animator modelAnimator;
    private bool postTutorialFlag = false;

    private AccessibleMenu.DifficultyLevel currentDifficulty;
    public RoundsManager roundsManager;
    public BoxingRingMapping ringMapping;
    public GameModuleManager gameModuleManager;

    private static int playerDuckCount = 0;
    private static int playerHitCount = 0;

    public float minDistanceFromPlayer;
    public float maxDistanceFromPlayer;

    public static int GetPlayerHitCount()
    {
        return playerHitCount;
    }

    public static void SetPlayerHitCount(int value)
    {
        playerHitCount = value;
    }

    public static int GetPlayerDuckCount()
    {
        return playerDuckCount;
    }

    public static void SetPlayerDuckCount(int value)
    {
        playerDuckCount = value;
    }

    void Update()
    {
        // Continuously check if the tutorial status has changed
        if (TutorialManager.TutorialCompleted && !postTutorialFlag)
        {
            playerDuckCount = 0;
            playerHitCount = 0;
            postTutorialFlag = true;
        }
    }

    void Start()
    {
        StartCoroutine(AttackRoutine());
        audioSource = GetComponent<AudioSource>();
        UpdateDifficultySettings();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void UpdateDifficultySettings()
    {
        currentDifficulty = AccessibleMenu.CurrentDifficulty;

        switch (currentDifficulty)
        {
            case AccessibleMenu.DifficultyLevel.Easy:
                minAttackInterval =  3f;
                maxAttackInterval =  10f;
                cooldownAfterAttack = 5f;
                reflex_time_duration = 2f;
                break;
            case AccessibleMenu.DifficultyLevel.Medium:
                minAttackInterval =  2f;
                maxAttackInterval =  4.5f;
                cooldownAfterAttack = 1.3f;
                reflex_time_duration = 1.25f;
                break;
            case AccessibleMenu.DifficultyLevel.Hard:
                minAttackInterval =  1.5f;
                maxAttackInterval =  3f;
                cooldownAfterAttack = 0.9f;
                reflex_time_duration = 1.3f;
                break;
            case AccessibleMenu.DifficultyLevel.UltraHard:
                minAttackInterval = 0.1f;
                maxAttackInterval = 1.1f;
                cooldownAfterAttack = 0.5f;
                reflex_time_duration = 1f;
                break;
        }
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack)
            {
                // Check if difficulty has changed
                if (currentDifficulty != AccessibleMenu.CurrentDifficulty)
                {
                    UpdateDifficultySettings();
                }

                float randomDelay = Random.Range(minAttackInterval, maxAttackInterval);
                yield return new WaitForSeconds(randomDelay);

                // Check if the mode is offensive before attacking
                if ( (AccessibleMenu.IsOffensiveMode && TutorialManager.TutorialAttackFlag))
                {
                    yield return StartCoroutine(PerformAttack());
                    canAttack = false;
                    yield return new WaitForSeconds(cooldownAfterAttack);
                    canAttack = true;
                }
                else
                {
                    // If in defensive mode, skip the attack
                    // Debug.Log("In defensive mode, skipping attack.");
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    public IEnumerator PerformAttack()
    {
        // Debug.Log("Enemy is attacking!");
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;

        // Wait until the headset is detected and we can get its position
        if(headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
             initialHeadsetHeight = headPosition.y;
        }

        // Capture the initial height
        // Debug.Log("Initial headset height: " + initialHeadsetHeight);

        // Set the ducking threshold as a percentage of the initial height
        duckingThresholdPercentage = 0.75f; // Set to 75% of initial height
        duckingThreshold = duckingThresholdPercentage * initialHeadsetHeight;

        // Debug.Log("Initial headset height: " + initialHeadsetHeight + ". Ducking threshold set to: " + duckingThreshold);
        // Flash red lights
        if (warningLight != null)
        {
            for (int i = 0; i < flashCount; i++)
            {
                warningLight.enabled = true;
                yield return new WaitForSeconds(flashDuration / 2);
                warningLight.enabled = false;
                yield return new WaitForSeconds(flashDuration / 2);
            }
            if (MoveEnemyInFront != null)
            {
                // Move the enemy in front of the player

                SetTargetPositionInFrontOfPlayer();
                float distanceToTarget;
                do
                {
                    distanceToTarget = MoveEnemyInFront.MoveEnemyTowardsTarget(targetPosition);
                    // Debug.Log("Distance to target: " + distanceToTarget);
                    yield return null; // Wait for the next frame
                } while (distanceToTarget > 1f); // Continue until the enemy is close enough
            }
            else
            {
                Debug.LogError("MoveEnemyInFront reference not set in the Inspector");
            }
        }
        TriggerPunchAnimation();

        // Set the attack sound based on the current difficulty
        attackIncomingSound = currentDifficulty == AccessibleMenu.DifficultyLevel.Easy ?
            attackIncomingSoundEasy : attackIncomingSoundHard;

        audioSource.PlayOneShot(attackIncomingSound);

        yield return new WaitForSeconds(reflex_time_duration);

        // Check if player is safe (ducking or far enough away)
        if (!IsPlayerSafe())
        {
            // If the player is not safe, reduce score
            audioSource.PlayOneShot(attackHitSound);
            ScoreManager.AddEnemyScore(1);
            ScoreManager.DecrementScore(4);
            playerHitCount++;
        }
        else
        {
            // Debug.Log("Player is safe! No score penalty.");
            audioSource.PlayOneShot(attackMissSound);
            playerDuckCount++;
            // Implement your actual attack logic here
        }

        // Move enemy to new location right after punch animation is done.
        // Move the enemy to a random position after the attack
        
        Vector3 newPosition = GetRandomPositionInFrontOfPlayer();

        // Use the existing MoveEnemyInFront script to move the enemy

        if (AccessibleMenu.CurrentDifficulty == AccessibleMenu.DifficultyLevel.UltraHard)
        {
            Debug.Log("Ultra hard survival mode, teleporting enemy after attack");
            roundsManager.TeleportEnemyPositionSurvivalMode();
        }
        else
        {
            float distanceToTarget1;
            do
            {
                distanceToTarget1 = MoveEnemyInFront.MoveEnemyTowardsTarget(newPosition);
                yield return null;
            } while (distanceToTarget1 > 1f);

            Debug.Log("Enemy moved to new position after attack");
        }

    }

    void TriggerPunchAnimation()
    {
        if (modelAnimator != null)
        {
            modelAnimator.SetTrigger("Punch");
        }
        else
        {
            Debug.LogWarning("Animator not assigned!");
        }
    }

    IEnumerator CaptureInitialHeadsetHeight()
    {
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;

        // Wait until the headset is detected and we can get its position
        while (!headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
            yield return null; // Keep waiting until we have a valid headset position
        }

        // Capture the initial height
        initialHeadsetHeight = headPosition.y;
        // Debug.Log("Initial headset height: " + initialHeadsetHeight);
        // Debug.Log("captured headset position : " + headPosition.y);

        // Set the ducking threshold as a percentage of the initial height
        duckingThresholdPercentage = 0.75f; // Set to 75% of initial height
        duckingThreshold = duckingThresholdPercentage * initialHeadsetHeight;
        // Debug.Log("Initial headset height: " + initialHeadsetHeight + ". Ducking threshold set to: " + duckingThresholdPercentage * initialHeadsetHeight);
    }


    bool IsPlayerSafe()
    {
        // Get headset position
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;
        // Debug.Log("ducking height: " + duckingThreshold);
        
        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
            // Check if the player is ducking
            bool isDucking = headPosition.y < duckingThreshold;

            // Check the distance between player and enemy
            if (enemyObject != null)
            {
                float distanceToEnemy = Vector3.Distance(headPosition, enemyObject.transform.position);
                bool isDistanceSafe = distanceToEnemy > safeDistance;

                // Debug.Log($"Distance to enemy: {distanceToEnemy}, Is distance safe: {isDistanceSafe}");

                // Player is safe if they are either ducking or far enough away
                return isDucking || isDistanceSafe;
            }
            else
            {
                Debug.LogWarning("Enemy object not set in EnemyAttackBehavior!");
                return isDucking; // Fall back to just checking ducking if enemy object is not set
            }
        }

        return false;
    }

    void SetTargetPositionInFrontOfPlayer()
    {
        if (enemyObject == null || playerCamera == null)
        {
            Debug.LogWarning("Enemy or Player Camera is not assigned!");
            return;
        }

        // Calculate the target position in front of the player (only Z and Y change, X stays the same)
        Vector3 forwardDirection = playerCamera.transform.forward;
        forwardDirection.x = 0;  // Ignore X-axis direction to keep it horizontal
        forwardDirection.Normalize();  // Normalize the forward vector

        targetPosition = playerCamera.transform.position + forwardDirection * distanceInFront;
        targetPosition.y = initialYPosition;  // Keep the enemy's y-coordinate fixed
    }

    private Vector3 GetRandomPositionInFrontOfPlayer()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player Camera is not assigned!");
            return transform.position;
        }
        
        Debug.Log("Attack done, finding new position to move to");
        // Get a random distance within the specified range
        float randomDistance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

        // Calculate a random angle within a 180-degree arc in front of the player
        float randomAngle = Random.Range(-120f, 120f);

        // Calculate the forward direction of the player, ignoring vertical rotation
        Vector3 playerForward = playerCamera.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        // Rotate the forward vector by the random angle
        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * playerForward;

        // Calculate the new position
        Vector3 newPosition = playerCamera.transform.position + randomDirection * randomDistance;
        newPosition.y = initialYPosition; // Keep the enemy's y-coordinate fixed

        // Check if the new position is within the specified rectangle
        if (newPosition.x < -3f || newPosition.x > 3f || newPosition.z < -3f || newPosition.z > 3f)
        {
            // If outside the rectangle, reset to the specified position
            newPosition = new Vector3(0.17f, 0.9f, 1.3f);
            Debug.Log("Position outside bounds. Reset to: " + newPosition);
        }
        else
        {
            Debug.Log("New position: " + newPosition);
        }

        return newPosition;
    }
}