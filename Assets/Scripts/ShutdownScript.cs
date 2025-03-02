using UnityEngine;
using System.Collections;
using Fusion;
using Agora.Rtc;
public class ShutdownScript : MonoBehaviour
{
    public IRtcEngine rtcEngine;
    public IRtcEngineEx rtcEngineEx;
    public NetworkRunner runner;

    private static ShutdownScript instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        GameEventsManager.instance.levelEvents.onLogout += OnApplicationQuit;
    }

    private void OnDisable() {
        GameEventsManager.instance.levelEvents.onLogout -= OnApplicationQuit;
    }

    private void Start() {
        runner = FindObjectOfType<NetworkRunner>();
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(CleanShutdown());
    }

    public IEnumerator CleanShutdown()
    {
        // Shutdown Agora
        if (rtcEngine != null)
        {
            Debug.Log("Releasing Agora resources...");
            rtcEngine.LeaveChannel();
            rtcEngine.StopPreview();
            rtcEngine.DisableAudio();
            rtcEngine.DisableVideo();
            rtcEngine.Dispose();
            rtcEngine = null;
        }

        if(rtcEngineEx != null)
        {
            Debug.Log("Releasing Agora resources...");
            rtcEngineEx.LeaveChannel();
            rtcEngineEx.StopPreview();
            rtcEngineEx.DisableAudio();
            rtcEngineEx.DisableVideo();
            rtcEngineEx.Dispose();
            rtcEngineEx = null;
        }
        // Shutdown Fusion
        if (runner != null)
        {
            Debug.Log("Shutting down Photon Fusion...");
            yield return runner.Shutdown();
            runner = null;
        }

        Debug.Log("Shutdown complete. Exiting application...");
        Application.Quit();
    }
}
