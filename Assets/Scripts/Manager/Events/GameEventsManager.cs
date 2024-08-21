using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }
    
    public AuthEvents authEvents;
    public JsonResponseEvents jsonResponseEvents;
    public LevelEvents levelEvents;
    public UIEvents UIEvents;
    public VoiceEvents voiceEvents;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        // Initialize Events
        authEvents = new AuthEvents();
        jsonResponseEvents = new JsonResponseEvents();
        levelEvents = new LevelEvents();
        UIEvents = new UIEvents();
        voiceEvents = new VoiceEvents();
    }
}
