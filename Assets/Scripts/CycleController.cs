using System.Collections;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class CycleController : MonoBehaviour
{
    private TotalCapturer capturer;

    public float interval = 3f;

    public string storeFolder = "dev_gen";

    void Start()
    {
        capturer = GetComponent<TotalCapturer>();
        //capturer.AssignObjectIDs();
        StartCoroutine(CaptureCycle());
    }

    private IEnumerator CaptureCycle()
    {
        int counter = 0;
        while (true)
        {
            yield return new WaitForSeconds(interval);
            var timeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForEndOfFrame();
            SavePhotos(capturer.CaptureAll(), counter);
            yield return new WaitForEndOfFrame();
            Time.timeScale = timeScale;
            counter++;
            Debug.Log($"pictures taken {counter}");
        }
    }

    private void SavePhotos(List<TotalCapturer.PhotoData> photos, int counter)
    {
        foreach (var photo in photos)
        {
            string fileName = $"{photo.cs.camera.name}_{counter}.png";
            string filePath = Path.Combine(storeFolder, fileName);
            File.WriteAllBytes(filePath, photo.contentPNG);
        }
    }
}
