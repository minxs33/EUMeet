using System;
using Newtonsoft.Json.Linq;

public class UIEvents
{
    public event Action<JObject> onRegisterError;

    public void RegisterError(JObject error) {
        onRegisterError?.Invoke(error);
    }
}