using UnityEngine;
using System.Collections;

public class EnemyAttackBehavior : MonoBehaviour
{
    public float minAttackInterval = 0f;
    public float maxAttackInterval = 10f;
    public float cooldownAfterAttack = 4.5f;

    private bool canAttack = true;

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (canAttack)
            {
                float randomDelay = Random.Range(minAttackInterval, maxAttackInterval);
                yield return new WaitForSeconds(randomDelay);

                PerformAttack();

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

    void PerformAttack()
    {
        Debug.Log("Enemy is attacking!");
        // Implement your attack logic here
    }
}