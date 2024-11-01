using UnityEngine;

public class PlayerName : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // Rotate the text so it always faces the camera
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0); // Adjust as needed depending on orientation
        }
    }
}
