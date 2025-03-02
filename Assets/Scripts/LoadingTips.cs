using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingTips : MonoBehaviour
{
    private TMP_Text textContent;
    private FadeAnimation fadeAnimation;
    [SerializeField] private string[] tips;
    [SerializeField] private float tipDisplayDuration = 3f;

    private int lastTipIndex = -1;

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
            fadeAnimation.FadeOut();
            yield return new WaitForSeconds(fadeAnimation.duration);

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, tips.Length);
            } while (randomIndex == lastTipIndex);

            lastTipIndex = randomIndex;
            textContent.text = "Tips: "+tips[randomIndex];

            fadeAnimation.FadeIn();
            yield return new WaitForSeconds(fadeAnimation.duration);

            yield return new WaitForSeconds(tipDisplayDuration - (2 * fadeAnimation.duration));
        }
    }

    private void StopTips() {
        StopCoroutine(DisplayTips());
        fadeAnimation.GetComponent<CanvasGroup>().alpha = 0;
    }
    
}
