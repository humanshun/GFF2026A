using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtomPsshed : MonoBehaviour
{
    public void Replay()
    {
        SceneManager.LoadScene("InGame");
    }
    public void GoTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
