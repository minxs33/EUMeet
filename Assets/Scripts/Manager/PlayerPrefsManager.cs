using Newtonsoft.Json.Linq;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    public static PlayerPrefsManager Instance { get; private set; }
    private void OnEnable() {
        GameEventsManager.instance.authEvents.onAuthenticate += SavePlayerPrefs;
    }

    private void OnDisable() {
        GameEventsManager.instance.authEvents.onAuthenticate -= SavePlayerPrefs;
    }

    public void SavePlayerPrefs(JObject token) {
    Debug.Log($"Token Data: {token.ToString()}");

    if (token["uid"] == null || token["name"] == null) {
        Debug.LogError("Token is missing required fields.");
        return;
    }

    PlayerPrefs.SetString("token", token["uid"].ToString());
    PlayerPrefs.SetString("name", token["name"].ToString());
}
}
