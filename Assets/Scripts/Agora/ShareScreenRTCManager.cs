#if !UNITY_SERVER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    using UnityEngine.Android;
#endif

public class ShareScreenRTCManager : MonoBehaviour
{
    public static ShareScreenRTCManager Instance { get; private set; }
    private string _appID= "aaee4ec8cfeb477380c9ec3f477894e7";
    public string _channelNameScreen = "classroom_screen";
    internal IRtcEngine shareScreenEngine;
    [SerializeField] private GameObject windowCardPrefab;
    [SerializeField] private Transform windowCardParent;
    
    private ScreenCaptureSourceInfo[] _screenCaptureSourceInfos;
    private IEnumerable<ScreenCaptureSourceInfo> windowSources;
    private IEnumerable<ScreenCaptureSourceInfo> screenSources;
    private ScreenCardID selectedCaptureOptions;
    private uint _uid = 0;

    private void OnEnable() {
        // Screen Share
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartRTC;
        GameEventsManager.instance.RTCEvents.OnToggleSelectCaptureType += SelectCapture;
        GameEventsManager.instance.RTCEvents.OnPublishCapture += PublishCapture;
        GameEventsManager.instance.RTCEvents.OnUnPublishCapture += UnPublishCapture;
    }

    private void OnDisable() {   
        // Screen Share
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= StartRTC;
        GameEventsManager.instance.RTCEvents.OnToggleSelectCaptureType -= SelectCapture;
        GameEventsManager.instance.RTCEvents.OnPublishCapture -= PublishCapture;
        GameEventsManager.instance.RTCEvents.OnUnPublishCapture -= UnPublishCapture;
    }

    private void StartRTC()
    {
        SetupVideoSDKEngine();

        // Request and join screen share channel token with different UID
        StartCoroutine(GetUserToken(PlayerPrefs.GetInt("uid") + 1, JoinShareScreenChannel));
    }

    void OnApplicationQuit()
    {
        shareScreenEngine.Dispose();
        shareScreenEngine = null;
        Leave();
    }

     public void Leave()
    {
        Debug.Log("Leaving "+_channelNameScreen);
        shareScreenEngine.LeaveChannel();
        shareScreenEngine.StopScreenCapture();
        shareScreenEngine.DisableVideo();
        shareScreenEngine.DisableAudio();
    }


