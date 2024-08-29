//
// Purpose: Estimates the velocity of an object based on change in position
//

using UnityEngine;
using System.Collections;



//-------------------------------------------------------------------------
public class VelocityEstimator : MonoBehaviour
{

    // Number of frames to average over for computing linear velocity
    [Tooltip("How many frames to average over for computing velocity")]
    public int velocityAverageFrames = 5;


    // Number of frames to average over for computing angular velocity
    [Tooltip("How many frames to average over for computing angular velocity")]
    public int angularVelocityAverageFrames = 11;


    // Whether to start estimating velocity immediately on Awake
    public bool estimateOnAwake = false;


    private Coroutine routine; // Coroutine for velocity estimation
    private int sampleCount; // Number of samples collected so far
    private Vector3[] velocitySamples; // Array to store linear velocity samples
    private Vector3[] angularVelocitySamples; // Array to store angular velocity samples


    //-------------------------------------------------
    // Starts the coroutine to estimate velocity and angular velocity
    public void BeginEstimatingVelocity()
    {
        FinishEstimatingVelocity(); // Stop any existing estimation routine

        // Start the velocity estimation coroutine
        routine = StartCoroutine(EstimateVelocityCoroutine());
    }


    //-------------------------------------------------
    // Stops the coroutine if it's running
    public void FinishEstimatingVelocity()
    {
        if (routine != null)
        {
            StopCoroutine(routine); // Stop the coroutine
            routine = null; // Clear the coroutine reference
        }
    }


    //-------------------------------------------------
    // Returns the average linear velocity based on collected samples
    public Vector3 GetVelocityEstimate()
    {
        Vector3 velocity = Vector3.zero;
        int velocitySampleCount = Mathf.Min(sampleCount, velocitySamples.Length);

        // Compute the average velocity from the collected samples
        if (velocitySampleCount != 0)
        {
            for (int i = 0; i < velocitySampleCount; i++)
            {
                velocity += velocitySamples[i];
            }
            velocity *= (1.0f / velocitySampleCount);
        }

        return velocity;
    }


    //-------------------------------------------------
    // Returns the average angular velocity based on collected samples
    public Vector3 GetAngularVelocityEstimate()
    {
        Vector3 angularVelocity = Vector3.zero;
        int angularVelocitySampleCount = Mathf.Min(sampleCount, angularVelocitySamples.Length);

        // Compute the average angular velocity from the collected samples
        if (angularVelocitySampleCount != 0)
        {
            for (int i = 0; i < angularVelocitySampleCount; i++)
            {
                angularVelocity += angularVelocitySamples[i];
            }
            angularVelocity *= (1.0f / angularVelocitySampleCount);
        }

        return angularVelocity;
    }


    //-------------------------------------------------
    // Calculates and returns the estimated acceleration based on velocity changes
    public Vector3 GetAccelerationEstimate()
    {
        Vector3 average = Vector3.zero;

        // Compute acceleration from changes in velocity samples
        for (int i = 2 + sampleCount - velocitySamples.Length; i < sampleCount; i++)
        {
            if (i < 2)
                continue;

            int first = i - 2;
            int second = i - 1;

            // Get velocity samples for the two previous frames
            Vector3 v1 = velocitySamples[first % velocitySamples.Length];
            Vector3 v2 = velocitySamples[second % velocitySamples.Length];
            average += v2 - v1;
        }
        average *= (1.0f / Time.deltaTime); // Average change in velocity over time

        return average;
    }


    //-------------------------------------------------
    // Unity's Awake method initializes the velocity and angular velocity arrays
    void Awake()
    {
        velocitySamples = new Vector3[velocityAverageFrames];
        angularVelocitySamples = new Vector3[angularVelocityAverageFrames];

        // Optionally start estimating velocity when the object wakes up
        if (estimateOnAwake)
        {
            BeginEstimatingVelocity();
        }
    }


    //-------------------------------------------------
    // Coroutine that continuously estimates linear and angular velocities
    private IEnumerator EstimateVelocityCoroutine()
    {
        sampleCount = 0;

        Vector3 previousPosition = transform.position;
        Quaternion previousRotation = transform.rotation;

        while (true)
        {
            yield return new WaitForEndOfFrame(); // Wait until the end of the frame

            float velocityFactor = 1.0f / Time.deltaTime; // Factor to convert position change to velocity

            int v = sampleCount % velocitySamples.Length;
            int w = sampleCount % angularVelocitySamples.Length;
            sampleCount++;

            // Estimate linear velocity
            velocitySamples[v] = velocityFactor * (transform.position - previousPosition);

            // Estimate angular velocity
            Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(previousRotation);

            // Compute the angle of rotation (in radians)
            float theta = 2.0f * Mathf.Acos(Mathf.Clamp(deltaRotation.w, -1.0f, 1.0f));
            if (theta > Mathf.PI)
            {
                theta -= 2.0f * Mathf.PI; // Normalize the angle to be within -π to π
            }

            Vector3 angularVelocity = new Vector3(deltaRotation.x, deltaRotation.y, deltaRotation.z);
            if (angularVelocity.sqrMagnitude > 0.0f)
            {
                angularVelocity = theta * velocityFactor * angularVelocity.normalized;
            }

            angularVelocitySamples[w] = angularVelocity;

            // Update previous position and rotation
            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }
    }


}
