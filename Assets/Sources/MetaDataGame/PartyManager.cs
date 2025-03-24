using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Référence à la prefab
    [SerializeField] private GameObject botPrefab; // Référence à la prefab
    [SerializeField] private GameObject terrain;
    private void Start() {
        // Prepare the terrain
        terrain.SetActive(true);
        terrain.GetComponent<TerrainGenerator>().GenerateTerrain();
        SpawnPlayers();
        // SpawnBots();
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

            // Optionnel : Modifier le nom et la couleur du joueur
            newPlayer.name = GameData.playerList[i].Name;
            newPlayer.GetComponent<Renderer>().material.color = GameData.playerList[i].Color;
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
