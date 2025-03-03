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
    public FusionEvents fusionEvents;
    public QuizEvents QuizEvents;
    public SoundEvents soundEvents;
    public ForumEvents forumEvents;

    private void Awake() {
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);

            authEvents = new AuthEvents();
            jsonResponseEvents = new JsonResponseEvents();
            levelEvents = new LevelEvents();
            UIEvents = new UIEvents();
            RTCEvents = new RTCEvents();
            QuizEvents = new QuizEvents();
            fusionEvents = new FusionEvents();
            soundEvents = new SoundEvents();
            forumEvents = new ForumEvents();

        } else if(instance != this) {
            Destroy(gameObject);
        }
    }

}
