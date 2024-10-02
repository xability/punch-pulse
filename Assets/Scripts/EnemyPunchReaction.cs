using UnityEngine;

public class EnemyPunchReaction : MonoBehaviour
{
    public float punchForce = 10f;
    public string playerGloveTag;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerGloveTag)) // Make sure your player's fist has this tag
        {
            Debug.Log("Player glove tag matched!");
            Vector3 punchDirection = transform.position - other.transform.position;
            punchDirection.y = 0;
            punchDirection = punchDirection.normalized;

            Debug.Log("Applying force: " + (punchDirection * punchForce));
            rb.AddForce(punchDirection * punchForce, ForceMode.Impulse);
        }
    }
}

/*
 * using UnityEngine;

public class EnemyPunchReaction : MonoBehaviour
{
    public float punchForce = 10f;
    public string playerGloveTag;
    public float minPunchForce = 2f; // Minimum force required to move the enemy
    public float dampingFactor = 0.98f; // Damping to slow down the enemy
    public float stopThreshold = 0.1f; // Velocity below which the enemy stops

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ensure the Rigidbody uses gravity and has some drag
        rb.useGravity = true;
        rb.drag = 0.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerGloveTag))
        {
            Vector3 punchDirection = transform.position - other.transform.position;
            punchDirection.y = 0; // Keep the force horizontal
            punchDirection = punchDirection.normalized;

            float appliedForce = punchForce * other.attachedRigidbody.velocity.magnitude;
            if (appliedForce > minPunchForce)
            {
                rb.AddForce(punchDirection * appliedForce, ForceMode.Impulse);
            }
        }
    }

    void FixedUpdate()
    {
        ApplyDamping();
        //CheckIfStopped();
    }

    void ApplyDamping()
    {
        rb.velocity *= dampingFactor;
    }

    void CheckIfStopped()
    {
        if (rb.velocity.magnitude < stopThreshold)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
 */