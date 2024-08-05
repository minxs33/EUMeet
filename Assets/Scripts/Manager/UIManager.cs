using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private void OnEnable() {
        GameEventsManager.instance.authEvents.onRegisterSuccess += RegisterSuccess;
    }

    private void OnDisable() {
        GameEventsManager.instance.authEvents.onRegisterSuccess -= RegisterSuccess;
    }

    public void RegisterSuccess() {
        // Show UI for validation
    }
}
