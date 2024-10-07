using System.Collections;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.Networking;
using System;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class JoinLobbyVideo : MonoBehaviour
{
    public static JoinLobbyVideo Instance { get; private set; }
    private string _appID= "aaee4ec8cfeb477380c9ec3f477894e7";
    private string _channelName = "lobby";
    private string _token;

    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;
    private WebCamTexture _webCamTexture;
    private WebCamDevice[] _webCamDevices;
    private uint videoTrackId;

    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif

    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnWebCamSelected += ChangeWebcam;
    }

    private void OnDisable() {
        GameEventsManager.instance.RTCEvents.OnWebCamSelected -= ChangeWebcam;
    }

    void Start()
    {
        SetupVideoSDKEngine();
        InitEventHandler();
        PreviewSelf();
        SetupUI();
        StartVideo();
    }

    void Update()
    {
        CheckPermissions();
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

    private void StartVideo()
    {
        StartCoroutine(GetUserToken());
    }

    IEnumerator GetUserToken()
    {
        int uid = PlayerPrefs.GetInt("uid");

        WWWForm form = new WWWForm();
        form.AddField("channelName", _channelName);
        form.AddField("uid", uid);

        using (UnityWebRequest www = UnityWebRequest.Post("http://172.29.174.196/get-token", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                _token = www.downloadHandler.text;
                Join(_token);
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

    private void SetupUI()
    {
        GameObject go = GameObject.Find("LocalView");
        LocalView = go.AddComponent<VideoSurface>();
        // go.transform.Rotate(0.0f, 0.0f, -180.0f);
        go = GameObject.Find("RemoteView");
        RemoteView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, -180.0f);
    }

    private void SetupVideoSDKEngine()
    {
        try
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            RtcEngineContext context = new RtcEngineContext
            {
                appId = _appID,
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT
            };
            RtcEngine.Initialize(context);
            Debug.Log("RtcEngine initialized successfully.");
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

    public void Join(string _token)
    {
        uint uid = (uint)PlayerPrefs.GetInt("uid");
        LocalView.SetEnable(true);
        videoTrackId = RtcEngine.CreateCustomVideoTrack();
        
        SenderOptions senderOptions = new SenderOptions();
        int setSourceResult = RtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, senderOptions);
        if (setSourceResult != 0)
        {
            Debug.LogError("Failed to set external video source: " + setSourceResult);
            return;
        }

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        options.publishCameraTrack.SetValue(true);
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(true);
        options.customVideoTrackId.SetValue(videoTrackId);
        options.publishCustomVideoTrack.SetValue(true);
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
        RtcEngine.UpdateChannelMediaOptions(options);
        RtcEngine.JoinChannel(_token, _channelName, "", uid);
    }

    public void Leave()
    {
        Debug.Log("Leaving "+_channelName);
        RtcEngine.DestroyCustomVideoTrack(videoTrackId);
        RtcEngine.StopPreview();
        RtcEngine.LeaveChannel();
        RemoteView.SetEnable(false);
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

        _webCamTexture = new WebCamTexture(_webCamDevices[index].name,1280, 720);
        _webCamTexture.Play();
        Debug.Log("Webcam selected: " + _webCamTexture.deviceName);

        PushVideoToAgora(_webCamTexture);
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
                timestamp = (long)(Time.time * 1000)
            };

            int pushFrameResult = RtcEngine.PushVideoFrame(externalVideoFrame, videoTrackId);
            if (pushFrameResult != 0)
            {
                Debug.LogError("Failed to push video frame: " + pushFrameResult);
            }
            else
            {
                PreviewSelf();
                Debug.Log($"Pushing video frame: width={width}, height={height}, videoTrackId={videoTrackId}");
            }
        }
        else
        {
            Debug.LogError("WebCamTexture is not playing or is null");
        }
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
            // Set remote video display
            _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            // Start video rendering
            _videoSample.RemoteView.SetEnable(true);
            Debug.Log("Remote user joined with uid: " + uid);
        }

        public override void OnFirstRemoteVideoDecoded(RtcConnection connection, uint uid, int width, int height, int elapsed)
        {
            // Debug.Log($"Received first remote video frame from uid: {uid}, size: {width}x{height}");
            // _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            // _videoSample.RemoteView.SetEnable(true);
        }

        public override void OnRemoteVideoStateChanged (RtcConnection connection, uint uid, REMOTE_VIDEO_STATE state, REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            Debug.Log($"Remote video state changed: uid={uid}, state={state}, reason={reason}");

            if (state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_DECODING)
            {
                _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                _videoSample.RemoteView.SetEnable(true);
                Debug.Log($"Remote video for uid={uid} is now decoding, refreshing view.");
            }
            else if (state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED)
            {
                _videoSample.RemoteView.SetEnable(false);
                Debug.Log($"Remote video for uid={uid} has stopped.");
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.RemoteView.SetEnable(false);
            Debug.Log("Remote user offline");
        }
    }
}