using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.CompilerServices;
public class TitleFadein : MonoBehaviour
{
    [SerializeField] private Image logoImage;   // �^�C�g�����S
    [SerializeField] private float fadeDuration = 0.5f; // �t�F�[�h����(�b)

    void Start()
    {
        // �N�����Ƀt�F�[�h�J�n
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = logoImage.color;

        // ������Ԃ͓���
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