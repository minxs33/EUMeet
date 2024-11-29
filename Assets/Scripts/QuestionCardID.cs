using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionCardID : MonoBehaviour
{
    public int _id;
    public string _question;
    public string _optionA;
    public string _optionB;
    public string _optionC;
    public string _optionD;
    public string _answer;

    [SerializeField] private TMP_InputField _questionText, _optionAText, _optionBText, _optionCText, _optionDText;
    [SerializeField] private TMP_Dropdown _answerText;

    private void OnEnable() {
        _questionText.onEndEdit.AddListener(value => UpdateField("question", value));
        _optionAText.onEndEdit.AddListener(value => UpdateField("a", value));
        _optionBText.onEndEdit.AddListener(value => UpdateField("b", value));
        _optionCText.onEndEdit.AddListener(value => UpdateField("c", value));
        _optionDText.onEndEdit.AddListener(value => UpdateField("d", value));
        _answerText.onValueChanged.AddListener(index => UpdateField("correct_answer", _answerText.options[index].text));
    }

    private void OnDisable() {
        _questionText.onEndEdit.RemoveListener(value => UpdateField("question", value));
        _optionAText.onEndEdit.RemoveListener(value => UpdateField("a", value));
        _optionBText.onEndEdit.RemoveListener(value => UpdateField("b", value));
        _optionCText.onEndEdit.RemoveListener(value => UpdateField("c", value));
        _optionDText.onEndEdit.RemoveListener(value => UpdateField("d", value));
        _answerText.onValueChanged.RemoveListener(index => UpdateField("correct_answer", _answerText.options[index].text));
    }

    public void Setup(int id, string question, string optionA, string optionB, string optionC, string optionD, string answer){
        this._id = id;
        this._question = question;
        this._optionA = optionA;
        this._optionB = optionB;
        this._optionC = optionC;
        this._optionD = optionD;
        this._answer = answer;

        _questionText.text = question;
        _optionAText.text = optionA;
        _optionBText.text = optionB;
        _optionCText.text = optionC;
        _optionDText.text = optionD;
        
        switch (answer.ToUpper())
        {
            case "A":
                _answerText.value = 0;
                break;
            case "B":
                _answerText.value = 1;
                break;
            case "C":
                _answerText.value = 2;
                break;
            case "D":
                _answerText.value = 3;
                break;
            default:
                Debug.LogWarning("Invalid answer value: " + answer);
                break;
        }
    }

    private void UpdateField(string fieldName, string value)
    {
        GameEventsManager.instance.QuizEvents.UpdateQuestion(this._id, fieldName, value);
    }
}
