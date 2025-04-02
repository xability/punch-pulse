using UnityEngine;

public class BoxingRingMapping : MonoBehaviour
{
    public GameObject ringObject;
    public float xLong = 1.2f;
    public float yWide = 1.2f;

    void Start()
    {
        // Approx. length of one step in meters:
        // float oneStepLength = 0.75f;

        // Calculate total length in meters:
        float ringLength = xLong;
        float ringWidth = yWide;

        if (ringObject != null)
        {
            // Scale the ring in the X and Z dimensions (Y stays the same, for example)
            ringObject.transform.localScale = new Vector3(ringLength, ringWidth, ringObject.transform.localScale.z);
        }
    }
}
