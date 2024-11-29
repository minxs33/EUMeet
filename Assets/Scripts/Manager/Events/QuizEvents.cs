using System;
using Newtonsoft.Json.Linq;

public class QuizEvents
{
    public event Action<String> OnAddQuiz;
    public void AddQuiz(string title) => OnAddQuiz?.Invoke(title);

    public event Action<int> OnDeleteQuiz;
    public void DeleteQuiz(int id) => OnDeleteQuiz?.Invoke(id);

    public event Action<int> OnOpenQuestion;
    public void OpenQuestion(int id) => OnOpenQuestion?.Invoke(id);

    public event Action<string, int> OnUpdateQuiz;
    public void UpdateQuiz(string title, int id) => OnUpdateQuiz?.Invoke(title, id);
}