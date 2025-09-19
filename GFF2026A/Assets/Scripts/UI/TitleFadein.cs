using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.CompilerServices;
public class TitleFadein : MonoBehaviour
{
    [SerializeField] private Image logoImage;   // タイトルロゴ
    [SerializeField] private float fadeDuration = 0.5f; // フェード時間(秒)

    void Start()
    {
        // 起動時にフェード開始
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = logoImage.color;

        // 初期状態は透明
        color.a = 0f;
        logoImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            color.a = alpha;
            logoImage.color = color;

            yield return null;
        }
    }
}