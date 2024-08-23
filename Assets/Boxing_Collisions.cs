using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxing_Collisions : MonoBehaviour
{
    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Collision detected with ");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with " + collision.gameObject.name);
        // Check if the collided object has a specific tag or name
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Perform actions on collision
            UnityEngine.Debug.Log("Collision detected with " + collision.gameObject.name);


            // Apply force to the other object
            // Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            // if (rb != null)
            // {
            //     rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
            // }
        }
    }
}
