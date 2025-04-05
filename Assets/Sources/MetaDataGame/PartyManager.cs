using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class PartyManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Référence à la prefab
    [SerializeField] private GameObject botPrefab; // Référence à la prefab
    [SerializeField] private GameObject terrain;

    [Header("Debug")]
    [SerializeField] private bool launch1Player = false;
    [Range(1, 4)] [SerializeField] private int nbPlayer = 1;

    private List<GameObject> players = new List<GameObject>();
    private void Start() {
        // Only to debug
        if (launch1Player)
        {
            for (int i = 0; i < nbPlayer; i++)
            {
                GameData.playerList.Add(new PlayerOption { Name = "Debug Player" + i, Color = Color.blue });
            }
        }
        PrepareTerrain();
        SpawnPlayers();
        // SpawnBots();
    }

    private void PrepareTerrain()
    {
        terrain.SetActive(true);
        terrain.GetComponent<TerrainGenerator>().GenerateTerrain();
        ElementaryBasics.terrainGenerator = terrain.GetComponent<TerrainGenerator>();
        if (terrain.transform.Find("FogOfWar(Clone)") != null) terrain.transform.Find("FogOfWar(Clone)").gameObject.SetActive(false);
        if (terrain.transform.Find("Highlight Map(Clone)") != null) terrain.transform.Find("Highlight Map(Clone)").gameObject.SetActive(false);
    }

    private void SpawnPlayers()
    {
        void AddHighlightMapToPlayer(GameObject player)
        {
            terrain.GetComponent<TerrainGenerator>().GenerateHighlightPlayer(player.transform.Find("Highlight Map/Highlight").gameObject);
            player.GetComponentInChildren<PlayerManager>().SetMouseShaderController(player.transform.Find("Highlight Map/Highlight").gameObject.GetComponent<MouseShaderController>());
        }

        void AddFogOfWarToPlayer(GameObject player)
        {
            terrain.GetComponent<TerrainGenerator>().GenerateFogPlayer(player.transform.Find("FogOfWar/Fog").gameObject);
            // player.GetComponentInChildren<FogLayerManager>().SetLayer(64);
            player.GetComponentInChildren<FogLayerManager>().SetFogPlayer(player.transform.Find("FogOfWar/Fog/Fog of War").gameObject);
        }

        if (GameData.playerList == null || GameData.playerList.Count == 0)
        {
            Debug.LogWarning("Aucun joueur à instancier !");
            return;
        }

        for (int i = 0; i < GameData.playerList.Count; i++)
        {
            Vector3 spawnPosition = new Vector3(i * 2, 0, 0); // Change la position selon ton besoin
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            int playerNumber = i + 1;
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            playerInput.SwitchCurrentControlScheme("BasicsInput", Keyboard.current);
            playerInput.SwitchCurrentActionMap("Player" + playerNumber);
            player.GetComponentInChildren<MovementManager>().SetInputSystem(playerInput.actions);
            
            player.GetComponentInChildren<PlayerManager>().SetTerrainGenerator(terrain.GetComponent<TerrainGenerator>());
            player.GetComponentInChildren<Camera>().targetDisplay = i;
            AddHighlightMapToPlayer(player);
            AddFogOfWarToPlayer(player);

            
            SetLayerRecursively(player, LayerMask.NameToLayer("Player" + playerNumber));

            Camera cam = player.transform
                                .Find("CameraBase/MainCamera")
                                .GetComponent<Camera>();
            
            cam.cullingMask = LayerMask.GetMask("Default") | LayerMask.GetMask("TransparentFX") | LayerMask.GetMask("Ignore Raycast")
                                & LayerMask.GetMask("Water") | LayerMask.GetMask("UI") | LayerMask.GetMask("Player" + playerNumber);

            // Optionnel : Modifier le nom et la couleur du joueur
            player.name = GameData.playerList[i].Name;
            // player.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;

            players.Add(player);
        }

        // Only for splitscreen but useless here
        // List<Camera> cameras = new List<Camera>();

        // foreach (GameObject player in players) // players = ta liste de joueurs
        // {
        //     Camera cam = player.transform
        //                         .Find("CameraBase/MainCamera")
        //                         .GetComponent<Camera>();
            
        //     cam.cullingMask = LayerMask.GetMask("Joueur1");
        // }

        // SetupViewports(cameras);
    }

    private void SpawnBots()
    {
        if (GameData.botList == null || GameData.botList.Count == 0)
        {
            Debug.LogWarning("Aucun bot à instancier !");
            return;
        }

        for (int i = 0; i < GameData.botList.Count; i++)
        {
            Vector3 spawnPosition = new Vector3(i * 2, 0, 0); // Change la position selon ton besoin
            GameObject bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

            // Optionnel : Modifier le nom et la couleur du joueur
            bot.name = GameData.playerList[i].Name;
            bot.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // SplitScreen
    private void SetupViewports(List<Camera> cameras)
    {
        int count = cameras.Count;

        for (int i = 0; i < cameras.Count; i++)
        {
            Camera cam = cameras[i];

            switch (count)
            {
                case 1:
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
                    break;

                case 2:
                    cam.rect = new Rect(i == 0 ? 0f : 0.5f, 0f, 0.5f, 1f);
                    break;

                case 3:
                    if (i == 0)
                        cam.rect = new Rect(0.25f, 0.5f, 0.5f, 0.5f); // Haut, centré
                    else if (i == 1)
                        cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); // Bas gauche
                    else
                        cam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); // Bas droite
                    break;

                case 4:
                    cam.rect = new Rect(
                        (i % 2) * 0.5f,
                        (i < 2 ? 0.5f : 0f),
                        0.5f,
                        0.5f
                    );
                    break;

                default:
                    Debug.LogWarning("Nombre de joueurs non géré");
                    break;
            }
        }
    }

    private void Update()
    {
        // Manage turn rotation

    }
}
