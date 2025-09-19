using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Button  button=GetComponent<Button>();
        if(button != null )
        {
            button.onClick.AddListener(ChangeScene);
        }
        else
        {
            Debug.LogError("Button compornent not found on this GameObject");
        }
    }

    
    void ChangeScene()
    {
        SceneManager.LoadScene("InGame");
    }
}
