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
    }

    private void OnDisable() {
        GameEventsManager.instance.levelEvents.onLevelLoad -= LoadScene;
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(this);
        }
    }

    private void Start() {
        if(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null){
            LoadScene("Lobby");
        }
    }

    public async void LoadScene(string sceneName) {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        // UI event invoke for loading animation soon

        await Task.Delay(500);
        scene.allowSceneActivation = true;
    }
}
