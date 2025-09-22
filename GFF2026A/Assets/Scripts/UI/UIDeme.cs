
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.Windows;

public class UIDeme : MonoBehaviour
{
    public TextMeshProUGUI output;
    public TMP_InputField userName;

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
}
