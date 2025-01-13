using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubjectCardID : MonoBehaviour
{
    [SerializeField] private TMP_InputField _subjectText;
    [SerializeField] private RectTransform _inputFieldRect;
    public int _id;

    private float minWidth = 50f;
    private float maxWidth = 347.37f;

    private void OnEnable(){
        _subjectText.onEndEdit.AddListener(UpdateName);
        _subjectText.onValueChanged.AddListener(AdjustWidth);
    }

    private void OnDisable(){
        _subjectText.onEndEdit.RemoveListener(UpdateName);
        _subjectText.onValueChanged.RemoveListener(AdjustWidth);
    }

    public void Setup(string subject, int id)
    {
        _subjectText.text = subject;
        AdjustWidth(subject);
        this._id = id;
    }

    public void UpdateName(string subject){
        if(!string.IsNullOrEmpty(subject)){
            LobbyUIManager.Instance.UpdateSubject(subject, _id);
        }
    }

    private void AdjustWidth(string newText)
    {
        float preferredWidth = _subjectText.textComponent.preferredWidth;

        float newWidth = Mathf.Clamp(preferredWidth + 20f, minWidth, maxWidth);

        _inputFieldRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

}