using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {private set; get;}

    private void Awake() {
        if(instance != null){
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void ChangeScene (string _sceneName) {
        Debug.Log(_sceneName);
        SceneManager.LoadScene(_sceneName);
    }

    public void Quit () {
        Application.Quit();
    }
}
