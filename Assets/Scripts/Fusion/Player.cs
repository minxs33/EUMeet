using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpluse = 10f;

    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private TextMeshPro playerNameText;
    [SerializeField] private MeshRenderer[] modelParts;
    
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] public uint Uid { get; set; }
    [Networked] public string PlayerName { get; set; }
    
    [Networked] private Vector2 NetworkedLookRotation { get; set; }

    [Networked] public int LeaderboardScore { get; set; } = 0;
    private ChairSync currentChair;
    private NetworkInputData input;
    private InputManager inputManager;
    private Vector2 baseLookRotation;
    private bool playerNameTextSet = false;
    private bool interactDebounce = false;
    // quiz stuff
    QuizSync quizSync;
    Leaderboard leaderboard;
    QuizManager quizManager;
    private List<QuizManager.QuestionItem> questions;
    private void OnEnable() {
        GameEventsManager.instance.QuizEvents.OnStartQuizClicked += StartQuiz;
    }

    private void OnDisable() {
        GameEventsManager.instance.QuizEvents.OnStartQuizClicked -= StartQuiz;
    }

    private void Start() {
        
        quizManager = FindObjectOfType<QuizManager>();

        if (quizSync != null && !quizSync.Object.IsValid)
        {
            SceneRef currentScene = Runner.GetSceneRef(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            Runner.RegisterSceneObjects(currentScene, new NetworkObject[] {
                quizSync.Object,
                leaderboard.Object    
            });
            
            Debug.Log("Registering scene objects");
        }
    }
    private void StartQuiz(){

        if (HasInputAuthority)
        {
            if (quizManager != null)
            {
                questions = quizManager.questions;

                string serializedQuestions = JsonUtility.ToJson(new QuestionResponseWrapper
                {
                    questions = questions
                });

                RPC_RequestStartQuiz(serializedQuestions);
                Debug.Log("Start Quiz");
            }
            else
            {
                Debug.LogError("QuizManager not found!");
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestStartQuiz(string serializedQuestions){
        Debug.Log("RPC_RequestStartQuiz");
        quizSync.RPC_BeginQuiz(serializedQuestions);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_AddScore(int score)
    {
        LeaderboardScore += score;
        Debug.Log($"Score updated on State Authority for {Object.InputAuthority}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_ResetScore()
    {
        LeaderboardScore = 0;
        Debug.Log($"Score reset for player {PlayerName}");
    }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        input = new NetworkInputData();

        inputManager = Runner.GetComponent<InputManager>();
        inputManager.LocalPlayer = this;
        LeaderboardScore = 0;

        quizSync = FindObjectOfType<QuizSync>();

        if (HasInputAuthority)
        {
            GameEventsManager.instance.RTCEvents.PlayerJoined();
            CameraFollow.Singleton.SetTarget(camTarget);

            foreach (MeshRenderer renderer in modelParts)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            Uid = (uint)PlayerPrefs.GetInt("uid");
            PlayerName = PlayerPrefs.GetString("name");
            Rpc_RequestPlayerPrefs(Uid, PlayerName);
        }

        kcc.Settings.ForcePredictedLookRotation = true; 
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_RequestPlayerPrefs(uint uid, string playerName)
    {
        Debug.Log("Requesting PlayerPrefs");
        Rpc_SetPlayerAttribute(uid, playerName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SetPlayerAttribute(uint uid, string playerName)
    {
        this.Uid = uid;
        this.PlayerName = playerName;
        this.playerNameText.text = playerName;
        Debug.Log("UID set to: " + uid + " and Player Name set to: " + playerName);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out input))
        {
            baseLookRotation += input.LookDelta * lookSensitivity;
            NetworkedLookRotation = baseLookRotation;

            kcc.AddLookRotation(input.LookDelta * lookSensitivity);
            UpdateCamTarget();

            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Interact) && !interactDebounce)
            {
                interactDebounce = true;
                PreviousButtons = input.Buttons;
                InteractChair();
            }else if(!input.Buttons.WasPressed(PreviousButtons, InputButton.Interact)){
                interactDebounce = false;
            }

            if (currentChair != null && currentChair.IsOccupied){
                Vector3 chairPosition = currentChair.transform.position;
                kcc.SetGravity(0f);
                Vector3 targetPosition = new Vector3(chairPosition.x, chairPosition.y - 0.2f, chairPosition.z);
                kcc.SetPosition(targetPosition);
            }else{
                Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
                float jump = 0f;
                kcc.SetGravity(Physics.gravity.y * 2f);

                if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump) && kcc.IsGrounded)
                {
                    jump = jumpImpluse;
                }

                kcc.Move(worldDirection.normalized * speed, jump);
            }

            PreviousButtons = input.Buttons;
        }

    }

    private void InteractChair(){
        if(currentChair != null && HasInputAuthority)
        {
            currentChair.RPC_ToggleOccupancy(this);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out ChairSync chair) && currentChair == null){
            currentChair = chair;
            Debug.Log("Chair triggered: " + currentChair);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.TryGetComponent(out ChairSync chair) && currentChair == chair){
            currentChair = null;
            Debug.Log("Chair exited: " + currentChair);
        }
    }

    public override void Render()
    {
        // Apply networked rotation to all clients to ensure synchronization
        if (!HasInputAuthority)
        {
            kcc.SetLookRotation(NetworkedLookRotation);
        }

        if (!playerNameTextSet && !string.IsNullOrEmpty(PlayerName))
        {
            playerNameText.text = PlayerName;
            playerNameTextSet = true;
        }

        UpdateCamTarget();
    }

    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

    [System.Serializable]
    public class QuestionResponseWrapper
    {
        public List<QuizManager.QuestionItem> questions;
    }
}
