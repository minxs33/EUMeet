using System;
using Newtonsoft.Json.Linq;

public class UIEvents
{

    public event Action<Boolean> onToggleOverlay;

    public void ToggleOverlay(Boolean state)
    {
        onToggleOverlay?.Invoke(state);
    }
    public event Action<JObject> onRegisterError;

    public void RegisterError(JObject error) {
        onRegisterError?.Invoke(error);
    }

    public event Action<JObject> onLoginError;

    public void LoginError(JObject error) {
        onRegisterError?.Invoke(error);
    }

}