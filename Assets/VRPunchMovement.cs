using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRPunchMovement : MonoBehaviour
{
    public XRDirectInteractor leftHandInteractor; // Reference to the left hand interactor
    public XRDirectInteractor rightHandInteractor; // Reference to the right hand interactor
    public XRBaseController leftController;  // Assign via Inspector
    public XRBaseController rightController; // Assign via Inspector
    public float pushForce = 10f;

    private void ApplyPushForce(Transform handTransform)
    {
        Vector3 direction = transform.position - handTransform.position;
        direction.y = 0; // Ignore vertical direction

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction.normalized * pushForce, ForceMode.Impulse);
        }
    }
}
