using UnityEngine;

public class DebuugCameraFollow : MonoBehaviour
{
    public Transform target;           // The target the camera will follow
    public float distance = 5f;        // Distance behind the player
    public float height = 2f;          // Height above the player
    public float smoothSpeed = 0.125f; // Smooth factor for camera movement

    private Vector3 currentVelocity;   // To smooth the camera movement

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position behind and above the player
        Vector3 desiredPosition = target.position + Vector3.back * distance + Vector3.up * height;

        // Smoothly move towards the desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);

        // Set the camera's position
        transform.position = smoothedPosition;

        // Always look at the player
        transform.LookAt(target);
    }
}
