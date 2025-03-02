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
    [SerializeField] private float interactionForce = 50f;
    [SerializeField] private LayerMask interactableLayer; 

    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private TextMeshPro playerNameText;
    [SerializeField] private MeshRenderer[] modelPartsMesh;
    [SerializeField] private SkinnedMeshRenderer[] modelPartsSkinnedMesh;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform characterModel;
    [SerializeField] private Transform characterHair;
    [SerializeField] private Transform ikTarget;
    [SerializeField] private Transform headRig;
    
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] public uint Uid { get; set; }
    [Networked] public string PlayerName { get; set; }
    [Networked] public string PlayerGender { get; set; }
    [Networked] public int IsDosen {get; set;}
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
    private const int ChunkSize = 250;
    QuizSync quizSync;
    Leaderboard leaderboard;
    QuizManager quizManager;
    private List<QuizManager.QuestionItem> questions;
    private int subjectId;
    private int quizId;
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
                subjectId = quizManager._subjectId;
                quizId = quizManager._quizId;

                Debug.Log("Started streaming quiz data.");

                RPC_RequestStartQuiz(quizId, subjectId);
            }else{
                Debug.LogError("QuizManager not found!");
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestStartQuiz(int quizId, int subjectId){
        Debug.Log("RPC_RequestStartQuiz");
        quizSync.Rpc_BeginQuiz(quizId, subjectId);
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
            CameraFollow.instance.SetTarget(camTarget);
            GameEventsManager.instance.UIEvents.LocalPlayerJoined();

            foreach (MeshRenderer renderer in modelPartsMesh)
            {
                renderer.enabled = false;
            }

            foreach(SkinnedMeshRenderer renderer in modelPartsSkinnedMesh)
            {
                renderer.enabled = false;
            }

            Uid = (uint)PlayerPrefs.GetInt("uid");
            PlayerName = PlayerPrefs.GetString("name");
            PlayerGender = PlayerPrefs.GetString("gender");
            IsDosen = PlayerPrefs.GetInt("isDosen");

            Debug.Log(Uid + " " + PlayerName + " " + PlayerGender + " " + IsDosen);
            Rpc_RequestPlayerPrefs(Uid, PlayerName, PlayerGender, IsDosen);
            
            lightState.RPC_RequestLightState();
        }

        kcc.Settings.ForcePredictedLookRotation = true; 
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_RequestPlayerPrefs(uint uid, string playerName, string playerGender, int IsDosen)
    {
        Debug.Log("Requesting PlayerPrefs");
        Rpc_SetPlayerAttribute(uid, playerName, playerGender, IsDosen);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SetPlayerAttribute(uint uid, string playerName, string playerGender, int IsDosen)
    {
        this.Uid = uid;
        this.PlayerName = playerName;
        if(IsDosen == 1){
            this.playerNameText.text = "<color=green>[Dosen]</color> " + playerName;
        }else{
            this.playerNameText.text = playerName;
        }
        this.IsDosen = IsDosen;
        this.PlayerGender = playerGender;

        Rpc_SetCharacterModelState(playerGender);

        Debug.Log("UID set to: " + uid + " and Player Name set to: " + playerName);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_SetCharacterModelState(string playerGender)
    {
        if (playerGender == "male")
        {
            characterModel.Find("Male").gameObject.SetActive(true);
            characterHair.Find("Male_Hair").gameObject.SetActive(true);
            characterModel.Find("Female").gameObject.SetActive(false);
            characterHair.Find("Female_Hair").gameObject.SetActive(false);
        }else{
            characterModel.Find("Female").gameObject.SetActive(true);
            characterHair.Find("Female_Hair").gameObject.SetActive(true);
            characterModel.Find("Male").gameObject.SetActive(false);
            characterHair.Find("Male_Hair").gameObject.SetActive(false);
        }

        characterModel.localRotation = kcc.LookRotation;
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
            IsDosen = IsDosen;
        }


        if(PlayerGender == "male"){
            this.characterModel.Find("Male").gameObject.SetActive(true);
            this.characterHair.Find("Male_Hair").gameObject.SetActive(true);
        }else{
            this.characterModel.Find("Female").gameObject.SetActive(true);
            this.characterHair.Find("Female_Hair").gameObject.SetActive(true);
        }

        UpdateCamTarget();
    }

    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

    // [System.Serializable]
    // public class QuestionResponseWrapper
    // {
    //     public List<QuizManager.QuestionItem> questions;
    // }
}
