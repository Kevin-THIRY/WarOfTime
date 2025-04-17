using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {private set; get;}
    [SerializeField] private GameObject turnManager;

    [Header("Debug")]
    [SerializeField] private bool fakeHost = false;
    // public GameObject networkHandlerPrefab;
    // private GameObject handlerInstance;

    private void Awake() {
        if(instance != null){
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (fakeHost) StartHost();
        // if (NetworkManager.Singleton.IsServer)
        // {
        //     handlerInstance = Instantiate(networkHandlerPrefab);
        //     handlerInstance.GetComponent<NetworkObject>().Spawn();
        // }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();

        if (NetworkManager.Singleton.IsServer)
        {
            turnManager = Instantiate(turnManager);
            turnManager.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void StartServeur()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient(string address)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = address;
        NetworkManager.Singleton.StartClient();
    }

    public void ChangeScene (string _sceneName) {
        Debug.Log(_sceneName);
        SceneManager.LoadScene(_sceneName);
    }

    public void Quit () {
        Application.Quit();
    }
}
