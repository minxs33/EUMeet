using UnityEngine;

public class LabelUI : MonoBehaviour
{
    private Camera mainCamera;
    public float baseSpacing = 0.4f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            Transform nameTag = null;
            Transform adjustableChild = null;
            foreach (Transform child in transform)
            {
                if (child.gameObject.name == "NameTag" && child.gameObject.activeSelf)
                {
                    nameTag = child;
                }
                else if (child.gameObject.name.EndsWith("_face") && child.gameObject.activeSelf)
                {
                    adjustableChild = child;
                }
            }

            // Calculate dynamic spacing based on the adjustable child
            float dynamicSpacing = baseSpacing;
            if (adjustableChild != null)
            {
                dynamicSpacing = Mathf.Max(baseSpacing, adjustableChild.localScale.y);
            }

            // Set yOffset to 0 initially
            float yOffset = 0;

            // Ensure NameTag is always at the top
            if (nameTag != null)
            {
                nameTag.localPosition = new Vector3(0, yOffset, 0);
                yOffset += dynamicSpacing; // Move down after the NameTag
                nameTag.transform.LookAt(mainCamera.transform);
                nameTag.transform.Rotate(0, 180, 0);
            }

            // Position the other children (video feeds, etc.)
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf && child != nameTag) // Skip the NameTag
                {
                    child.localPosition = new Vector3(0, -yOffset, 0);
                    yOffset += dynamicSpacing; // Increment position for the next element

                    // Make sure all children face the camera
                    child.transform.LookAt(mainCamera.transform);
                    child.transform.Rotate(0, 180, 0);
                }
            }
        }
    }
}
