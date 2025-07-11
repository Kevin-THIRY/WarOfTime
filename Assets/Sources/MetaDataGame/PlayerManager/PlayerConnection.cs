using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerConnection : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();
    private TerrainGenerator terrain;
    
    private void Start() {
        PrepareTerrain();
        SpawnPlayers();
    }

    private void Update()
    {
        // Debug.Log(PlayerInfos.localPlayerIndex);
    }

    private void PrepareTerrain()
    {
        terrain = FindAnyObjectByType<TerrainGenerator>();
        terrain.GenerateTerrain();
        ElementaryBasics.terrainGenerator = terrain;
        if (terrain.transform.Find("FogOfWar(Clone)") != null) terrain.transform.Find("FogOfWar(Clone)").gameObject.SetActive(false);
        if (terrain.transform.Find("Highlight Map(Clone)") != null) terrain.transform.Find("Highlight Map(Clone)").gameObject.SetActive(false);
        if (terrain.transform.Find("UnlockedFog(Clone)") != null) terrain.transform.Find("UnlockedFog(Clone)").gameObject.SetActive(false);
    }

    private void SpawnPlayers()
    {
        void AddHighlightMapToPlayer()
        {
            terrain.GenerateHighlightPlayer(transform.Find("Highlight Map/Highlight").gameObject);
            // gameObject.GetComponentInChildren<PlayerManager>().SetMouseShaderController(transform.Find("Highlight Map/Highlight").gameObject.GetComponent<MouseShaderController>());
        }

        void AddFogOfWarToPlayer()
        {
            terrain.GenerateFogPlayer(transform.Find("FogOfWar/Fog").gameObject);
            // gameObject.GetComponentInChildren<FogLayerManager>().SetFogPlayer(transform.Find("FogOfWar/Fog/Fog of War").gameObject);
        }

        void AddUnlockedFogToPlayer()
        {
            terrain.GeneratUnlockedPlayer(transform.Find("UnlockedFog/Fog").gameObject);
            // gameObject.GetComponentInChildren<FogLayerManager>().SetFogPlayer(transform.Find("FogOfWar/Fog/Fog of War").gameObject);
        }

        void SetInputToPlayer()
        {
            PlayerInput playerInput = gameObject.GetComponent<PlayerInput>();
            playerInput.SwitchCurrentControlScheme("BasicsInput", Keyboard.current);
            playerInput.SwitchCurrentActionMap("Player" + 1);
            MovementManager.instance.SetInputSystem(playerInput.actions);
        }

        void InitializeCameraPlayer(int playerNumber)
        {
            Camera cam = transform.Find("CameraBase/MainCamera")
                            .GetComponent<Camera>();
            
            cam.cullingMask = LayerMask.GetMask("Default") | LayerMask.GetMask("TransparentFX") | LayerMask.GetMask("Ignore Raycast")
                                | LayerMask.GetMask("Water") | LayerMask.GetMask("UI") | LayerMask.GetMask("Player" + playerNumber)
                                | LayerMask.GetMask("VisibleToPlayer");
        }

        if (GameData.playerInfos == null)
        {
            Debug.LogWarning("Aucun joueur à instancier !");
            return;
        }

        SetInputToPlayer();

        gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;

        AddHighlightMapToPlayer();
        AddFogOfWarToPlayer();
        AddUnlockedFogToPlayer();

        int playerNumber = NetworkManager.Singleton.ConnectedClients.Count + 1;

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player" + playerNumber));

        InitializeCameraPlayer(playerNumber);

        // Optionnel : Modifier le nom et la couleur du joueur
        // gameObject.name = GameData.playerInfos.Name;
        // player.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;

        players.Add(gameObject);

        // Debug.Log("Is host : " + NetworkManager.Singleton.IsHost + " Is client : " + NetworkManager.Singleton.IsClient);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
