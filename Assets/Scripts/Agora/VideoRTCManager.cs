using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.Networking;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    using UnityEngine.Android;
#endif

public class VideoRTCManager : MonoBehaviour
{
    public static VideoRTCManager Instance { get; private set; }
    private string _appID= "aaee4ec8cfeb477380c9ec3f477894e7";
    public string _channelName = "classroom";
    [SerializeField] private VideoSurface LocalView;
    internal IRtcEngineEx videoEngine;
    private WebCamTexture _webCamTexture;
    private WebCamDevice[] _webCamDevices;
    private uint _videoTrackId;
    private bool isPushingFrames;

    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif

    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartRTC;
        GameEventsManager.instance.RTCEvents.OnWebCamSelected += ChangeWebcam;
        GameEventsManager.instance.RTCEvents.OnUnMuteVideo += StartPushingFrame;
        GameEventsManager.instance.RTCEvents.OnMuteVideo += StopPushingFrame;
    }

    private void OnDisable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= StartRTC;
        GameEventsManager.instance.RTCEvents.OnWebCamSelected -= ChangeWebcam;
        GameEventsManager.instance.RTCEvents.OnUnMuteVideo -= StartPushingFrame;
        GameEventsManager.instance.RTCEvents.OnMuteVideo -= StopPushingFrame;
    }

    private void StartRTC()
    {
        SetupVideoSDKEngine();
        // Request and join video channel token
        StartCoroutine(GetUserToken(PlayerPrefs.GetInt("uid"), JoinVideoChannel, _channelName));
    }

    private void SetupVideoSDKEngine()
    {
        try
        {
            videoEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();

            Player[] players = FindObjectsOfType<Player>();

            VideoEventHandler handlerVideo = new VideoEventHandler(this);
            RtcEngineContext context = new RtcEngineContext
            {
                appId = _appID,
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
                areaCode = AREA_CODE.AREA_CODE_JP,
            };
            
            videoEngine.Initialize(context);
            videoEngine.InitEventHandler(handlerVideo);
            
            Debug.Log("RtcEngine initialized successfully.");

            SenderOptions senderOptions = new SenderOptions();
            videoEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, senderOptions);


        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize RtcEngine: " + ex.Message);
        }
    }
    public void JoinVideoChannel(string token, int uid)
    {
        _videoTrackId = videoEngine.CreateCustomVideoTrack();

        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelName;
        connection.localUid = (uint)uid;

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        options.publishCameraTrack.SetValue(true);
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(true);
        options.customVideoTrackId.SetValue(_videoTrackId);
        options.publishCustomVideoTrack.SetValue(true);
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
        var ret = videoEngine.JoinChannelEx(token, connection, options);

        if (ret != 0)
        {
            Debug.LogError("Failed to join video channel: " + ret);
        }

    }

    void Update()
    {
        if(videoEngine != null){
            CheckPermissions();
        }
    }

    void OnApplicationQuit()
    {
        videoEngine.Dispose();
        videoEngine = null;
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

        public void Leave()
    {
        Debug.Log("Leaving "+_channelName);
        videoEngine.DestroyCustomVideoTrack(_videoTrackId);
        // videoEngine.EnableLocalVideo(false);
        videoEngine.DisableVideo();
        videoEngine.StopPreview();
        videoEngine.StopScreenCapture();
        videoEngine.LeaveChannel();

        StopPushingFrame();

        // RemoteView.SetEnable(false);
    }

    IEnumerator GetUserToken(int uid, Action<string, int> onTokenRecieved, string channelName)
    {

        WWWForm form = new WWWForm();
        form.AddField("channelName", _channelName);
        form.AddField("uid", uid);

        using (UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-token", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string _token = www.downloadHandler.text;
                onTokenRecieved?.Invoke(_token, uid);
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
            videoEngine.EnableVideo();
            videoEngine.StartPreview();
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CUSTOM);
            LocalView.SetEnable(true);
        }
        else
        {
            Debug.LogError("LocalView is not set or not found.");
        }
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

            int pushFrameResult = videoEngine.PushVideoFrame(externalVideoFrame, _videoTrackId);
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

    internal class VideoEventHandler : IRtcEngineEventHandler
    {
        private readonly VideoRTCManager rtcSample;

        internal VideoEventHandler(VideoRTCManager rtcSample)
        {
            this.rtcSample = rtcSample;
            Debug.Log("VideoEventHandler created");
        }

        public override void OnError(int err, string msg)
        {
            Debug.LogError("Error: " + err);
        }

        // Callback triggered when the local user successfully joins the channel
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Successfully joined channel: " + connection.channelId);
            
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log("Remote user joined with channelId: " + connection.channelId);

            Player[] players = FindObjectsOfType<Player>();

            foreach (Player player in players)
            {
                Debug.Log("Checking player with UID: " + player.Uid);
                if (player.Uid == uid)
                {
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
    }
}
