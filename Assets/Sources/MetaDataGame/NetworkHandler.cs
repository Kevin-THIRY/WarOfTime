using Unity.Netcode;
using UnityEngine;

public class NetworkHandler : NetworkBehaviour
{
    public static NetworkHandler instance {private set; get;}
    [SerializeField] private GameObject playerPrefab; // Référence à la prefab

    private void Awake() {
        if(instance != null){
            Destroy(this);
            return; 
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("HelloHandler");
        // Ici on spawn un joueur pour ce client spécifique
        SpawnClientServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnClientServerRpc(ulong clientId)
    {
        // On spawn le joueur pour ce client et on l'associe à ce client
        var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}