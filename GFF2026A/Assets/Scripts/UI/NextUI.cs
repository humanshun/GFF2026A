using UnityEngine;
using UnityEngine.UI;

public class NextUI : MonoBehaviour
{
    [SerializeField] private AnimalSpawner spawner;
    [SerializeField] private Image[] slots;

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void TrySubscribe()
    {
        if (spawner && spawner.Decider is NextDecider nd)
            nd.QueueChanged += Refresh;
    }

    private void TryUnsubscribe()
    {
        if (spawner && spawner.Decider is NextDecider nd)
            nd.QueueChanged -= Refresh;
    }

    private void Refresh()
    {
        if (!(spawner && spawner.Decider is NextDecider nd)) return;
        var arr = nd.NextAnimalsList; // ← IReadOnlyList なのでインデックスOK

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < arr.Count && arr[i] != null && arr[i].Icon != null)
            {
                slots[i].sprite = arr[i].Icon;
                slots[i].enabled = true;
                slots[i].color = Color.white;
            }
            else
            {
                slots[i].enabled = false;
            }
        }
    }
}
