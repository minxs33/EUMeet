using System;
using Newtonsoft.Json.Linq;

public class AuthEvents
{
    public event Action<String,String,String> onRegister;
    public void Register(String name,String email,String password) {
        onRegister?.Invoke(name,email,password);
    }

    public event Action onRegisterSuccess;
    public void RegisterSuccess() {
        onRegisterSuccess?.Invoke();
    }

    public event Action onRegisterFailed;
    public void RegisterFailed() {
        onRegisterFailed?.Invoke();
    }

    public event Action<String,String> onLogin;
    public void Login(String email,String password) {
        onLogin?.Invoke(email,password);
    }

    public event Action<JObject> onAuthenticate;
    public void Authenticate(JObject response) {
        onAuthenticate?.Invoke(response);
    }
}
