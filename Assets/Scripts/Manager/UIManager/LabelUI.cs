using UnityEngine;

public class LabelUI : MonoBehaviour
{
    private Camera mainCamera;
    private float baseSpacing = 0.4f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        Transform nameTag = null;
        Transform adjustableChild = null;

        // Find the NameTag and adjustable child
        foreach (Transform child in transform)
        {
            if (child.gameObject.name.EndsWith("_face") && child.gameObject.activeSelf)
            {
                adjustableChild = child;
            }
            else if (child.gameObject.name == "NameTag" && child.gameObject.activeSelf)
            {
                nameTag = child;
            }
        }

        // Calculate dynamic spacing based on the adjustable child's scale
        float dynamicSpacing = baseSpacing;
        if (adjustableChild != null)
        {
            dynamicSpacing = Mathf.Max(baseSpacing, adjustableChild.localScale.y);
        }

        // Set yOffset for stacking elements
        float yOffset = 0;

        // Handle NameTag positioning
        if (nameTag != null)
        {
            nameTag.localPosition = new Vector3(0, yOffset, 0);
            yOffset += dynamicSpacing;

            // Ensure NameTag always faces the camera
            nameTag.transform.LookAt(mainCamera.transform);
            nameTag.transform.Rotate(0, 180, 0);
        }

        // Position other children (e.g., video feeds)
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf && child != nameTag) // Skip the NameTag
            {
                child.localPosition = new Vector3(0, -yOffset, 0);
                yOffset += dynamicSpacing;

                // Ensure child faces the camera
                child.transform.LookAt(mainCamera.transform);
                child.transform.Rotate(0, 180, 0);
            }
        }

        // Set the parent's position based on child visibility
        if (nameTag != null || adjustableChild != null)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 2.62f, transform.localPosition.z);
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 2.139f, transform.localPosition.z);
        }
    }
}
