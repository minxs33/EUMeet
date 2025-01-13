using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    public static PlayerPrefsManager Instance { get; private set; }
    private void OnEnable() {
        GameEventsManager.instance.authEvents.onAuthenticate += SavePlayerPrefs;
        GameEventsManager.instance.authEvents.onSaveToken += SaveToken;
        GameEventsManager.instance.authEvents.onSaveGender += SaveGender;
    }


    private void OnDisable() {
        GameEventsManager.instance.authEvents.onAuthenticate -= SavePlayerPrefs;
        GameEventsManager.instance.authEvents.onSaveToken -= SaveToken;
        GameEventsManager.instance.authEvents.onSaveGender -= SaveGender;
    }

    public void SavePlayerPrefs(JObject token) {
        Debug.Log($"Token Data: {token.ToString()}");

        if (token["uid"] == null || token["name"] == null || token["isDosen"] == null) {
            Debug.LogError("Token is missing required fields.");
            return;
        }

        PlayerPrefs.SetInt("uid", token["uid"].Value<int>());
        PlayerPrefs.SetString("name", token["name"].ToString());
        if(token["isDosen"].Value<int>() == 1){
            PlayerPrefs.SetInt("isDosen", 1);
        }else{
            PlayerPrefs.SetInt("isDosen", 0);
        }
    }

    private void SaveToken(string token)
    {
        PlayerPrefs.SetString("token", token);
    }

    private void SaveGender(string gender){
        PlayerPrefs.SetString("gender", gender);
    }
}
