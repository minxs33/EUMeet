using System;
using Newtonsoft.Json.Linq;

public class QuizEvents
{
    public event Action<String> OnAddQuiz;
    public void AddQuiz(string title) => OnAddQuiz?.Invoke(title);

    public event Action<int> OnDeleteQuiz;
    public void DeleteQuiz(int id) => OnDeleteQuiz?.Invoke(id);

    public event Action OnOpenQuizQuestion;
    public void OpenQuizQuestion() => OnOpenQuizQuestion?.Invoke();

    public event Action<string, int> OnUpdateQuiz;
    public void UpdateQuiz(string title, int id) => OnUpdateQuiz?.Invoke(title, id);

    public event Action<string> OnSetTitleText;
    public void SetTitleText(string title) => OnSetTitleText?.Invoke(title);

    public event Action OnAddQuestion;
    public void AddQuestion() => OnAddQuestion?.Invoke();

    public event Action<int, string, string> OnUpdateQuestion;
    public void UpdateQuestion(int id, string field, string value) => OnUpdateQuestion?.Invoke(id, field, value);
}