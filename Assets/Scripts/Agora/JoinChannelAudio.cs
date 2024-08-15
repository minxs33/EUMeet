using Agora.Rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class JoinChannelAudio : MonoBehaviour
{
    private string _appID = "aaee4ec8cfeb477380c9ec3f477894e7";
    private string _channelName = "Lobby";
    private string _token;
    internal IRtcEngine RtcEngine;
    
    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private ArrayList permissionList = new ArrayList() { Permission.Microphone };
    #endif

    public void CheckPermissions(){
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

    private void SetupAudioSDKEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

        // Initialize context based on the latest Agora SDK requirements
        RtcEngineContext context = new RtcEngineContext();
        context.appId = _appID;
        context.areaCode = AREA_CODE.AREA_CODE_AS;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;

        RtcEngine.Initialize(context);
    }

    private void SetupUI()
    {
        GameObject go = GameObject.Find("Leave");
        go.GetComponent<Button>().onClick.AddListener(Leave);
        go = GameObject.Find("Join");
        go.GetComponent<Button>().onClick.AddListener(Join);
    }

    private void InitEventHandler()
    {
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }

    public void Join()
    {
        Debug.Log("Joining"+ _channelName);
        
        RtcEngine.EnableAudio();

        ChannelMediaOptions options = new ChannelMediaOptions();

        options.autoSubscribeAudio.SetValue(true);

        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);

        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        RtcEngine.JoinChannel(_token, _channelName, 0, options);
    }

    public void Leave()
    {
        Debug.Log("Leaving"+ _channelName);
        RtcEngine.LeaveChannel();
        RtcEngine.DisableAudio();
    }

    void Start()
    {
        _token = PlayerPrefs.GetString("token");
        SetupAudioSDKEngine();
        InitEventHandler();
        SetupUI();
    }

    void Update(){
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


    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinChannelAudio _audioSample;

        internal UserEventHandler(JoinChannelAudio audioSample)
        {
            _audioSample = audioSample;
        }

        public override void OnError(int err, string msg)
        {
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("OnJoinChannelSuccess _channelName");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log("Remote user joined");
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
        }
    }
}
