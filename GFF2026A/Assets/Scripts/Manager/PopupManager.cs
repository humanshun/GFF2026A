using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [System.Serializable]
    public class PopupEntry
    {
        public string Id;
        public GameObject PopupObj;
    }

    [SerializeField] private List<PopupEntry> popups;

    public void Open(string id)
    {
        var entry = popups.Find(p => p.Id == id);
        if (entry != null) entry.PopupObj.SetActive(true);
    }

    public void Close(string id)
    {
        var entry = popups.Find(p => p.Id == id);
        if (entry != null) entry.PopupObj.SetActive(false);
    }

    public void Toggle(string id)
    {
        var entry = popups.Find(p => p.Id == id);
        if (entry != null) entry.PopupObj.SetActive(!entry.PopupObj.activeSelf);
    }
}
