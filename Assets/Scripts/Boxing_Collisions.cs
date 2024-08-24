using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using TMPro; // Required for TextMeshPro



public class Boxing_Collisions : MonoBehaviour
{
    public XRBaseController leftController;  // Assign via Inspector
    public XRBaseController rightController; // Assign via Inspector
    public InputActionReference hapticAction;
    public int score = 0;  // Variable to keep track of the score
    public TMP_Text scoreText; // Assign via Inspector, TextMeshPro text element to display the score


    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame

    void OnTriggerEnter(Collider collision)
    {

        // Increment the score
        score++;
        UpdateScoreText();

        // Check if the collided object has a specific tag or name
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Perform actions on collision
            Debug.Log("Collision detected with " + collision.gameObject.name);
            SendHapticImpulse(leftController, 0.5f, 0.2f);
            SendHapticImpulse(rightController, 0.5f, 0.2f);


            // Apply force to the other object
            // Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            // if (rb != null)
            // {
            //     rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
            // }
        }
    }

    void SendHapticImpulse(XRBaseController controller, float amplitude, float duration)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

}

