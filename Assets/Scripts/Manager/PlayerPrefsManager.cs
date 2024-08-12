using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    private void OnEnable() {
        GameEventsManager.instance.authEvents.onAuthenticate += SavePlayerPrefs;
    }

    private void OnDisable() {
        GameEventsManager.instance.authEvents.onAuthenticate -= SavePlayerPrefs;
    }

    public void SavePlayerPrefs(JObject token) {
        // PlayerPrefs.SetString("token", token);
    }
}
