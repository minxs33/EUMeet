using System;

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
}
