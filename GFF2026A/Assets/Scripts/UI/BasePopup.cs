using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public abstract class BasePopup : MonoBehaviour, IPopup
{
    [Header("Animation")]
    [SerializeField] protected float inDuration  = 0.18f;
    [SerializeField] protected float outDuration = 0.14f;
    [SerializeField] protected bool  useScaleAnimation = true;
    [SerializeField] protected bool  closeOnBackKey = true;

    protected RectTransform rt;
    protected CanvasGroup cg;
    public bool IsOpen { get; private set; }

    protected virtual void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        // 初期は非表示状態にしておく（Instantiate直後でも安全）
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        if (useScaleAnimation) rt.localScale = Vector3.zero;
    }

    void Update()
    {
        if (!IsOpen || !closeOnBackKey) return;
#if UNITY_ANDROID || UNITY_WEBGL || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
#endif
    }

    public void Open()
    {
        StopAllCoroutines();
        StartCoroutine(CoOpen());
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(CoClose());
    }

    IEnumerator CoOpen()
    {
        OnBeforeOpen();

        IsOpen = true;
        cg.blocksRaycasts = true;
        cg.interactable   = true;

        float t = 0;
        while (t < inDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / inDuration);
            cg.alpha = k;
            if (useScaleAnimation) rt.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, EaseOutBack(k));
            yield return null;
        }
        cg.alpha = 1f;
        if (useScaleAnimation) rt.localScale = Vector3.one;

        OnAfterOpen();
    }

    IEnumerator CoClose()
    {
        OnBeforeClose();

        float t = 0;
        while (t < outDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(t / outDuration);
            cg.alpha = k;
            if (useScaleAnimation) rt.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, EaseIn(k));
            yield return null;
        }
        cg.alpha = 0f;
        if (useScaleAnimation) rt.localScale = Vector3.zero;

        cg.blocksRaycasts = false;
        cg.interactable   = false;
        IsOpen = false;

        OnAfterClose();
        // 基本は閉じたら破棄（必要なら SetActive(false) 運用に変更OK）
        Destroy(transform.parent != null && transform.parent.name == "Dim"
            ? transform.parent.gameObject : gameObject);
    }

    // --- Hooks（派生で必要な初期化/後片付けを差し込む）---
    protected virtual void OnBeforeOpen()  {}
    protected virtual void OnAfterOpen()   {}
    protected virtual void OnBeforeClose() {}
    protected virtual void OnAfterClose()  {}

    // --- 簡易イージング ---
    protected static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f; const float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
    protected static float EaseIn(float x) => x * x;
}
