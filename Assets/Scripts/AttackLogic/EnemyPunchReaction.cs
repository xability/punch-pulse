using UnityEngine;

public class EnemyPunchReaction : MonoBehaviour
{
    public float punchForce = 7f;
    public string playerGloveTag;
    public float dampingFactor = 0.95f;
    public float minimumVelocity = 0.1f;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private bool isPunched = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerGloveTag))
        {
            Vector3 punchDirection = transform.position - other.transform.position;
            punchDirection.y = 0;
            punchDirection = punchDirection.normalized;

            currentVelocity = punchDirection * punchForce;
            isPunched = true;
        }
    }

    void FixedUpdate()
    {
        if (isPunched)
        {
            ApplyDamping();
            MoveObject();
        }
    }

    void ApplyDamping()
    {
        currentVelocity *= dampingFactor;

        if (currentVelocity.magnitude < minimumVelocity)
        {
            currentVelocity = Vector3.zero;
            isPunched = false;
        }
    }

    void MoveObject()
    {
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }
}
/*
 
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