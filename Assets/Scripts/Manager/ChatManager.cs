using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    ChatClient chatClient;
    private string _username;
    private bool _isConnected = false;
    private string _whisper = "";
    [SerializeField] private GameObject _chatGo;

    private void OnEnable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined += StartChat;
        GameEventsManager.instance.RTCEvents.OnSendChat += SendChat;
    }

    private void OnDisable() {
        GameEventsManager.instance.RTCEvents.OnPlayerJoined -= StartChat;
        GameEventsManager.instance.RTCEvents.OnSendChat -= SendChat;
    }

    private void StartChat() {
        _username = PlayerPrefs.GetString("name");
        _isConnected = true;
        chatClient = new ChatClient(this);
        chatClient.ChatRegion = "JP";
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, new AuthenticationValues(_username));
    }

    private void Update() {
        if(_isConnected){
            chatClient.Service();
        }
    }

    private void SendChat(string message){
        Debug.Log(message); 
        if(_whisper == ""){
            chatClient.PublishMessage("lobby", message);
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        
    }

    public void OnChatStateChange(ChatState state)
    {
        
    }

    public void OnConnected()
    {
        var ret = chatClient.Subscribe("lobby");
        if (!ret)
        {
            Debug.LogError("Subscribe failed");
            return;
        }
        Debug.Log("Connected");
    }

    public void OnDisconnected()
    {
        
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        string messageTexts = "";
        for (int i = 0; i < senders.Length; i++){
            messageTexts = string.Format("{0}: {1}", senders[i],    messages[i]);

            LobbyUIManager.Instance.AddMessage(messageTexts);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        
    }

    public void OnUnsubscribed(string[] channels)
    {
        
    }

    public void OnUserSubscribed(string channel, string user)
    {
        
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        
    }

}