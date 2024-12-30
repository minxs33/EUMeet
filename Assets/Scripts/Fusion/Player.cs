using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpluse = 8f;

    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private TextMeshPro playerNameText;
    [SerializeField] private MeshRenderer[] modelPartsMesh;
    [SerializeField] private SkinnedMeshRenderer[] modelPartsSkinnedMesh;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform characterModel;
    [SerializeField] private Transform ikTarget;
    [SerializeField] private Transform headRig;
    
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] public uint Uid { get; set; }
    [Networked] public string PlayerName { get; set; }
    
    [Networked] private Vector2 NetworkedLookRotation { get; set; }

    [Networked] public int LeaderboardScore { get; set; } = 0;
    private ChairSync currentChair;
    private LightSync lightTrigger;
    private LightSync lightState;
    private GameObject[] lightsGo;
    private NetworkInputData input;
    private InputManager inputManager;
    private Vector2 baseLookRotation;
    private bool playerNameTextSet = false;
    private bool interactDebounce = false;
    // animation paramters
    [Networked] private NetworkBool isMoving {get; set;} = false;
    [Networked] private NetworkBool isJumping {get; set;} = false;
    [Networked] private NetworkBool isSitting {get; set;} = false;
    // quiz stuff
    QuizSync quizSync;
    Leaderboard leaderboard;
    QuizManager quizManager;
    private List<QuizManager.QuestionItem> questions;
    private void OnEnable() {
        GameEventsManager.instance.QuizEvents.OnStartQuizClicked += StartQuiz;
        GameEventsManager.instance.fusionEvents.onLightToggle += ToggleLight;
    }

    private void OnDisable() {
        GameEventsManager.instance.QuizEvents.OnStartQuizClicked -= StartQuiz;
        GameEventsManager.instance.fusionEvents.onLightToggle -= ToggleLight;
    }

    private void Start() {
        
        quizManager = FindObjectOfType<QuizManager>();
        lightsGo =  GameObject.FindObjectsOfType<Transform>().Where(t => t.name == "Spot Light").Select(t => t.gameObject).ToArray();

        if (quizSync != null && !quizSync.Object.IsValid)
        {
            SceneRef currentScene = Runner.GetSceneRef(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            Runner.RegisterSceneObjects(currentScene, new NetworkObject[] {
                quizSync.Object,
                leaderboard.Object,
                lightState.Object
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
        lightState = GameObject.FindObjectOfType<LightSync>();

        if (HasInputAuthority)
        {
            GameEventsManager.instance.RTCEvents.PlayerJoined();
            CameraFollow.Singleton.SetTarget(camTarget);

            // foreach (MeshRenderer renderer in modelPartsMesh)
            // {
            //     renderer.enabled = false;
            // }

            // foreach(SkinnedMeshRenderer renderer in modelPartsSkinnedMesh)
            // {
            //     renderer.enabled = false;
            // }

            Uid = (uint)PlayerPrefs.GetInt("uid");
            PlayerName = PlayerPrefs.GetString("name");
            Rpc_RequestPlayerPrefs(Uid, PlayerName);
            
            lightState.RPC_RequestLightState();
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

            if (ikTarget != null && headRig != null){
                ikTarget.position = headRig.position + kcc.LookDirection * 5f;
            }

            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Interact) && !interactDebounce)
            {
                interactDebounce = true;
                PreviousButtons = input.Buttons;
                InteractChair();
                InteractLight();
            }else if(!input.Buttons.WasPressed(PreviousButtons, InputButton.Interact)){
                interactDebounce = false;
            }

            if (currentChair != null && currentChair.IsOccupied && currentChair.OccupyingPlayer == this) {
                Vector3 chairPosition = currentChair.transform.position;
                kcc.SetGravity(0f);
                
                isSitting = true;

                if(currentChair.IsDosen){
                    Vector3 targetPosition = new Vector3(chairPosition.x, chairPosition.y, chairPosition.z);
                    kcc.SetPosition(targetPosition);
                }else{
                    Vector3 targetPosition = new Vector3(chairPosition.x - 0.15f, chairPosition.y, chairPosition.z);
                    kcc.SetPosition(targetPosition);
                }
            }else{
                    isSitting = false;

                Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
                float jump = 0f;
                kcc.SetGravity(Physics.gravity.y * 2f);

                isMoving = worldDirection != Vector3.zero;

                if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump) && kcc.IsGrounded){
                    jump = jumpImpluse;
                }

                if(!kcc.IsGrounded){
                    isJumping = true;
                }else{
                    isJumping = false;
                }

                kcc.Move(worldDirection.normalized * speed, jump);
            }

            PreviousButtons = input.Buttons;
        }

        if(HasStateAuthority){
            RpcSyncAnimationState(isSitting, isJumping, isMoving, ikTarget.position);
        }

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSyncAnimationState(bool isSitting, bool isJumping, bool isMoving, Vector3 targetPosition)
    {
        animator.SetBool("IsSitting", isSitting);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsMoving", isMoving);
        ikTarget.position = targetPosition;
    }

    private void InteractChair(){
        if(currentChair != null && HasInputAuthority)
        {
            currentChair.RPC_ToggleOccupancy(this);
        }
    }

    private void InteractLight(){
        if(lightTrigger != null && HasInputAuthority)
        {
            lightTrigger.RPC_ToggleLight();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out ChairSync chair) && currentChair == null){
            currentChair = chair;
            Debug.Log("Chair triggered: " + currentChair);
        }else if(other.TryGetComponent(out LightSync lightSync) && lightTrigger == null){
            lightTrigger = lightSync;
            Debug.Log("Light triggered: " + lightTrigger);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.TryGetComponent(out ChairSync chair) && currentChair == chair){
            currentChair = null;
            Debug.Log("Chair exited: " + currentChair);
        }else if(other.TryGetComponent(out LightSync lightSync) && lightTrigger == lightSync){
            lightTrigger = null;
            Debug.Log("Light exited: " + lightTrigger);
        }
    }

    private void ToggleLight(bool isOn){
        Debug.Log("Toggling light: " + isOn);
        foreach (var light in lightsGo)
        {
            var lightComponent = light.GetComponent<Light>();
            lightComponent.intensity = isOn ? 58f : 5f;
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
