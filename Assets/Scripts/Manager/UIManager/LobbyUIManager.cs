using Fusion;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance { get; private set; }

    [SerializeField] public Button _toggleMuteButton;

    public NetworkRunner _runner;

    private void OnEnable() {
        GameEventsManager.instance.voiceEvents.OnPlayerJoined += MuteVoice;
    }

    private void OnDisable() {
        GameEventsManager.instance.voiceEvents.OnPlayerJoined -= MuteVoice;
    }

    private void MuteVoice()
    {
    //    _runner.GetComponent<Recorder>().TransmitEnabled = false;
    }
}
