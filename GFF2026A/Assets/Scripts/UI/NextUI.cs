using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextUI : MonoBehaviour
{
    [SerializeField] private AnimalSpawner spawner;
    [SerializeField] private Image[] slots;

    private void Update()
    {
        if (spawner.Decider is NextDecider nd)
        {
            var arr = new List<AnimalData>(nd.NextAnimals);
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < arr.Count && arr[i] != null)
                {
                    slots[i].sprite = arr[i].Icon;
                    slots[i].enabled = true;
                }
                else
                {
                    slots[i].enabled = false;
                }
            }
        }
    }
}
