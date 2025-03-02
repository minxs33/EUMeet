using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private NetworkRunner runner;
    private void OnEnable() {
        GameEventsManager.instance.levelEvents.onLevelLoad += LoadScene;
    }

    private void OnDisable() {
        GameEventsManager.instance.levelEvents.onLevelLoad -= LoadScene;
    }

    private void Awake() {
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);

        } else if(Instance != this) {
            Destroy(gameObject);
            return;
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

        await Task.Delay(200);
        scene.allowSceneActivation = true;
    }

}
