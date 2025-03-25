using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

class RandomObjectIDsCustomPass : ObjectIDCustomPass
{
    public override void AssignObjectIDs()
    {
        var rendererList = Resources.FindObjectsOfTypeAll(typeof(Renderer));
        System.Random rand = new System.Random();

        int index = 0;
        foreach (Renderer renderer in rendererList)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            float hue = (float)rand.NextDouble();
            propertyBlock.SetColor("ObjectColor", Color.HSVToRGB(hue, 0.7f, 1.0f));
            renderer.SetPropertyBlock(propertyBlock);
            index++;
        }
    }
}

