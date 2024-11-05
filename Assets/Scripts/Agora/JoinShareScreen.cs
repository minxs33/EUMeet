using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;

public class JoinShareScreen : MonoBehaviour
{   
    public JoinShareScreen Instance { get; private set; }
    private string _appID= "7c7db391e31044c298cce7f4ddcbe940";
    private string _channelName = "lobby";

    [SerializeField] private VideoSurface ScreenView;

    internal IRtcEngine RtcEngine;

    private bool isPushingFrames;

    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartScreenShare;
    }
    private void StartScreenShare()
    {
        SetupSDKEngine();
        InitEventHandler();
    }

    private void SetupSDKEngine(){
        try
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            RtcEngineContext context = new RtcEngineContext
            {
                appId = _appID,
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                areaCode = AREA_CODE.AREA_CODE_JP
            };
            RtcEngine.Initialize(context);
            Debug.Log("RtcEngine initialized successfully.");

            SenderOptions senderOptions = new SenderOptions();
            RtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, senderOptions);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize RtcEngine: " + ex.Message);
        }
    }

    private void InitEventHandler()
    {
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }
    void Update()
    {
        
    }

    private class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinShareScreen _joinShareScreen;

        public UserEventHandler(JoinShareScreen joinShareScreen)
        {
            this._joinShareScreen = joinShareScreen;
        }
    }
}
