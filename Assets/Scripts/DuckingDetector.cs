using UnityEngine;
using UnityEngine.XR;

public class DuckingDetector : MonoBehaviour
{
    public float duckingThreshold = 1.2f; // Adjust this value based on your game's scale
    public float duckingDurationThreshold = 10f; // 10 seconds

    private float initialHeadHeight;
    private float duckingTimer = 0f;
    private bool isCurrentlyDucking = false;

    void Start()
    {
        // Capture initial head height when the game starts
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;
        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
            initialHeadHeight = headPosition.y;
        }
    }

    void Update()
    {
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 headPosition;

        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
        {
            float currentHeight = headPosition.y;

            if (currentHeight < initialHeadHeight - duckingThreshold)
            {
                if (!isCurrentlyDucking)
                {
                    isCurrentlyDucking = true;
                    duckingTimer = 0f;
                }
                duckingTimer += Time.deltaTime;

                if (duckingTimer >= duckingDurationThreshold)
                {
                    Debug.Log("Player has been ducking for over 10 seconds!");
                    // Add your logic here for when the player has been ducking too long
                }
            }
            else
            {
                isCurrentlyDucking = false;
                duckingTimer = 0f;
            }
        }
    }
}
