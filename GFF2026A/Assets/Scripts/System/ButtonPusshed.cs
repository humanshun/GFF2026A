using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPusshed : MonoBehaviour
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
