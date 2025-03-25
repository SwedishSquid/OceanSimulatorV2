using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

public class ExpCapturer : MonoBehaviour
{
    [System.Serializable]
    public class CameraSettings
    {
        public Camera camera;
        public bool hasObjIdPass = false;
    }

    public int width = 5184;
    public int height = 3888;

    public float interval = 3f;

    public string storeFolder = "dev_gen";

    public CameraSettings[] cameraSettings;

    void Start()
    {
        if (!Directory.Exists(storeFolder))
        {
            Directory.CreateDirectory(storeFolder);
        }

        //    foreach (var cameraSettings in cameraSettings)
        //{
        //    if (cameraSettings.hasObjIdPass)
        //    {
        //        var customPassVolume = cameraSettings.camera.gameObject.GetComponent<CustomPassVolume>();
        //        Debug.Log(customPassVolume);
        //        var objIdPass = (ObjectIDCustomPass) customPassVolume.customPasses[0];
        //        Debug.Log(objIdPass);
        //        //objIdPass.
        //        objIdPass.AssignObjectIDs();
        //    }
        //}

        StartCoroutine(CapturingProcess());
    }

    private IEnumerator CapturingProcess()
    {
        int counter = 0;
        while (true)
        {
            yield return new WaitForSeconds(interval);
            var timeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForEndOfFrame();
            foreach (var cameraSettings in cameraSettings)
            {
                CapturePhoto(cameraSettings, counter);
            }
            yield return new WaitForEndOfFrame();
            Time.timeScale = timeScale;
            counter++;
        }
    }

    private void CapturePhoto(CameraSettings cameraSettings, int iter)
    {
        var camera = cameraSettings.camera;
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        var prevTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = prevTarget;

        var old_rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = old_rt;

        byte[] bytes = texture.EncodeToPNG();
        string fileName = $"{camera.name}_{iter}.png";
        string filePath = Path.Combine(storeFolder, fileName);
        File.WriteAllBytes(filePath, bytes);

        Destroy(renderTexture);
        Destroy(texture);

        Debug.Log($"Captured photo from {camera.name}: {filePath}");
    }
}
