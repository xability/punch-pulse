using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchImpartMovement : MonoBehaviour
{
    public float punchThreshold = 2f;
    private Vector3 previousPosition;
    private float currentVelocity;

    void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        currentVelocity = Vector3.Distance(currentPosition, previousPosition) / Time.fixedDeltaTime;
        previousPosition = currentPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentVelocity > punchThreshold && collision.gameObject.CompareTag("Ragdoll"))
        {
            Vector3 punchForce = transform.forward * currentVelocity * 10f;
            collision.rigidbody.AddForce(punchForce, ForceMode.Impulse);
        }
    }
}