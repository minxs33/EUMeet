using System;
using Newtonsoft.Json.Linq;

public class LevelEvents
{
    public event Action<String> onLevelLoad;
    public void LevelLoad(string sceneName) {
        onLevelLoad?.Invoke(sceneName);
    }

    public event Action onLogout;

    public void Logout() {
        onLogout?.Invoke();
    }

}
