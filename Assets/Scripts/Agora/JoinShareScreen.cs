using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Schema;
using Agora.Rtc;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JoinShareScreen : MonoBehaviour
{   
    public JoinShareScreen Instance { get; private set; }
    private string _appID= "7c7db391e31044c298cce7f4ddcbe940";
    private string _channelName = "classroom_screen";
    private string _token;

    [SerializeField] private GameObject windowCardPrefab;
    [SerializeField] private Transform windowCardParent;
    [SerializeField] private GameObject previewCapture;

    internal IRtcEngineEx RtcEngine;
    private ScreenCaptureSourceInfo[] _screenCaptureSourceInfos;
    private IEnumerable<ScreenCaptureSourceInfo> windowSources;
    private IEnumerable<ScreenCaptureSourceInfo> screenSources;

    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnVideoRTCConnected += StartScreenShare;
        GameEventsManager.instance.RTCEvents.OnToggleSelectCaptureType += SelectCapture;
        GameEventsManager.instance.RTCEvents.OnPublishCapture += PublishCapture;
        GameEventsManager.instance.RTCEvents.OnUnPublishCapture += UnPublishCapture;
    }


    private void OnDisable() {
        GameEventsManager.instance.RTCEvents.OnVideoRTCConnected -= StartScreenShare;
        GameEventsManager.instance.RTCEvents.OnToggleSelectCaptureType -= SelectCapture;
        GameEventsManager.instance.RTCEvents.OnPublishCapture -= PublishCapture;
        GameEventsManager.instance.RTCEvents.OnUnPublishCapture -= UnPublishCapture;
    }

    private void StartScreenShare()
    {
        SetupSDKEngine();
        InitEventHandler();
        SetupScreenCaptureList();
        Join();
        SetupVideoConfig();
    }

    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            Leave();
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }

    public void Leave()
    {
        Debug.Log("Leaving "+_channelName);
        // RtcEngine.EnableLocalVideo(false);
        
        RtcEngine.StopScreenCapture();
        RtcEngine.LeaveChannel();

        // RemoteView.SetEnable(false);
    }

    private void SetupSDKEngine(){
        try
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
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

    private void Join(){
        _token = PlayerPrefs.GetString("token");
        uint uid = (uint)PlayerPrefs.GetInt("uid");

        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelName;
        connection.localUid = uid;

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);

        #if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(true);
            options.publishScreenCaptureVideo.SetValue(true);
        #endif
        
        var ret = RtcEngine.JoinChannelEx(_token, connection, options);

        if (ret != 0)
        {
            Debug.LogError("Failed to join channel: " + ret);
        }
        else
        {
            Debug.Log("Successfully joined screenshare channel: " + _channelName);
        }
    }

    private void InitEventHandler()
    {
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }

    private void SetupVideoConfig(){
        VideoEncoderConfiguration encoder = new VideoEncoderConfiguration{
            dimensions = new VideoDimensions(1280, 720),
            frameRate = 60,
            bitrate = 5000,
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE
        };

        RtcEngine.SetVideoEncoderConfiguration(encoder);
    }

    private void SetupScreenCaptureList(){
        
        if (windowCardParent == null || RtcEngine == null) return;
        
        foreach (Transform child in windowCardParent)
        {
            Destroy(child.gameObject);
        }

        SIZE thumbSize = new SIZE
        {
            width = 1280,
            height = 720
        };

        SIZE iconSize = new SIZE
        {
            width = 640,
            height = 640
        };

        _screenCaptureSourceInfos = RtcEngine.GetScreenCaptureSources(thumbSize, iconSize, true);

        windowSources = _screenCaptureSourceInfos.Where(w => w.type == ScreenCaptureSourceType.ScreenCaptureSourceType_Window);
        screenSources = _screenCaptureSourceInfos.Where(w => w.type == ScreenCaptureSourceType.ScreenCaptureSourceType_Screen);

        ShowWindows();
    }

    private void SelectCapture(bool state)
    {
        if (state)
        {
            ShowWindows();
        }
        else
        {
            ShowScreens();
        }
    }

    private void PublishCapture()
    {
        MakeVideoView(0, "classroom", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
        Debug.Log("Publishing screen");
    }

    private void UnPublishCapture(){
        RtcEngine.StopScreenCapture();
        DestroyVideoView(0);
        Debug.Log("Unpublishing screen");
    }

    private void ShowWindows()
    {
        UpdateCards(windowSources);
    }

    private void ShowScreens()
    {
        UpdateCards(screenSources);
    }

    private void UpdateCards(IEnumerable<ScreenCaptureSourceInfo> sources)
    {
        foreach (Transform child in windowCardParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var source in sources)
        {
            CreateCard(source);
        }
    }

    private void CreateCard(ScreenCaptureSourceInfo source)
    {

        var card = Instantiate(windowCardPrefab, windowCardParent);
        var thumbnail = card.transform.Find("Thumbnail").GetComponent<Image>();
        var sourceName = card.transform.Find("SourceName").GetComponent<TextMeshProUGUI>();

        var captureOptions = card.GetComponent<ScreenCardID>();
        captureOptions.sourceId = source.sourceId;
        captureOptions.type = source.type.ToString();

        var selectedButton = card.GetComponent<Button>();

        if (selectedButton != null)
        {
            selectedButton.onClick.AddListener(() => StartCapture(captureOptions));
        }

        if (source.thumbImage != null)
        {
            Texture2D texture = new Texture2D((int)source.thumbImage.width, (int)source.thumbImage.height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(source.thumbImage.buffer);
            texture.Apply();

            thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        sourceName.text = source.sourceName;

    }

    private void StartCapture(ScreenCardID captureOptions)
    {
        #if UNITY_ANDROID || UNITY_IPHONE
            var parameters2 = new ScreenCaptureParameters2();
            parameters2.captureAudio = true;
            parameters2.captureVideo = true;
            var nRet = RtcEngine.StartScreenCapture(parameters2);
            this.Log.UpdateLog("StartScreenCapture :" + nRet);
        #else
            RtcEngine.StopScreenCapture();
            if (captureOptions == null) return;

            Debug.Log($"Starting capture for sourceId: {captureOptions.sourceId}, type: {captureOptions.type}");

            if(captureOptions.type == "ScreenCaptureSourceType_Window")
            {
                var nRet = RtcEngine.StartScreenCaptureByWindowId(captureOptions.sourceId, default(Rectangle),
                    default(ScreenCaptureParameters));
            }
            else if(captureOptions.type == "ScreenCaptureSourceType_Screen")
            {
               var nRet = RtcEngine.StartScreenCaptureByDisplayId((uint)captureOptions.sourceId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 60 });
            }
            
            
        #endif
        GameEventsManager.instance.RTCEvents.ToggleCaptureSelected(true);
    }

    internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return;
        }

        var videoSurface = GameObject.Find("Screen").GetComponent<VideoSurface>();
        if (ReferenceEquals(videoSurface, null)) return;
        // configure videoSurface
        videoSurface.SetForUser(uid, channelId, videoSourceType);
        videoSurface.SetEnable(true);

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            var transform = videoSurface.GetComponent<RectTransform>();

            float scale = (float)height / (float)width;
            videoSurface.transform.localScale = new Vector3(1, -1, scale);
            Debug.Log("OnTextureSizeModify: " + width + "  " + height);
        };
    }

    internal static void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find("Screen").GetComponent<VideoSurface>();
        if (!ReferenceEquals(go, null))
        {
            go.SetEnable(false);
            go.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
        }
    }


    // Callbacks
    private class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinShareScreen _joinShareScreen;

        public UserEventHandler(JoinShareScreen joinShareScreen)
        {
            this._joinShareScreen = joinShareScreen;
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            
            JoinShareScreen.DestroyVideoView(connection.localUid);
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            JoinShareScreen.MakeVideoView(uid, _joinShareScreen._channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            JoinShareScreen.DestroyVideoView(uid);
        }
    }
}
