using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    // PopupManagerのシングルトン
    public static PopupManager Instance { get; private set; }

    // Awakeでインスタンスを設定
    private void Awake()
    {
        // 自分自身をInstanceに登録
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [SerializeField] private Canvas canvas;

    // IDでポップアップを開く
    public void Open(GameObject popupPrefab)
    {
        Instantiate(popupPrefab, canvas.transform);
    }
}
