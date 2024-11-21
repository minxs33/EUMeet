using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;

// public class EventHandler : IRtcEngineEventHandler
// {
//     // Start is called before the first frame update
//     private readonly VideoRTCManager rtcSample;

//     internal EventHandler(VideoRTCManager rtcSample)
//     {
//         this.rtcSample = rtcSample;
//         Debug.Log("VideoEventHandler created");
//     }

//     public override void OnError(int err, string msg)
//     {
//         Debug.LogError("Error: " + err);
//     }

//     // Callback triggered when the local user successfully joins the channel
//     public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
//     {
//         Debug.Log("Successfully joined channel: " + connection.channelId);
        
//     }

//     public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
//     {
//         Debug.Log("Remote user joined with channelId: " + connection.channelId);
//         var players = rtcSample.GetPlayers();
//         foreach (Player player in players)
//         {
//             Debug.Log("Checking player with UID: " + player.Uid);
//             if (player.Uid == uid)
//             {
//                 // Assign video surface to this player
//                 Debug.Log("Remote user joined with uid: " + uid);
//                 VideoSurface playerVideoSurface = player.gameObject.GetComponentInChildren<VideoSurface>();
//                 if(playerVideoSurface != null){
//                     Debug.Log("Setting remote video for player: " + player.PlayerName);
//                     playerVideoSurface.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
//                     playerVideoSurface.SetEnable(true);
//                 }else{
//                     Debug.LogError("VideoSurface not found for player: " + player.PlayerName);
//                 }
//                 break;
//             }
//         }
//     }

//     public override void OnRemoteVideoStateChanged (RtcConnection connection, uint uid, REMOTE_VIDEO_STATE state, REMOTE_VIDEO_STATE_REASON reason, int elapsed)
//     {
//         // Debug.Log($"Remote video state changed: uid={uid}, state={state}, channel={connection.channelId} reason={reason} this is event handler");

//         // if(connection.channelId != rtcSample._channelName){
//         //     if(uid != rtcSample._uid){
//         //         if ((int)state == (int)REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_DECODING)
//         //         {
//         //             ShareScreenRTCManager.MakeVideoView(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
//         //             Debug.Log($"Remote video for uid={uid} is now starting, refreshing view.");
//         //         }
//         //         else if ((int)state == (int)REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED)
//         //         {
//         //             // _videoSample.RemoteView.SetEnable(false);
//         //             ShareScreenRTCManager.DestroyVideoView(uid);
//         //             Debug.Log($"Remote video for uid={uid} has stopped.");
//         //         }
//         //     }
//         // }else{
//         //     var player = rtcSample.GetPlayer(uid);
//         //     if (player.GetComponent<Player>().Uid == uid && uid != rtcSample._uid)
//         //     {
//         //         if((int)state == (int)REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_DECODING){
                    
//         //             rtcSample.MakeVideoView(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                
//         //         }else if((int)state == (int)REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED){
                    
//         //             VideoRTCManager.DestroyVideoView(uid);
                
//         //         }
//         //     }
//         // }
//     }
// }
