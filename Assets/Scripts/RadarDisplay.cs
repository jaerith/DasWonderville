using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct RadarEntry
{
    public string tag;
    public Color color;
    [Range(4f, 24f)]
    public float blipSize;
}

public class RadarDisplay : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float worldRadius = 200f;
    [SerializeField] private RadarEntry[] trackedEntries;

    public RectTransform blipContainer;
    public float blipContainerRadius = 195f;

    private readonly Dictionary<GameObject, Image> blips = new();
    private readonly HashSet<GameObject> seen = new();

    private void Update()
    {
        if (player == null || blipContainer == null)
            return;

        seen.Clear();

        foreach (RadarEntry entry in trackedEntries)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(entry.tag))
            {
                seen.Add(obj);

                if (!blips.TryGetValue(obj, out Image blip) || blip == null)
                    blip = CreateBlip(obj, entry);

                Vector3 delta = obj.transform.position - player.position;
                delta.y = 0f;
                bool inRange = delta.magnitude <= worldRadius;

                blip.gameObject.SetActive(inRange);
                if (inRange)
                {
                    blip.color = entry.color;
                    blip.rectTransform.anchoredPosition = WorldToRadar(delta);
                }
            }
        }

        List<GameObject> toRemove = null;
        foreach (var kvp in blips)
        {
            if (kvp.Key == null || !seen.Contains(kvp.Key))
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
                (toRemove ??= new List<GameObject>()).Add(kvp.Key);
            }
        }
        if (toRemove != null)
            foreach (var key in toRemove) blips.Remove(key);
    }

    private Image CreateBlip(GameObject target, RadarEntry entry)
    {
        var go = new GameObject(target.name + "_blip");
        go.transform.SetParent(blipContainer, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(entry.blipSize, entry.blipSize);

        var img = go.AddComponent<Image>();
        img.raycastTarget = false;

        blips[target] = img;
        return img;
    }

    // Projects a world-space xz delta onto the radar panel (heading-up: player forward = UI up).
    private Vector2 WorldToRadar(Vector3 delta)
    {
        float yaw = player.eulerAngles.y * Mathf.Deg2Rad;
        float cos = Mathf.Cos(yaw);
        float sin = Mathf.Sin(yaw);

        float scale = blipContainerRadius / worldRadius;
        return new Vector2(
            (delta.x * cos - delta.z * sin) * scale,
            (delta.x * sin + delta.z * cos) * scale
        );
    }
}
