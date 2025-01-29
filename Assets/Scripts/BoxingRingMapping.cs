using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.Oculus;

public class BoxingRingMapping : MonoBehaviour
{
    public GameObject objectToScale; // The object you want to scale based on boundary dimensions

    void Start()
    {
        bool isConfigured = Unity.XR.Oculus.Boundary.GetBoundaryConfigured();
        // Check if the boundary is configured
        if (isConfigured)
        {
            Vector3 dimensions;
            bool success = Unity.XR.Oculus.Boundary.GetBoundaryDimensions(Unity.XR.Oculus.Boundary.BoundaryType.PlayArea, out dimensions);
            if (success)
            {
                float width = dimensions.x;
                float depth = dimensions.z;
                float height = dimensions.y;

                Debug.Log($"Play Area Dimensions: Width = {width}, Depth = {depth}, Height = {height}");
            }
            else
            {
                Debug.Log("Failed to get boundary dimensions");
            }
        }
        else
        {
            Debug.LogWarning("Boundary is not configured.");
        }
    }

    void ScaleObjectBasedOnBoundary(Vector3 boundaryDimensions)
    {
        if (objectToScale != null)
        {
            // Example: Scale the object proportionally to the boundary dimensions
            float scaleFactor = boundaryDimensions.x / 10f; // Adjust scaling logic as needed
            objectToScale.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            Debug.Log("Object scaled based on boundary dimensions.");
        }
        else
        {
            Debug.LogWarning("Object to scale is not assigned.");
        }
    }
}
