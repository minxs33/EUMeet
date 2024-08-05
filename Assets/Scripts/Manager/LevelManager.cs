using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private void OnEnable() {
        GameEventsManager.instance.levelEvents.onLevelLoad += LoadScene;
        GameEventsManager.instance.levelEvents.onLevelLoadWithTextParams += LoadSceneWithTextParams;
    }

    private void OnDisable() {
        GameEventsManager.instance.levelEvents.onLevelLoad -= LoadScene;
        GameEventsManager.instance.levelEvents.onLevelLoadWithTextParams -= LoadSceneWithTextParams;
    }

    public async void LoadScene(string sceneName) {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        // UI event invoke for loading animation soon

        await Task.Delay(500);
        scene.allowSceneActivation = true;
    }

    public async void LoadSceneWithTextParams(string sceneName, JObject text) {
        
        PlayerPrefs.SetString("error", text.ToString());

        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        while (!scene.isDone)
        {
            await Task.Yield();
        }
        scene.allowSceneActivation = true;
    }
}
