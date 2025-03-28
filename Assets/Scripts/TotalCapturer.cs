using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using static UnityEditor.Progress;
using System.IO;
using System.Linq;

public class TotalCapturer : MonoBehaviour
{
    [System.Serializable]
    public class CameraSettings
    {
        public Camera camera;
        public int width = 5184;
        public int height = 3888;
    }

    public CameraSettings[] cameraSettings;

    public void AssignObjectIDs()
    {
        foreach (var cs in cameraSettings)
        {
            if (cs.camera.gameObject.TryGetComponent<ObjectIDCustomPass>(out var customPass))
            {
                customPass.AssignObjectIDs();
            }
        }
    }

    public class PhotoData
    {
        public byte[] contentPNG;
        public CameraSettings cs;
    }

    public List<PhotoData> CaptureAll()
    {
        return cameraSettings.Select(cs => CaptureOne(cs))
            .ToList();
    }

    public PhotoData CaptureOne(CameraSettings cs)
    {
        var camera = cs.camera;
        RenderTexture renderTexture = new RenderTexture(cs.width, cs.height, 24);
        var prevTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = prevTarget;

        var old_rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(cs.width, cs.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, cs.width, cs.height), 0, 0);
        texture.Apply();
        RenderTexture.active = old_rt;

        byte[] bytes = texture.EncodeToPNG();

        Destroy(renderTexture);
        Destroy(texture);
        return new PhotoData { contentPNG = bytes, cs = cs };
    }
}
