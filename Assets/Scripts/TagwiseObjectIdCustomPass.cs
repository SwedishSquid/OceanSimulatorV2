using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

public class TagwiseObjectIdCustomPass : ObjectIDCustomPass
{
    public override void AssignObjectIDs()
    {
        var rendererList = Resources.FindObjectsOfTypeAll(typeof(Renderer));

        foreach (Renderer renderer in rendererList)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            //Debug.Log($"coloring [{renderer.gameObject}]");
            var color = TagColorMapData.GetColor(renderer.gameObject.tag);
            propertyBlock.SetColor("ObjectColor", color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
