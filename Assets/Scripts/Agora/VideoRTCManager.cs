
#if !UNITY_SERVER
using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using Unity.VisualScripting;
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
    public IVideoDeviceManager deviceManager;
    private DeviceInfo[] _videoDevices;
    private uint _videoTrackId;
    private bool isPushingFrames = false;
    private string _selectedWebcamDevice;
    public uint _uid;

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

            // VideoEventHandler handlerVideo = new VideoEventHandler(this);
            deviceManager = videoEngine.GetVideoDeviceManager();
            EventHandler eventHandler = new EventHandler(this);
            RtcEngineContext context = new RtcEngineContext
            {
                appId = _appID,
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
                areaCode = AREA_CODE.AREA_CODE_JP,
            };
            
            videoEngine.Initialize(context);
            // videoEngine.InitEventHandler(handlerVideo);
            videoEngine.InitEventHandler(eventHandler);
            
            Debug.Log("RtcEngine initialized successfully.");

            SenderOptions senderOptions = new SenderOptions();

            UpdateVideoDevices();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize RtcEngine: " + ex.Message);
        }
    }

    public Player[] GetPlayers(){
        return FindObjectsOfType<Player>();
    }

    public GameObject GetPlayer(uint Uid){
        var players = GetPlayers();
        foreach (Player player in players)
        {
            if (player.Uid == Uid)
            {
                return player.gameObject;
            }
        }
        return null;
    }

    public void JoinVideoChannel(string token, int uid)
    {
        _uid = (uint)uid;
        RtcConnection connection = new RtcConnection();
        connection.channelId = _channelName;
        connection.localUid = _uid;

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        options.publishCameraTrack.SetValue(true);
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(true);
        // options.publishCustomVideoTrack.SetValue(true);
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
        var ret = videoEngine.JoinChannelEx(token, connection, options);

        if (ret != 0)
        {
            Debug.LogError("Failed to join video channel: " + ret);
        }else{
            StopPushingFrame();
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
        // videoEngine.EnableLocalVideo(false);
        videoEngine.DisableVideo();
        videoEngine.StopPreview();
        videoEngine.StopScreenCapture();
        videoEngine.LeaveChannel();

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
            videoEngine.StartPreview();
            videoEngine.EnableVideo();
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
            LocalView.transform.localScale = new Vector3(1f, -1f, 1f);
            LocalView.SetEnable(true);
        }
        else
        {
            Debug.LogError("LocalView is not set or not found.");
        }
    }

    private void UpdateVideoDevices(){
        _videoDevices = deviceManager.EnumerateVideoDevices();

        if (_videoDevices == null || _videoDevices.Length == 0)
        {
            Debug.LogError("No video devices found!");
        }
        else
        {
            Debug.Log($"Found {_videoDevices.Length} video devices.");
            GameEventsManager.instance.RTCEvents.CameraDeviceUpdated(_videoDevices);
        }
    }

    public void ChangeWebcam(int index)
    {
        if (videoEngine == null) return;

        if (_videoDevices == null || index < 0 || index >= _videoDevices.Length)
        {
            Debug.LogError("Invalid device index.");
            return;
        }

        _selectedWebcamDevice = _videoDevices[index].deviceId;
        Debug.Log($"Switching to device: {_videoDevices[index].deviceName} (ID: {_selectedWebcamDevice})");

        if (isPushingFrames)
        {
            StopPushingFrame();
            StartCoroutine(DelayedStartPushingFrame());
        }
    }

    private IEnumerator DelayedStartPushingFrame()
    {
        yield return new WaitForEndOfFrame();

        StartPushingFrame();
    }

    private void StartPushingFrame()
    {
        int webcamSwitchResult = deviceManager.SetDevice(_selectedWebcamDevice);
        isPushingFrames = true;

        if (webcamSwitchResult != 0)
        {
            Debug.LogError($"Error switching webcam: {webcamSwitchResult}");
            return;
        }

        PreviewSelf();
    }

    private void StopPushingFrame()
    {
        isPushingFrames = false;

        videoEngine.StopPreview();
        videoEngine.EnableLocalVideo(false);
        LocalView.SetEnable(false);

        Debug.Log("Stopped pushing frames and disabled video.");
    }

    internal void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE)
    {
        var go = GameObject.Find(uid.ToString() + "face");
        if (go != null) return;

        var videoSurface = MakeQuadSurface(uid.ToString() + "face", uid);
        if (videoSurface == null) return;

        videoSurface.SetForUser(uid, channelId, videoSourceType);
        videoSurface.SetEnable(true);

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            var player = GetPlayer(uid);
            if (player != null)
            {
                var playerGo = player.gameObject;
                var goVisual = playerGo.transform.Find("Visual");
                var goParent = goVisual.Find("face");
                if (goParent != null)
                {
                    var parentRenderer = goParent.GetComponent<RectTransform>();
                    videoSurface.transform.localScale = parentRenderer != null 
                        ? Vector3.one 
                        : new Vector3(1, -1, 1);
                }
            }

            Debug.Log($"OnTextureSizeModify: {width}x{height}");
        };

        Debug.Log($"Video view created for UID: {uid}");
    }

    private VideoSurface MakeQuadSurface(string goName, uint uid)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        if (go == null) return null;

        go.name = goName;

        var player = GetPlayer(uid);
        if (player == null)
        {
            Debug.LogWarning($"Player with UID {uid} not found.");
            return null;
        }

        var playerGo = player.gameObject;
        var goVisual = playerGo.transform.Find("Visual");
        if (goVisual == null)
        {
            Debug.LogError("Visual GameObject not found!");
            return null;
        }

        var goParent = goVisual.Find("face");
        if (goParent == null)
        {
            Debug.LogError("Face GameObject not found!");
            return null;
        }

        go.transform.SetParent(goParent, false);
        go.transform.localPosition = new Vector3(0,0,-0.53f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        return go.AddComponent<VideoSurface>();
    }
    
    internal static void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString() + "face");
        if (go != null)
        {
            Debug.Log($"Destroying video view for UID: {uid}");
            Destroy(go);
        }
        else
        {
            Debug.LogWarning($"Attempted to destroy non-existent video view for UID: {uid}");
        }
    }

    internal class EventHandler : IRtcEngineEventHandler
    {
        // Start is called before the first frame update
        private readonly VideoRTCManager rtcSample;

        internal EventHandler(VideoRTCManager rtcSample)
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
                if (player.Uid == uid)
                {
                    var videoFeed = player.gameObject.transform.Find("Visual/face/VideoFeed");
                    Debug.Log("Remote user joined with uid: " + uid);
                    VideoSurface playerVideoSurface = videoFeed.GetComponent<VideoSurface>();
                    MeshRenderer playerMeshRenderer = videoFeed.GetComponent<MeshRenderer>();
                    if(playerVideoSurface != null && playerMeshRenderer != null){
                        Debug.Log("Setting remote video for player: " + player.PlayerName);
                        playerVideoSurface.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                        playerVideoSurface.SetEnable(true);
                        playerMeshRenderer.enabled = false;
                        
                    }else{
                        Debug.LogError("VideoSurface not found for player: " + player.PlayerName);
                    }
                    break;
                }
            }
        }

        public override void OnRemoteVideoStateChanged (RtcConnection connection, uint uid, REMOTE_VIDEO_STATE state, REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            Debug.Log($"Remote video state changed: uid={uid}, state={state}, channel={connection.channelId} reason={reason} this is event handler");

            if(connection.channelId != rtcSample._channelName){
                if(uid != rtcSample._uid){
                    if ((int)state == (int)REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_DECODING)
                    {
                        ShareScreenRTCManager.MakeVideoView(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                        Debug.Log($"Remote video for uid={uid} is now starting, refreshing view.");
                    }
                    else if ((int)state == (int)REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED)
                    {
                        // _videoSample.RemoteView.SetEnable(false);
                        ShareScreenRTCManager.DestroyVideoView(uid);
                        Debug.Log($"Remote video for uid={uid} has stopped.");
                    }
                }
            }else{
                if (uid != rtcSample._uid)
                {
                    
                    if(reason == REMOTE_VIDEO_STATE_REASON.REMOTE_VIDEO_STATE_REASON_REMOTE_UNMUTED || reason == REMOTE_VIDEO_STATE_REASON.REMOTE_VIDEO_STATE_REASON_LOCAL_MUTED){
                        var player = rtcSample.GetPlayer(uid);
                        var videoFeed = player.transform.Find("Visual/face/VideoFeed");
                        MeshRenderer playerMeshRenderer = videoFeed.GetComponent<MeshRenderer>();

                        Debug.Log("Setting remote video for uid: "+uid);
                        
                        if(state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_DECODING){
                            
                            playerMeshRenderer.enabled = true;
                            Debug.Log("Turning on video object");
                        
                        }else if(state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED){
                            
                            playerMeshRenderer.enabled = false;
                            Debug.Log("Turning off video object");
                        
                        }
                    }
                }
            }
        }
    }
}
#endif