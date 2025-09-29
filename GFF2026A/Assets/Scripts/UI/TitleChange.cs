using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleChange : MonoBehaviour
{
  

    public void SwitchScene()
    {
        SceneManager.LoadScene("InGame", LoadSceneMode.Single);
    }
}
