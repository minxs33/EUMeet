using System;
using Agora.Rtc;
using UnityEngine;

public class RTCEvents
{

    public event Action OnPlayerJoined;

    public void PlayerJoined()
    {
        OnPlayerJoined?.Invoke();
    }

    // public event Action OnVideoRTCConnected;

    // public void VideoRTCConnected()
    // {
    //     OnVideoRTCConnected?.Invoke();
    // }
    public event Action OnUpdateWebRTCTarget;

    public void UpdateWebRTCTarget()
    {
        OnUpdateWebRTCTarget?.Invoke();
    }

    public event Action OnUnMuteVoice;

    public void UnMuteVoice()
    {
        OnUnMuteVoice?.Invoke();
    }

    public event Action OnMuteVoice;

    public void MuteVoice()
    {
        OnMuteVoice?.Invoke();
    }
    
    public event Action<DeviceInfo[]> OnCameraDeviceUpdated;

    public void CameraDeviceUpdated(DeviceInfo[] deviceInfo)
    {
        OnCameraDeviceUpdated?.Invoke(deviceInfo);
    }

    public event Action<int> OnWebCamSelected;

    public void WebCamSelected(int webCamIndex)
    {
        OnWebCamSelected?.Invoke(webCamIndex);
    }

    public event Action OnUnMuteVideo;

    public void UnMuteVideo(){
        OnUnMuteVideo?.Invoke();
    }

    public event Action OnMuteVideo;

    public void MuteVideo(){
        OnMuteVideo?.Invoke();
    }
    
    public event Action<bool> OnToggleSelectCaptureType;

    public void ToggleSelectCaptureType(bool state){
        OnToggleSelectCaptureType?.Invoke(state);
    }

    public event Action<bool> OnToggleCaptureSelected;

    public void ToggleCaptureSelected(bool state){
        OnToggleCaptureSelected?.Invoke(state);
    }

    public enum CaptureStates{
        Started,
        Stopped
    }

    public event Action<CaptureStates> OnCaptureState;


    public void CaptureState(CaptureStates state){
        OnCaptureState?.Invoke(state);
    }

    public event Action OnPublishCapture;

    public void PublishCapture(){
        OnPublishCapture?.Invoke();
    }
    
    public event Action OnUnPublishCapture;

    public void UnPublishCapture(){
        OnUnPublishCapture?.Invoke();
    }

    public event Action<string> OnSendChat;

    public void SendChat(string message){
        OnSendChat?.Invoke(message);
    }
}
