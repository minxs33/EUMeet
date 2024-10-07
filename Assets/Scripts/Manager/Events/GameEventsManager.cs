using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }
    
    public AuthEvents authEvents;
    public JsonResponseEvents jsonResponseEvents;
    public LevelEvents levelEvents;
    public UIEvents UIEvents;
    public RTCEvents RTCEvents;

    private void Awake() {
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);

            authEvents = new AuthEvents();
            jsonResponseEvents = new JsonResponseEvents();
            levelEvents = new LevelEvents();
            UIEvents = new UIEvents();
            RTCEvents = new RTCEvents();
        } else if(instance != this) {
            Destroy(gameObject);
        }
    }

}
