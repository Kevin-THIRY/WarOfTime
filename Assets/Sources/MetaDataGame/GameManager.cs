using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {private set; get;}

    [Header("Debug")]
    [SerializeField] private bool fakeHost = false;

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
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
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
