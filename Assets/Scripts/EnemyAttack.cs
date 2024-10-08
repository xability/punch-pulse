using UnityEngine;
using System.Collections;
using UnityEngine.XR;

public class EnemyAttackBehavior : MonoBehaviour
{
    public float minAttackInterval = 0f;
    public float maxAttackInterval = 10f;
    public float cooldownAfterAttack = 4.5f;
    public AudioClip attackIncomingSound;
    public AudioClip attackHitSound;
    public AudioClip attackMissSound;
    public Light warningLight;
    public float flashDuration = 0.5f;
    public int flashCount = 3;
    public float duckingThresholdPercentage = 0.75f;

    private bool canAttack = true;
    public AudioSource audioSource;
    private float initialHeadsetHeight;
    private float duckingThreshold;

    void Start()
    {
        StartCoroutine(AttackRoutine());
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack)
            {
                float randomDelay = Random.Range(minAttackInterval, maxAttackInterval);
                yield return new WaitForSeconds(randomDelay);

                yield return StartCoroutine(PerformAttack());

                canAttack = false;
                yield return new WaitForSeconds(cooldownAfterAttack);
                canAttack = true;
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator PerformAttack()
    {
        Debug.Log("Enemy is attacking!");
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;

        // Wait until the headset is detected and we can get its position
        if(headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
             initialHeadsetHeight = headPosition.y;
        }

        // Capture the initial height
        Debug.Log("Initial headset height: " + initialHeadsetHeight);

        // Set the ducking threshold as a percentage of the initial height
        duckingThresholdPercentage = 0.75f; // Set to 75% of initial height
        duckingThreshold = duckingThresholdPercentage * initialHeadsetHeight;

        // Play attack sound
        if (attackIncomingSound != null)
        {
            audioSource.PlayOneShot(attackIncomingSound);
        }

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
        }

        // Check if player is ducking
        if (!IsPlayerDucking())
        {
            // If the player is not ducking, reduce score
            audioSource.PlayOneShot(attackHitSound);
            ScoreManager.DecrementScore(5);
        }
        else
        {
            Debug.Log("Player ducked! No score penalty.");
            audioSource.PlayOneShot(attackMissSound);
            // Implement your actual attack logic here
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
        Debug.Log("Initial headset height: " + initialHeadsetHeight);
        Debug.Log("captured headset position : " + headPosition.y);

        // Set the ducking threshold as a percentage of the initial height
        duckingThresholdPercentage = 0.75f; // Set to 75% of initial height
        duckingThreshold = duckingThresholdPercentage * initialHeadsetHeight;
        Debug.Log("Initial headset height: " + initialHeadsetHeight + ". Ducking threshold set to: " + duckingThresholdPercentage * initialHeadsetHeight);
    }


    bool IsPlayerDucking()
    {
        // Get headset position
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;
        Debug.Log("ducking height: " + duckingThreshold);
        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
            // Return true if the player's head is below the ducking threshold
            Debug.Log("captured headset position : " + headPosition.y);
            return headPosition.y < duckingThreshold;
        }

        return false;
    }
}