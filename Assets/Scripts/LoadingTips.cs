using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingTips : MonoBehaviour
{
    private TMP_Text textContent;
    private FadeAnimation fadeAnimation;
    [SerializeField] private string[] tips; // Array to hold the tips
    [SerializeField] private float tipDisplayDuration = 5f; // Time in seconds to display each tip

    private int lastTipIndex = -1; // To avoid immediate repetition of the same tip

    private void OnEnable(){
        GameEventsManager.instance.UIEvents.onLocalPlayerJoined += StopTips;
    }
    void Start()
    {
        fadeAnimation = GetComponent<FadeAnimation>();
        textContent = this.transform.Find("TipsContent").GetComponent<TMP_Text>();

        if (tips.Length > 0)
        {
            StartCoroutine(DisplayTips());
        }
        else
        {
            Debug.LogWarning("No tips found in the array!");
        }
    }

    private IEnumerator DisplayTips()
    {
        while (true)
        {
            // Fade out the current tip
            fadeAnimation.FadeOut();
            yield return new WaitForSeconds(fadeAnimation.duration);

            // Pick a random tip index that is not the same as the last one
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, tips.Length);
            } while (randomIndex == lastTipIndex);

            // Update the last tip index and set the new tip
            lastTipIndex = randomIndex;
            textContent.text = "Tips: "+tips[randomIndex];

            // Fade in the new tip
            fadeAnimation.FadeIn();
            yield return new WaitForSeconds(fadeAnimation.duration);

            // Wait for the duration of the tip display (minus fade durations)
            yield return new WaitForSeconds(tipDisplayDuration - (2 * fadeAnimation.duration));
        }
    }

    private void StopTips() {
        StopCoroutine(DisplayTips());
        fadeAnimation.GetComponent<CanvasGroup>().alpha = 0;
    }
    
}