     IEnumerator GetUserToken(int uid, Action<string> onTokenRecieved)
    {

        WWWForm form = new WWWForm();
        form.AddField("channelName", _channelNameScreen);
        form.AddField("uid", uid);

        using (UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-token", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string _token = www.downloadHandler.text;
                _uid = (uint)uid;
                onTokenRecieved?.Invoke(_token);
            }
            else
            {
                Debug.LogError("Error receiving token: " + www.error);
            }
        }
    }

    private void SetupVideoSDKEngine()
    {
        try
        {
            shareScreenEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();


            // ShareScreenEventHandler handlerShareScreen = new ShareScreenEventHandler(this);
            
            LogConfig Log = new LogConfig
            {
                filePath = "Agora Log",
                fileSizeInKB = 2048,
                level = LOG_LEVEL.LOG_LEVEL_ERROR,
            };
            
            RtcEngineContext context = new RtcEngineContext
            {
                appId = _appID,
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM,
                areaCode = AREA_CODE.AREA_CODE_JP,
                logConfig = Log,
            };

            shareScreenEngine.Initialize(context);
            Debug.Log("RtcEngine initialized successfully.");
            
            SetupShareScreenConfig();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize RtcEngine: " + ex.Message);
        }
    }

     public void JoinShareScreenChannel(string token){
        
        ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(false);
            options.autoSubscribeVideo.SetValue(true);
            options.publishCameraTrack.SetValue(false);
            options.publishScreenTrack.SetValue(true);
            options.enableAudioRecordingOrPlayout.SetValue(false);

        #if UNITY_ANDROID || UNITY_IPHONE
                    options.publishScreenCaptureAudio.SetValue(false);
                    options.publishScreenCaptureVideo.SetValue(true);
        #endif

        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        var ret = shareScreenEngine.JoinChannel(token, _channelNameScreen, _uid, options);
        if (ret != 0)
        {
            Debug.LogError("Failed to join sharescreen channel: " + ret);
        }
        else
        {
            SetupScreenCaptureList();
        }
    }
    private void SetupShareScreenConfig(){
        VideoEncoderConfiguration encoder = new VideoEncoderConfiguration{
            dimensions = new VideoDimensions(1280, 720),
            frameRate = 60,
            bitrate = 5000,
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE
        };

        shareScreenEngine.SetVideoEncoderConfiguration(encoder);
        // shareScreenEngine.EnableAudio();
        shareScreenEngine.EnableVideo();
        shareScreenEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }
    
    private void SetupScreenCaptureList(){
        
        if (windowCardParent == null || shareScreenEngine == null) return;
        
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

        _screenCaptureSourceInfos = shareScreenEngine.GetScreenCaptureSources(thumbSize, iconSize, true);

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

    private void PublishCapture(){
        if (selectedCaptureOptions == null)
        {
            Debug.LogWarning("No capture options selected to publish.");
            return;
        }


        #if UNITY_ANDROID || UNITY_IPHONE
            var parameters2 = new ScreenCaptureParameters2();
            parameters2.captureAudio = true;
            parameters2.captureVideo = true;
            var nRet = RtcEngine.StartScreenCapture(parameters2);
            Debug.Log("Publishing mobile screen capture: " + nRet);
        #else
            

            if (selectedCaptureOptions.type == "ScreenCaptureSourceType_Window")
            {
                var nRet = shareScreenEngine.StartScreenCaptureByWindowId(selectedCaptureOptions.sourceId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
                Debug.Log("Publishing window capture with sourceId: " + selectedCaptureOptions.sourceId);
            }
            else if (selectedCaptureOptions.type == "ScreenCaptureSourceType_Screen")
            {
                var nRet = shareScreenEngine.StartScreenCaptureByDisplayId((uint)selectedCaptureOptions.sourceId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
                Debug.Log("Publishing screen capture with sourceId: " + selectedCaptureOptions.sourceId);
            }
        #endif

        // RtcConnection connection = new RtcConnection();
        // connection.channelId = _channelNameScreen;
        // connection.localUid = _uid;

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(true);
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        #if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(true);
            options.publishScreenCaptureVideo.SetValue(true);
        #endif

        // #if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        //     shareScreenEngine.EnableLoopbackRecording(false, "");
        // #endif

        shareScreenEngine.UpdateChannelMediaOptions(options);

        
        GameEventsManager.instance.RTCEvents.ToggleCaptureSelected(false);
        
        MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
    }

    private void UnPublishCapture(){
        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelNameScreen;
        connection.localUid = _uid;

        shareScreenEngine.StopScreenCapture();
        DestroyVideoView(0);
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(false);

        #if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(false);
            options.publishScreenCaptureVideo.SetValue(false);
        #endif

        var ret = shareScreenEngine.UpdateChannelMediaOptions(options);
        Debug.Log("UpdateChannelMediaOptions returns: " + ret);
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
            selectedButton.onClick.AddListener(() => {
                selectedCaptureOptions = captureOptions;
                GameEventsManager.instance.RTCEvents.ToggleCaptureSelected(true);
            });
        }

        if (source.thumbImage != null)
        {
            Texture2D texture = new Texture2D((int)source.thumbImage.width, (int)source.thumbImage.height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(source.thumbImage.buffer);
            texture.Apply();

            thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            thumbnail.transform.localScale = new Vector3(1, -1, 1);
        }

        sourceName.text = source.sourceName;

    }

    internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE)
    {
        var go = GameObject.Find(uid.ToString());
        if(!ReferenceEquals(go, null)) 
        {
            return;
        }

        var videoSurface = MakePlaneSurface(uid.ToString());
        if (ReferenceEquals(videoSurface, null)) return;

        videoSurface.SetForUser(uid, channelId, videoSourceType);
        videoSurface.SetEnable(true);

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            var goParent = GameObject.Find("ScreenBoard");
            if (goParent != null)
            {
                var parentRenderer = goParent.GetComponent<Renderer>();
                if (parentRenderer != null)
                {

                    videoSurface.transform.localScale = new Vector3(0.1f, -1f, 0.1f);
                }
                else
                {
                    videoSurface.transform.localScale = new Vector3(width, -1, height);
                }
            }

            Debug.Log("OnTextureSizeModify: " + width + "x" + height);
        };

        Debug.Log("Video view created for uid: " + uid);
    }

    internal static void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Destroy(go);
        }
    }

    private static VideoSurface MakePlaneSurface(string goName)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        
        var goParent = GameObject.Find("ScreenBoard");
        if (goParent != null)
        {
            go.transform.SetParent(goParent.transform);

            // Get the parent's bounds to calculate width and height constraints
            var parentRenderer = goParent.GetComponent<Renderer>();
            if (parentRenderer != null)
            {
                go.transform.localScale = new Vector3(0.1f, -0.1f, 0.1f);
            }
        }
        
        go.transform.Rotate(-90f, 0.0f, 180f);
        go.transform.localPosition = new Vector3(0, 0, -0.51f);

        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
}
#endif