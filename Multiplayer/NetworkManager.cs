using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    [Header("Settings")]
    public string roomName = "TestRoom";

    // The prefab for the player character (Must have a NetworkObject component!)
    public NetworkPrefabRef playerPrefab;

    private NetworkRunner _runner;

    public int gameSceneBuildIndex = 2;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent != null)
        {
            Debug.LogWarning("[NetworkManager] NetworkManager is not a root object! Detaching parent.");
            transform.SetParent(null);
        }
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        // Only log error if the Singleton instance (the "real" one) is destroyed
        if (Instance == this)
        {
            Debug.LogError($"[NetworkManager] I was destroyed! Stack Trace: {System.Environment.StackTrace}");
        }
    }

    async void Start()
    {
        // Detect Server Mode (Headless)
        if (Application.isBatchMode)
        {
            Debug.Log("--- HEADLESS SERVER DETECTED: AUTO-STARTING ---");
            // Server starts and FORCES the Game Scene to load
            await StartGame(GameMode.Server, gameSceneBuildIndex);
        }


        // ===============

        // // 1. Setup the Runner
        // if (_runner == null)
        // {
        //     _runner = gameObject.AddComponent<NetworkRunner>();
        // }

        // // 2. Detect Mode (Headless Server vs Client)
        // var mode = GameMode.Client;
        // if (Application.isBatchMode) // Headless Server Check
        // {
        //     mode = GameMode.Server;
        //     Debug.Log("--- STARTING FUSION SERVER (HEADLESS) ---");
        //     Application.targetFrameRate = 30;
        // }
        // else
        // {
        //     Debug.Log("--- STARTING FUSION CLIENT ---");
        // }

        // var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        // var sceneInfo = new NetworkSceneInfo();
        // if (scene.IsValid)
        // {
        //     sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        // }

        // // 3. Start the Game
        // // In Fusion, "StartGame" handles connecting, creating rooms, and loading scenes.
        // await _runner.StartGame(new StartGameArgs()
        // {
        //     GameMode = mode,
        //     SessionName = roomName,
        //     Scene = scene,
        //     SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        // });
    }

    public async System.Threading.Tasks.Task StartGame(GameMode mode, int sceneIndex)
    {
        if (_runner != null && _runner.IsRunning)
        {
            Debug.LogWarning("[NetworkManager] Runner is already running! Ignoring StartGame call.");
            return;
        }

        Debug.Log($"[NetworkManager] StartGame called with mode: {mode}");

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        _runner.ProvideInput = true;

        // Create Scene Info for the specific Game Scene we want to play in
        var scene = SceneRef.FromIndex(sceneIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogError($"[NetworkManager] SceneRef for Index {gameSceneBuildIndex} is INVALID! Check Build Settings.");
        }

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = scene, // Fusion will load this scene for Server & sync Clients to it
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    // --- FUSION CALLBACKS ---

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} Joined!");

        // Only the Server spawns objects
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3(0, 0, 0);

            // Spawn the Player Avatar
            NetworkObject networkPlayer = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

            runner.SetPlayerObject(player, networkPlayer);

            // Keep track of the player object if needed
            _spawnedCharacters.Add(player, networkPlayer);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} Left");

        // --- NEW: Remove the "Ghost" when they leave ---
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }
    /// <summary>
    /// This is where we capture LOCAL input and send it to Fusion.
    /// Fusion calls this automatically.
    /// </summary>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Check if Keyboard is connected (Avoids errors if no keyboard)
        if (Keyboard.current != null)
        {
            // 1. Capture Movement (Construct Vector2 manually from keys)
            Vector2 moveDir = Vector2.zero;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                moveDir.y += 1;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                moveDir.y -= 1;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                moveDir.x -= 1;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                moveDir.x += 1;

            data.direction = moveDir;

            // // 2. Capture Buttons (Space for Jump)
            // if (Keyboard.current.spaceKey.isPressed)
            // {
            //     data.buttons.Set(InputButtons.Jump, true);
            // }
        }

        // 3. Pass it to Fusion
        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { Debug.Log("Connected to Server"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { Debug.Log("Disconnected from Server"); }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
