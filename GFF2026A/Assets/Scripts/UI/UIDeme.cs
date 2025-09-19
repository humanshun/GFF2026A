
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.Windows;

public class UIDeme : MonoBehaviour
{
    public TextMeshProUGUI output;
    public TMP_InputField userName;

    private string allowedPattern = @"^[ぁ-んァ-ン一-龥a-zA-Z0-9]+$";
    public void ButtonDemo()
    {
        string Input = userName.text;
        Input = Input.Trim();
        Input = Input.Trim();                      // 前後のスペース削除
        Input = Input.Replace(" ", "");            // 半角スペース削除
        Input = Input.Replace("　", "");           // 全角スペース削除

        if (Regex.IsMatch(Input,allowedPattern))
        {
            output.text = Input;
        }
        else
        {
            output.text = "名前は半角英数字と日本語で2～12文字以内にしてください";

        }
    }
}
