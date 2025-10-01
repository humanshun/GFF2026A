using UnityEngine;
using UnityEngine.InputSystem;

public class AudioController : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AudioManager.Instance.ToggleBGM();
        }
    }
}
