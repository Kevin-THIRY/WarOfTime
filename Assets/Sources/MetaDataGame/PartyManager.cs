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
        terrain.transform.Find("FogOfWar(Clone)").gameObject.SetActive(false);
    }

    private void SpawnPlayers()
    {
        if (GameData.playerList == null || GameData.playerList.Count == 0)
        {
            Debug.LogWarning("Aucun joueur à instancier !");
            return;
        }

        for (int i = 0; i < GameData.playerList.Count; i++)
        {
            Vector3 spawnPosition = new Vector3(i * 2, 0, 0); // Change la position selon ton besoin
            GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            newPlayer.GetComponentInChildren<PlayerManager>().SetTerrainGenerator(terrain.GetComponent<TerrainGenerator>());
            newPlayer.GetComponentInChildren<PlayerManager>().SetMouseShaderController(terrain.GetComponentInChildren<MouseShaderController>());
            terrain.GetComponent<TerrainGenerator>().GenerateFogPlayer(newPlayer.transform.Find("FogOfWar/Fog").gameObject);
            newPlayer.GetComponentInChildren<FogLayerManager>().SetLayer(64);
            newPlayer.GetComponentInChildren<FogLayerManager>().SetFogPlayer(newPlayer.transform.Find("FogOfWar/Fog/Fog of War").gameObject);

            // Optionnel : Modifier le nom et la couleur du joueur
            newPlayer.name = GameData.playerList[i].Name;
            // newPlayer.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;
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
            GameObject newbot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

            // Optionnel : Modifier le nom et la couleur du joueur
            newbot.name = GameData.playerList[i].Name;
            newbot.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;
        }
    }

    private void Update()
    {
        // Manage turn rotation

    }
}
