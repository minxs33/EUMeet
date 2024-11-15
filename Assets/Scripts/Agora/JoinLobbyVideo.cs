#if !UNITY_SERVER
using Agora.Rtc;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    using UnityEngine.Android;
#endif

public class JoinLobbyVideo : MonoBehaviour
{

    public static JoinLobbyVideo Instance { get; private set; }
    private string _appID= "aaee4ec8cfeb477380c9ec3f477894e7";
    public string _channelName = "classroom";
    [SerializeField] private VideoSurface LocalView;
    // internal VideoSurface RemoteView;
    internal IRtcEngineEx RtcEngine;
    private WebCamTexture _webCamTexture;
    private WebCamDevice[] _webCamDevices;
    private uint _videoTrackId;
    private bool isPushingFrames;

    // Share Screen
    [SerializeField] private GameObject windowCardPrefab;
    [SerializeField] private Transform windowCardParent;
    
    private ScreenCaptureSourceInfo[] _screenCaptureSourceInfos;
    private IEnumerable<ScreenCaptureSourceInfo> windowSources;
    private IEnumerable<ScreenCaptureSourceInfo> screenSources;
    private ScreenCardID selectedCaptureOptions;
    private uint _uid1 = 0;
    private uint _uid2 = 0;
    

    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif

    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartRTC;
        GameEventsManager.instance.RTCEvents.OnWebCamSelected += ChangeWebcam;
        GameEventsManager.instance.RTCEvents.OnUnMuteVideo += StartPushingFrame;
        GameEventsManager.instance.RTCEvents.OnMuteVideo += StopPushingFrame;

