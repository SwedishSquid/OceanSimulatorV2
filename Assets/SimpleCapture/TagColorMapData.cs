using System.Collections.Generic;
using UnityEngine;

public class TagColorMapData : MonoBehaviour
{
    [System.Serializable]
    public class TagColorData
    {
        public string tag;
        public Color color;
    }

    public List<TagColorData> tagColoring;

    public Color fallbackColor = Color.magenta;

    public static TagColorMapData instance = null;

    public void Start()
    {
        if (instance != null)
        {
            throw new System.InvalidOperationException("attempt to create several TagColorMapData in one scene - it is a singleton!");
        }
        instance = this;
    }

    public static Color GetColor(string tag)
    {
        foreach (var tagData in instance.tagColoring)
        {
            if (tagData.tag == tag)
            {
                return tagData.color;
            }
        }
        Debug.LogWarning($"color for tag [{tag}] not found");
        return instance.fallbackColor;
    }
}
