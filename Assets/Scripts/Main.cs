using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Main : MonoBehaviour
{
    //private string savePath = "";

    public WaterSurface targetSurface;

    public GameObject cameraCase;
    private Vector3 defaultCameraCasePosition;
    private Quaternion defaultCameraCaseRotation;

    public TotalCapturer capturer;

    private FloatingWizard floatingWizard;
    private GameObject[] objectPrefabs;

    private float toSurfaceAdjustmentPeriod = 1f;  // how much to wait before adjusting to surface in seconds; Improves accuracy
    private float intervalBetweenCapture = 3f;     //seconds
    private string storeFolder = "dev_gen";

    private void Awake()
    {
        if (cameraCase == null || targetSurface == null || capturer == null)
        {
            throw new ArgumentException("not all dependencies set");
        }
        floatingWizard = new FloatingWizard(targetSurface);
        objectPrefabs = Resources.LoadAll<GameObject>("ObjectPrefabs");
        defaultCameraCasePosition = cameraCase.transform.position;
        defaultCameraCaseRotation = cameraCase.transform.rotation;
    }

    void Start()
    {
        // set up scene
        StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        var iteration = 0;
        while (true)
        {
            Debug.Log($"starting iteration {iteration}");
            yield return StartEncounter();
            break; // todo: make different save directories
        }
        Debug.Log("end of simulation");
    }
    
    private IEnumerator StartEncounter()
    {
        var shipDirection = new Vector3(-1, 0, 0);
        var shipSpeed = 3f;
        var objectToBoardDistance = 10f;
        var reserveDistance = 5f;
        cameraCase.transform.position = defaultCameraCasePosition;
        cameraCase.transform.rotation = defaultCameraCaseRotation;
        // todo: handle camera rotation

        var shipVelocityNomal = cameraCase.transform.forward;
        shipVelocityNomal.y = 0;
        shipVelocityNomal.Normalize();

        var objSpawnLocation = cameraCase.transform.position + shipVelocityNomal * objectToBoardDistance
            + shipDirection * (objectToBoardDistance + reserveDistance);
        // todo: sample object and its rotation
        var obj = Instantiate(objectPrefabs[0], objSpawnLocation, Quaternion.identity);

        yield return new WaitForSeconds(1f);    // warmup, maybe reduncant

        var nSteps = (int)Math.Ceiling((objectToBoardDistance + reserveDistance) * 2 / shipSpeed);
        for (int i = 0; i < nSteps; i++)
        {
            yield return new WaitForSeconds(intervalBetweenCapture);
            yield return new WaitForEndOfFrame();
            var timeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(toSurfaceAdjustmentPeriod);
            floatingWizard.AdjustToSurface(obj);
            Debug.Log("surface adjustment complete");
            yield return new WaitForSecondsRealtime(0.3f);
            yield return new WaitForEndOfFrame();
            Debug.Log("before capturing");

            CaptureAllCameras(i);
            
            Debug.Log("results saved");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();   // just to be sure
            cameraCase.transform.position += shipDirection * shipSpeed;
            Time.timeScale = timeScale;
        }
    }

    private void CaptureAllCameras(int counter)
    {
        foreach (var cs in capturer.cameraSettings)
        {
            string fileName = $"{cs.camera.name}_{counter}.png";
            string filePath = Path.Combine(storeFolder, fileName);
            capturer.CaptureAndSave(cs, filePath);
        }
    }

    //private void SavePhotos(List<TotalCapturer.PhotoData> photos, int counter)
    //{
    //    foreach (var photo in photos)
    //    {
    //        string fileName = $"{photo.cs.camera.name}_{counter}.png";
    //        string filePath = Path.Combine(storeFolder, fileName);
    //        File.WriteAllBytes(filePath, photo.contentPNG);
    //    }
    //}
}
