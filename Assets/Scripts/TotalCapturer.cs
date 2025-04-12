using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using static UnityEditor.Progress;
using System.IO;
using System.Linq;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using Unity.Collections.LowLevel.Unsafe;

public class TotalCapturer : MonoBehaviour
{
    [System.Serializable]
    public class CameraSettings
    {
        public Camera camera;
    }

    [System.Serializable]
    public class Resolution
    {
        public int width;
        public int height;
    }

    public bool UseMainResolution = true;
    public Resolution MainResolution = new Resolution() { width=3888, height=2916 };
    public Resolution DevResolution;

    public CameraSettings[] cameraSettings;

    private RenderTexture renderTexture;
    private Texture2D texture;

    private int width;
    private int height;

    public void AssignObjectIDs()
    {
        foreach (var cs in cameraSettings)
        {
            if (cs.camera.gameObject.TryGetComponent<CustomPassVolume>(out var customPass))
            {
                foreach (var pass in customPass.customPasses)
                {
                    if (pass is ObjectIDCustomPass objIdPass)
                    {
                        objIdPass.AssignObjectIDs();
                    }
                }
            }
        }
    }

    public class PhotoData
    {
        public byte[] contentPNG;
        public CameraSettings cs;
    }

    public void Start()
    {
        if (UseMainResolution)
        {
            width = MainResolution.width;
            height = MainResolution.height;
        }
        else
        {
            width = DevResolution.width;
            height = DevResolution.height;
        }
        renderTexture = new RenderTexture(width, height, 24);
        texture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public List<PhotoData> CaptureAll()
    {
        return cameraSettings.Select(cs => CaptureOne(cs))
            .ToList();
    }

    /// <summary>
    /// call this when game is paused
    /// </summary>
    /// <param name="cs"></param>
    /// <returns></returns>
    public PhotoData CaptureOne(CameraSettings cs)
    {
        var camera = cs.camera;
        var prevTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = prevTarget;

        var old_rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = old_rt;

        // these two might not be needed, but they as well might help a bit
        renderTexture.Release();
        System.GC.Collect();

        byte[] bytes = texture.EncodeToPNG();

        return new PhotoData { contentPNG = bytes, cs = cs };
    }

    public void CaptureAndSave(CameraSettings cs, string filepath)
    {
        var photo = CaptureOne(cs);
        File.WriteAllBytes(filepath, photo.contentPNG);
    }

    private void OnDestroy()
    {
        Destroy(renderTexture);
        Destroy(texture);
    }
}
