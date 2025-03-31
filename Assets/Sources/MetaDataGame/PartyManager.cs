using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Référence à la prefab
    [SerializeField] private GameObject botPrefab; // Référence à la prefab
    [SerializeField] private GameObject terrain;
    private void Start() {
        PrepareTerrain();
        SpawnPlayers();
        // SpawnBots();
    }

    private void PrepareTerrain()
    {
        terrain.SetActive(true);
        terrain.GetComponent<TerrainGenerator>().GenerateTerrain();
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
            player.GetComponentInChildren<FogLayerManager>().SetLayer(64);
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
            
            player.GetComponentInChildren<PlayerManager>().SetTerrainGenerator(terrain.GetComponent<TerrainGenerator>());
            player.GetComponentInChildren<Camera>().targetDisplay = i;
            AddHighlightMapToPlayer(player);
            AddFogOfWarToPlayer(player);

            // Optionnel : Modifier le nom et la couleur du joueur
            player.name = GameData.playerList[i].Name;
            // player.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;
        }
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

    private void Update()
    {
        // Manage turn rotation

    }
}
