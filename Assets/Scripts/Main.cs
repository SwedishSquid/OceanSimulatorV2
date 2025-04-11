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
    public WaterSurface targetSurface;

    public GameObject cameraCase;
    public ShipMover shipMover;

    public GameObject horizonDetectionPlane;

    public TotalCapturer capturer;

    private FloatingWizard floatingWizard;
    private GameObject[] objectPrefabs;

    private float toSurfaceAdjustmentPeriod = 1f;  // how much to wait before adjusting to surface in seconds; Improves accuracy
    private float intervalBetweenCapture = 3f;     //seconds
    private string storeFolder = "dev_gen";

    private void Awake()
    {
        if (shipMover == null || cameraCase == null || targetSurface == null || capturer == null)
        {
            throw new ArgumentException("not all dependencies set");
        }
        floatingWizard = new FloatingWizard(targetSurface);
        objectPrefabs = Resources.LoadAll<GameObject>("ObjectPrefabs");
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
        var shipDirection = new Vector3(-1, 0, -1).normalized;
        var shipSpeed = 3f;
        var objectToBoardDistance = 10f;
        var hopDistance = shipSpeed * intervalBetweenCapture;
        var objectHorizontalShift = objectToBoardDistance + 2 * hopDistance;    // todo: add random value to diversify object position
        shipMover.SetParamsAndReset(shipDirection, shipSpeed,
            rollOscillator: Oscillator.MakeEmpty().AddBand(2, 11, 0.25f),
            pitchOscillator: Oscillator.MakeEmpty().AddBand(2, 15, 0),
            heaveOscillator: Oscillator.MakeEmpty().AddBand(0.25f, 9, 0)
            );

        var shipVelocityNomal = shipMover.gameObject.transform.forward;
        shipVelocityNomal.y = 0;
        shipVelocityNomal.Normalize();

        var objSpawnLocation = cameraCase.transform.position + shipVelocityNomal * objectToBoardDistance
            + shipDirection * (objectHorizontalShift);
        // todo: sample object and its rotation
        var obj = Instantiate(objectPrefabs[0], objSpawnLocation, Quaternion.identity);

        yield return new WaitForSeconds(1f);    // warmup, maybe reduncant

        var nSteps = (int)Math.Ceiling(objectHorizontalShift * 2 / (shipSpeed * intervalBetweenCapture)) + 2;
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
            shipMover.AdjustPosition(i * intervalBetweenCapture);
            AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);
            Time.timeScale = timeScale;
            Debug.Log($"step {i} / {nSteps}");
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

    private void AdjustHorizonPlanePosition(Vector3 cameraPosition)
    {
        if (horizonDetectionPlane == null)
        {
            Debug.LogWarning("horizon detection plane not set");
            return;
        }
        horizonDetectionPlane.transform.position = new Vector3(cameraPosition.x, 
            horizonDetectionPlane.transform.position.y, cameraPosition.z);
    }
}
