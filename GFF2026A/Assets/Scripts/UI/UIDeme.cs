using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.Windows;
using UnityEngine.UI;
using UnityEditor.U2D.Sprites;
using Unity.VisualScripting;

public class UIDeme : MonoBehaviour
{
    public TextMeshProUGUI output;
    public TMP_InputField userName;
   
    public int max = 12;
    public int mini = 2;

    private string allowedPattern = @"^[ぁ-んァ-ン\p{IsCJKUnifiedIdeographs} a-zA-Z0-9]+$";

    

    public void ButtonDemo()
    {
        string input = userName.text;
        input = input.Trim();                      // 前後のスペース削除
        input = input.Replace(" ", "");            // 半角スペース削除
        input = input.Replace("　", "");           // 全角スペース削除

        if (Regex.IsMatch(input,allowedPattern))
        {
            output.text = input;
        }
        else
        {
            output.text = "名前は半角英数字と日本語で2～12文字以内にしてください";

        }

    }
    [SerializeField]public TMP_Text Input;

    public void GameStart()
    {
        SceneManager.LoadScene("InGame");
    }

    public void namelimit()
    {
        string name = userName.text;
        int namecount = name.Length;

        //if (mini <= namecount&&namecount <= max)
        //{
        //    Debug.Log(namecount +  "文字：名前の長さが有効です。");
        //    SceneManager.LoadScene("InGame");
        //}
        //else
        //{
        //    output.text = $"名前は{mini}～{max}文字以内にしてください";
        //    Debug.Log("無効な名前の長さ：" + namecount);
        //}
        switch (namecount)
        {
            case int n when(n >= 2 && n <= 12):
                Debug.Log(namecount + "文字：名前の長さが有効です。");
                Input.text = "ログイン中...";
                Input.color = Color.white;
                SceneManager.LoadScene("InGame");
            break;

            case int n when (n > 12):
                Debug.Log(namecount + "文字：名前の長さが１２文字以上のため無効です。");
                Input.text = "文字数が超えています";
                Input.color = Color.red;
            break;

            case int n when (n < 2):
                Debug.Log(namecount + "文字：名前の長さが２文字以下のため無効です。");
                Input.text = "文字数が足りません";
                Input.color = Color.red;
            break;

            default:
                Debug.Log(namecount + "文字：名前の長さが無効です。");
            break;
        }
        
    }

    

}
