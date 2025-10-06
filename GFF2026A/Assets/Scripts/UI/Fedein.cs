
using UnityEngine;
using TMPro;

public class Fedein : MonoBehaviour
{
    public float duration = 1.0f; // フェードインにかかる時間
    private TextMeshProUGUI textMesh; 
    private float timer;
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = 0;
            textMesh.color = color;
        }
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;

        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = Mathf.Lerp(0, 1, progress); // 0から1へ変化させる
            textMesh.color = color;
        }
    }
}
