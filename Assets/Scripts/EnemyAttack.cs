using UnityEngine;
using System.Collections;

public class EnemyAttackBehavior : MonoBehaviour
{
    public float minAttackInterval = 0f;
    public float maxAttackInterval = 10f;
    public float cooldownAfterAttack = 4.5f;
    public AudioClip attackSound;
    public Light warningLight;
    public float flashDuration = 0.5f;
    public int flashCount = 3;

    private bool canAttack = true;
    private AudioSource audioSource;

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

        // Play attack sound
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
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
        
        // On player hit, reduce score by 5
        ScoreManager.DecrementScore(5);
        // Implement your actual attack logic here
    }
}