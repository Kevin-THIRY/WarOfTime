using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Référence à la prefab
    // [SerializeField] private GameObject botPrefab; // Référence à la prefab
    // [SerializeField] private GameObject terrain;

    [Header("Host/Client")]
    private NetworkObject networkObject;
    
    private void Start() {
        StartHost();
    }

    private void StartHost()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        Debug.Log("Spawn host");
        Vector3 spawnPosition = new Vector3(0, 0, 0); // Change la position selon ton besoin
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
        bool isClientOnly = NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
        Debug.Log("Is host : " + NetworkManager.Singleton.IsHost + " Is client : " + isClientOnly);
    }

    // private void OnSceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    // {
    //     Debug.Log("Hello");
    //     if (!NetworkManager.Singleton.IsServer) return;

    //     foreach (var clientId in clientsCompleted)
    //     {
    //         if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
    //             continue;

    //         var player = Instantiate(playerPrefab);
    //         player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    //     } 
    // }

    // private void SpawnHost()
    // {
    //     if (NetworkManager.Singleton.IsServer)
    //     {
    //         Debug.Log("Hello");
    //         Vector3 spawnPosition = new Vector3(0, 0, 0); // Change la position selon ton besoin
    //         GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    //         networkObject = player.GetComponent<NetworkObject>();
    //         networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
    //         // GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;

    //         GameData.playerInfos.localPlayerIndex = 0;
    //     }
    // }

    // private void OnEnable()
    // {
    //     // S'abonner au callback lorsque le client est connecté
    //     NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayers;
    // }

    private void Update()
    {
        // Debug.Log(PlayerInfos.localPlayerIndex);
    }

    // private void PrepareTerrain()
    // {
    //     terrain.SetActive(true);
    //     terrain.GetComponent<TerrainGenerator>().GenerateTerrain();
    //     ElementaryBasics.terrainGenerator = terrain.GetComponent<TerrainGenerator>();
    //     if (terrain.transform.Find("FogOfWar(Clone)") != null) terrain.transform.Find("FogOfWar(Clone)").gameObject.SetActive(false);
    //     if (terrain.transform.Find("Highlight Map(Clone)") != null) terrain.transform.Find("Highlight Map(Clone)").gameObject.SetActive(false);
    // }

    // private void SpawnPlayers(ulong clientId)
    // {
    //     bool isHost = NetworkManager.Singleton.IsHost;
    //     bool IsServer = NetworkManager.Singleton.IsServer;
    //     bool isClientOnly = NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
    //     void AddHighlightMapToPlayer(GameObject player)
    //     {
    //         terrain.GetComponent<TerrainGenerator>().GenerateHighlightPlayer(player.transform.Find("Highlight Map/Highlight").gameObject);
    //         player.GetComponentInChildren<PlayerManager>().SetMouseShaderController(player.transform.Find("Highlight Map/Highlight").gameObject.GetComponent<MouseShaderController>());
    //     }

    //     void AddFogOfWarToPlayer(GameObject player)
    //     {
    //         terrain.GetComponent<TerrainGenerator>().GenerateFogPlayer(player.transform.Find("FogOfWar/Fog").gameObject);
    //         // player.GetComponentInChildren<FogLayerManager>().SetLayer(64);
    //         player.GetComponentInChildren<FogLayerManager>().SetFogPlayer(player.transform.Find("FogOfWar/Fog/Fog of War").gameObject);
    //     }

    //     void SetInputToPlayer(GameObject player, int playerNumber)
    //     {
    //         PlayerInput playerInput = player.GetComponent<PlayerInput>();
    //         playerInput.SwitchCurrentControlScheme("BasicsInput", Keyboard.current);
    //         playerInput.SwitchCurrentActionMap("Player" + playerNumber);
    //         player.GetComponentInChildren<MovementManager>().SetInputSystem(playerInput.actions);
    //     }

    //     void InitializeCameraPlayer(GameObject player, int playerNumber)
    //     {
    //         Camera cam = player.transform
    //                             .Find("CameraBase/MainCamera")
    //                             .GetComponent<Camera>();
            
    //         cam.cullingMask = LayerMask.GetMask("Default") | LayerMask.GetMask("TransparentFX") | LayerMask.GetMask("Ignore Raycast")
    //                             & LayerMask.GetMask("Water") | LayerMask.GetMask("UI") | LayerMask.GetMask("Player" + playerNumber);
    //     }

    //     GameObject GetNetworkPlayer()
    //     {
    //         if (NetworkManager.Singleton.IsServer)
    //         {
    //             Debug.Log("Spawn host");
    //             Vector3 spawnPosition = new Vector3(0, 0, 0); // Change la position selon ton besoin
    //             GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    //             networkObject = player.GetComponent<NetworkObject>();
    //             networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
    //             // GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;

    //             GameData.playerInfos.localPlayerIndex = 0;
    //             return player;
    //         }
    //         // // if (NetworkManager.Singleton.LocalClient.PlayerObject == null && isHost)
    //         // // {
    //         // //     Vector3 spawnPosition = new Vector3(0, 0, 0); // Change la position selon ton besoin
    //         // //     GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    //         // //     networkObject = player.GetComponent<NetworkObject>();
    //         // //     networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
    //         // //     // GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;

    //         // //     GameData.playerInfos.localPlayerIndex = 0;
    //         // //     return player;
    //         // // }
    //         else
    //         {
    //             // Vector3 spawnPosition = new Vector3(0, 0, 0); // Change la position selon ton besoin
    //             // GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    //             // networkObject = player.GetComponent<NetworkObject>();
    //             // networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
    //             Debug.Log("Spawn client");
    //             // NetworkHandler netHandler = FindAnyObjectByType<NetworkHandler>();
    //             // netHandler.SpawnClientServerRpc();
    //             GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;
    //             // GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;
    //             GameData.playerInfos.localPlayerIndex = NetworkManager.Singleton.ConnectedClientsList.Count;
    //             return player;
    //         }
    //         // Debug.Log("Spawn client");
    //         // // NetworkHandler netHandler = FindAnyObjectByType<NetworkHandler>();
    //         // // netHandler.SpawnClientServerRpc();
    //         // GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;
    //         // // GameObject player = FindAnyObjectByType<NetworkObject>().gameObject;
    //         // GameData.playerInfos.localPlayerIndex = NetworkManager.Singleton.ConnectedClientsList.Count;
    //         // return player;
    //     }

    //     if (GameData.playerInfos == null)
    //     {
    //         Debug.LogWarning("Aucun joueur à instancier !");
    //         return;
    //     }

    //     GameObject player = GetNetworkPlayer();

    //     SetInputToPlayer(player, 1);
            
    //     player.GetComponentInChildren<PlayerManager>().SetTerrainGenerator(terrain.GetComponent<TerrainGenerator>());
    //     player.GetComponentInChildren<Camera>().targetDisplay = 0;

    //     AddHighlightMapToPlayer(player);
    //     AddFogOfWarToPlayer(player);

    //     int playerNumber = GameData.playerInfos.localPlayerIndex + 1;

    //     SetLayerRecursively(player, LayerMask.NameToLayer("Player" + playerNumber));

    //     InitializeCameraPlayer(player, playerNumber);

    //     // Optionnel : Modifier le nom et la couleur du joueur
    //     player.name = GameData.playerInfos.Name;
    //     // player.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;

    //     players.Add(player);

    //     Debug.Log("Is host : " + NetworkManager.Singleton.IsHost + " Is client : " + NetworkManager.Singleton.IsClient);

    //     // Only for splitscreen but useless here
    //     // List<Camera> cameras = new List<Camera>();

    //     // foreach (GameObject player in players) // players = ta liste de joueurs
    //     // {
    //     //     Camera cam = player.transform
    //     //                         .Find("CameraBase/MainCamera")
    //     //                         .GetComponent<Camera>();
            
    //     //     cam.cullingMask = LayerMask.GetMask("Joueur1");
    //     // }

    //     // SetupViewports(cameras);
    // }

    // private void SpawnBots()
    // {
    //     if (GameData.botList == null || GameData.botList.Count == 0)
    //     {
    //         Debug.LogWarning("Aucun bot à instancier !");
    //         return;
    //     }

    //     for (int i = 0; i < GameData.botList.Count; i++)
    //     {
    //         Vector3 spawnPosition = new Vector3(i * 2, 0, 0); // Change la position selon ton besoin
    //         GameObject bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

    //         // Optionnel : Modifier le nom et la couleur du joueur
    //         // bot.name = GameData.playerList[i].Name;
    //         // bot.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;
    //     }
    // }

    // private void SetLayerRecursively(GameObject obj, int newLayer)
    // {
    //     obj.layer = newLayer;

    //     foreach (Transform child in obj.transform)
    //     {
    //         SetLayerRecursively(child.gameObject, newLayer);
    //     }
    // }
}