        // Screen Share
        GameEventsManager.instance.RTCEvents.OnToggleSelectCaptureType += SelectCapture;
        GameEventsManager.instance.RTCEvents.OnPublishCapture += PublishCapture;
        GameEventsManager.instance.RTCEvents.OnUnPublishCapture += UnPublishCapture;
    }

    private void OnDisable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= StartRTC;
        GameEventsManager.instance.RTCEvents.OnWebCamSelected -= ChangeWebcam;
        GameEventsManager.instance.RTCEvents.OnUnMuteVideo -= StartPushingFrame;
        GameEventsManager.instance.RTCEvents.OnMuteVideo -= StopPushingFrame;
        
        // Screen Share
        GameEventsManager.instance.RTCEvents.OnToggleSelectCaptureType -= SelectCapture;
        GameEventsManager.instance.RTCEvents.OnPublishCapture -= PublishCapture;
        GameEventsManager.instance.RTCEvents.OnUnPublishCapture -= UnPublishCapture;
    }

    private void StartRTC()
    {
        SetupVideoSDKEngine();
        SetupShareScreenConfig();
        ChangeWebcam(0);   
        _uid1 = (uint)PlayerPrefs.GetInt("uid");
        _uid2 = (uint)PlayerPrefs.GetInt("uid") + 1;

        StartCoroutine(GetUserToken(_uid1, JoinVideoChannel));

        // Request and join screen share channel token with different UID
        StartCoroutine(GetUserToken(_uid2, JoinShareScreenChannel));
    }

    void Update()
    {
        if(RtcEngine != null){
            CheckPermissions();
        }
    }

    void OnApplicationQuit()
    {
        RtcEngine.Dispose();
        RtcEngine = null;
        Leave();
    }

    private void CheckPermissions() {
    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            foreach (string permission in permissionList)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermission(permission);
                }
            }
    #endif
    }

    IEnumerator GetUserToken(uint uid, Action<string> onTokenRecieved)
    {

        WWWForm form = new WWWForm();
        form.AddField("channelName", _channelName);
        form.AddField("uid", (int)uid);

        using (UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-token", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string _token = www.downloadHandler.text;
                onTokenRecieved?.Invoke(_token);
            }
            else
            {
                Debug.LogError("Error receiving token: " + www.error);
            }
        }
    }
    
    private void PreviewSelf()
    {
        if (LocalView != null)
        {
            RtcEngine.EnableVideo();
            RtcEngine.StartPreview();
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CUSTOM);
            LocalView.SetEnable(true);
        }
        else
        {
            Debug.LogError("LocalView is not set or not found.");
        }
    }

    private void SetupVideoSDKEngine()
    {
        try
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();

            Player[] players = FindObjectsOfType<Player>();

            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext
            {
                appId = _appID,
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
                areaCode = AREA_CODE.AREA_CODE_JP,
            };
            
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            
            Debug.Log("RtcEngine initialized successfully.");

            SenderOptions senderOptions = new SenderOptions();
            RtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, senderOptions);


        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize RtcEngine: " + ex.Message);
        }
    }


    public void JoinVideoChannel(string token)
    {
        _videoTrackId = RtcEngine.CreateCustomVideoTrack();

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        options.publishCameraTrack.SetValue(true);
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(true);
        options.customVideoTrackId.SetValue(_videoTrackId);
        options.publishCustomVideoTrack.SetValue(true);
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
        var ret = RtcEngine.JoinChannel(token, _channelName, _uid1, options);

        if (ret != 0)
        {
            Debug.LogError("Failed to join video channel: " + ret);
        }

    }

    public void JoinShareScreenChannel(string token){
        // Share Screen

        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelName;
        connection.localUid = _uid2;
        
        ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(false);
            options.autoSubscribeVideo.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            options.publishScreenTrack.SetValue(true);
            options.enableAudioRecordingOrPlayout.SetValue(false);

        #if UNITY_ANDROID || UNITY_IPHONE
                    options.publishScreenCaptureAudio.SetValue(true);
                    options.publishScreenCaptureVideo.SetValue(true);
        #endif

        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        var ret = RtcEngine.JoinChannelEx(token, connection, options);
        if (ret != 0)
        {
            Debug.LogError("Failed to join sharescreen channel: " + ret);
        }
        else
        {
            SetupScreenCaptureList();
        }
    }

    public void Leave()
    {
        Debug.Log("Leaving "+_channelName);
        if(RtcEngine == null) return;

        RtcEngine.DestroyCustomVideoTrack(_videoTrackId);
        // RtcEngine.EnableLocalVideo(false);
        RtcEngine.DisableVideo();
        RtcEngine.StopPreview();
        RtcEngine.StopScreenCapture();
        RtcEngine.LeaveChannel();

        
        StopPushingFrame();

        // RemoteView.SetEnable(false);
    }

    public void ChangeWebcam(int index)
    {
        _webCamDevices = WebCamTexture.devices;
        if (_webCamDevices == null || _webCamDevices.Length == 0)
        {
            Debug.LogError("No webcam devices found.");
            return;
        }

        if (index >= _webCamDevices.Length)
        {
            Debug.LogError("WebCam index out of range.");
            return;
        }

        if (_webCamTexture != null && _webCamTexture.isPlaying)
        {
            _webCamTexture.Stop();
        }

        _webCamTexture = new WebCamTexture(_webCamDevices[index].name);
        _webCamTexture.requestedWidth = 640;
        _webCamTexture.requestedHeight = 480;
        _webCamTexture.requestedFPS = 30;
        Debug.Log("Webcam selected: " + _webCamTexture.deviceName);
        if(isPushingFrames){
            StartPushingFrame();
        }
    }

    private void StartPushingFrame(){
        isPushingFrames = true;
        _webCamTexture.Play();
        LocalView.SetEnable(true);
        StartCoroutine(PushFrames());
    }

    private void StopPushingFrame(){
        isPushingFrames = false;
        if(_webCamTexture != null) _webCamTexture.Stop();
        LocalView.SetEnable(false);
        StopCoroutine(PushFrames());
    }

    IEnumerator PushFrames()
    {
       while (isPushingFrames)
        {
            PushVideoToAgora(_webCamTexture);
            yield return new WaitForSeconds(1f / 30f);
        }
    }

    private void PushVideoToAgora(WebCamTexture webCamTexture)
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            int width = webCamTexture.width;
            int height = webCamTexture.height;
            
            // Get pixels from the WebCamTexture
            Color32[] pixels = webCamTexture.GetPixels32();
            
            // Create a byte array for the video data (4 bytes per pixel: RGBA)
            byte[] videoData = new byte[width * height * 4];

            // Manually copy each pixel's RGBA data to the byte array
            for (int i = 0; i < pixels.Length; i++)
            {
                videoData[i * 4] = pixels[i].r;     // Red
                videoData[i * 4 + 1] = pixels[i].g; // Green
                videoData[i * 4 + 2] = pixels[i].b; // Blue
                videoData[i * 4 + 3] = pixels[i].a; // Alpha
            }

            // Prepare the external video frame
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame
            {
                type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA,
                format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA,
                buffer = videoData,
                stride = width,
                height = height,
                timestamp = (long)(Time.time * 1000),
                eglContext = IntPtr.Zero,
                eglType = 0,
                textureId = 0,
                d3d11Texture2d = IntPtr.Zero,
                textureSliceIndex = 0,
            };

            int pushFrameResult = RtcEngine.PushVideoFrame(externalVideoFrame, _videoTrackId);
            if (pushFrameResult != 0)
            {
                Debug.LogError("Failed to push video frame: " + pushFrameResult);
            }
            else
            {
                PreviewSelf();
                Debug.Log($"Pushing video frame: width={width}, height={height}");
            }
        }
        else
        {
            Debug.LogError("WebCamTexture is not playing or is null");
            // give feedback texture
        }
        
    }

    // Share Screen

    private void SetupShareScreenConfig(){
        VideoEncoderConfiguration encoder = new VideoEncoderConfiguration{
            dimensions = new VideoDimensions(1280, 720),
            frameRate = 60,
            bitrate = 5000,
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE
        };

        RtcEngine.SetVideoEncoderConfiguration(encoder);
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
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
                var nRet = RtcEngine.StartScreenCaptureByWindowId(selectedCaptureOptions.sourceId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
                Debug.Log("Publishing window capture with sourceId: " + selectedCaptureOptions.sourceId);
            }
            else if (selectedCaptureOptions.type == "ScreenCaptureSourceType_Screen")
            {
                var nRet = RtcEngine.StartScreenCaptureByDisplayId((uint)selectedCaptureOptions.sourceId, default(Rectangle),
                    new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
                Debug.Log("Publishing screen capture with sourceId: " + selectedCaptureOptions.sourceId);
            }
        #endif

        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelName;
        connection.localUid = _uid2;

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(false);
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);
        options.enableAudioRecordingOrPlayout.SetValue(false);

        #if UNITY_ANDROID || UNITY_IPHONE
        options.publishScreenCaptureAudio.SetValue(true);
        options.publishScreenCaptureVideo.SetValue(true);
        #endif

        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        #if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(true);
            options.publishScreenCaptureVideo.SetValue(true);
        #endif

        #if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            RtcEngine.EnableLoopbackRecording(true, "");
        #endif

        var ret = RtcEngine.UpdateChannelMediaOptionsEx(options, connection);
        GameEventsManager.instance.RTCEvents.ToggleCaptureSelected(false);
        MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
    }

    private void UnPublishCapture(){
        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelName;
        connection.localUid = _uid2;

        RtcEngine.StopScreenCapture();
        DestroyVideoView(0);
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishCameraTrack.SetValue(true);
        options.publishScreenTrack.SetValue(false);

        #if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(false);
            options.publishScreenCaptureVideo.SetValue(false);
        #endif

        var ret = RtcEngine.UpdateChannelMediaOptionsEx(options, connection);
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

    internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN)
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

            var mesh = go.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
            
            go.transform.Rotate(-90f, 0.0f, 180f);
            go.transform.position = goParent.transform.position + new Vector3(0, 0, -0.51f);

            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

    // Callback class
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinLobbyVideo _videoSample;

        internal UserEventHandler(JoinLobbyVideo videoSample)
        {
            _videoSample = videoSample;
            Debug.Log("UserEventHandler created");
        }

        // Callback triggered when an error occurs
        public override void OnError(int err, string msg)
        {
            Debug.LogError("Error: " + err);
        }

        // Callback triggered when the local user successfully joins the channel
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Successfully joined channel: " + connection.channelId);

            
        }

        // OnUserJoined callback is triggered when the SDK receives and successfully decodes the first frame of remote video
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log("Remote user joined with channelId: " + connection.channelId);
            bool isPlayerVideoUID = false;

            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                Debug.Log("Checking player with UID: " + player.Uid);
                if (player.Uid == uid)
                {
                    isPlayerVideoUID = true;
                    // Assign video surface to this player
                    Debug.Log("Remote user joined with uid: " + uid);
                    VideoSurface playerVideoSurface = player.gameObject.GetComponentInChildren<VideoSurface>();
                    if(playerVideoSurface != null){
                        Debug.Log("Setting remote video for player: " + player.PlayerName);
                        playerVideoSurface.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                        playerVideoSurface.SetEnable(true);
                    }else{
                        Debug.LogError("VideoSurface not found for player: " + player.PlayerName);
                    }
                    break;
                }
            }

            if(!isPlayerVideoUID && uid != _videoSample._uid1 && uid != _videoSample._uid2){
                MakeVideoView(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }
        }

        public override void OnRemoteVideoStateChanged (RtcConnection connection, uint uid, REMOTE_VIDEO_STATE state, REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            Debug.Log($"Remote video state changed: uid={uid}, state={state}, channel={connection.channelId} reason={reason}");

            if (state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_DECODING)
            {
                // _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                // _videoSample.RemoteView.SetEnable(true);
                Debug.Log($"Remote video for uid={uid} is now decoding, refreshing view.");
            }
            else if (state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED)
            {
                // _videoSample.RemoteView.SetEnable(false);
                Debug.Log($"Remote video for uid={uid} has stopped.");
            }
        }

        public override void OnUserMuteVideo(RtcConnection connection, uint uid, bool muted)
        {
            Debug.Log($"User {uid} has stopped publishing video on channel {connection.channelId}");
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            // _videoSample.RemoteView.SetEnable(false);
            if (uid != _videoSample._uid1 && uid != _videoSample._uid2)
            {
                JoinLobbyVideo.DestroyVideoView(uid);
            }
            Debug.Log("Remote user offline");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            Debug.Log("Left channel");
            JoinShareScreen.DestroyVideoView(connection.localUid);
        }
    }
}
#endif