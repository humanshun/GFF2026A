using System.Collections.Generic;
using UnityEngine;

public static class TopHeightUtility
{
    public static float ComputeTopY(IReadOnlyList<GameObject> list, float fallback = 0f)
    {
        float top = fallback;
        if (list == null) return top;

        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (!go) continue;

            // Collider2D優先
            var col = go.GetComponentInChildren<Collider2D>();
            if (col) { top = Mathf.Max(top, col.bounds.max.y); continue; }

            // なければRenderer
            var ren = go.GetComponentInChildren<Renderer>();
            if (ren) { top = Mathf.Max(top, ren.bounds.max.y); }
        }
        return top;
    }
}
