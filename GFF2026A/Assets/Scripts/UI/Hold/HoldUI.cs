using UnityEngine;
using UnityEngine.UI;

public class HoldUI : MonoBehaviour
{
    [SerializeField] private HoldSlot holdSlot;
    [SerializeField] private Image icon;

    private void Update()
    {
        if (!holdSlot || !icon) return;

        if (holdSlot.TryGetStored(out var go) && go)
        {
            Sprite s = null;

            if (go.TryGetComponent(out AnimalScoreProvider sp) && sp.Data)
            {
                s = sp.Data.Icon;
            }

            icon.enabled = (s != null);
            icon.sprite = s;
            if (s != null) icon.color = Color.white;
        }
        else
        {
            icon.enabled = false;
        }
    }
}
