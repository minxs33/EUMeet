using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizCardID : MonoBehaviour
{
    [SerializeField] private TMP_InputField _titleText;
    [SerializeField] private RectTransform inputFieldRect;
    public int _id;

    private float minWidth = 50f;
    private float maxWidth = 347.37f;

    private void OnEnable(){
        _titleText.onEndEdit.AddListener(UpdateTitle);
        _titleText.onValueChanged.AddListener(AdjustWidth);
    }

    private void OnDisable(){
        _titleText.onEndEdit.RemoveListener(UpdateTitle);
        _titleText.onValueChanged.RemoveListener(AdjustWidth);
    }

    public void Setup(string title, int id)
    {
        _titleText.text = title;
        AdjustWidth(title);
        this._id = id;
    }

    public void UpdateTitle(string title){
        if(!string.IsNullOrEmpty(title)){
            LobbyUIManager.Instance.UpdateQuiz(title, _id);
        }
    }

    private void AdjustWidth(string newText)
    {
        float preferredWidth = _titleText.textComponent.preferredWidth;

        float newWidth = Mathf.Clamp(preferredWidth + 20f, minWidth, maxWidth);

        inputFieldRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

}