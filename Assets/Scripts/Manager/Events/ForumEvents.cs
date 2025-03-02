using System;
using Newtonsoft.Json.Linq;

public class ForumEvents
{
    public event Action<int,string,string> onAddForumPost;

    public void AddForumPost(int subject,string title, string content) {
        onAddForumPost?.Invoke(subject, title, content);
    }

    public event Action onGetSubject;
    public void GetSubjects()=>onGetSubject?.Invoke();

    public event Action OnAddForumPostSuccess;
    public void AddForumPostSuccess()=>OnAddForumPostSuccess?.Invoke();
}