using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;

public class Main : MonoBehaviour
{
    public WaterSurface targetSurface;

    public Volume globalVolume;

    public Light sunLight;

    public GameObject cameraCase;
    public ShipMover shipMover;

    public GameObject horizonDetectionPlane;

    public TotalCapturer capturer;

    private FloatingWizard floatingWizard;
    private GameObject[] objectPrefabs;

    private float toSurfaceAdjustmentPeriod = 1f;  // how much to wait before adjusting to surface in seconds; Improves accuracy
    private float intervalBetweenCapture = 3f;     //seconds
    
    private string storeFolder = "dev_gen";
    private bool throwIfEpisodeExists = true;

    private int paddingShots = 2;

    private MetaConfig metaConfig;

    private void Awake()
    {
        if (shipMover == null || cameraCase == null || targetSurface == null || capturer == null || globalVolume == null)
        {
            throw new ArgumentException("not all dependencies set");
        }
        floatingWizard = new FloatingWizard(targetSurface);
        objectPrefabs = Resources.LoadAll<GameObject>("ObjectPrefabs");

        metaConfig = new MetaConfig();
    }

    void Start()
    {
        metaConfig.SaveMetaConfig(storeFolder);
        StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        var iteration = 0;
        var nEpisodes = 5;
        while (iteration < nEpisodes)
        {
            Debug.Log($"starting episode {iteration}");
            yield return StartEpisode(iteration);
            iteration++;
        }
        Debug.Log("end of simulation");
    }

    private string InitializeEpisodeDirectory(EpisodeConfig config, int episodeIndex)
    {
        var episodeName = $"episode_{episodeIndex}";
        var episodeDir = Path.Combine(storeFolder, episodeName);
        if (Directory.Exists(episodeDir) && throwIfEpisodeExists)
        {
            throw new InvalidOperationException($"{episodeName} already exists - stopping to avoid overriding it");
        }
        Directory.CreateDirectory(episodeDir);
        var configJson = JsonUtility.ToJson(config, prettyPrint: true);
        File.WriteAllText(Path.Combine(episodeDir, "episode_config.json"), configJson);
        return episodeDir;
    }

    private IEnumerator StartEpisode(int episodeIndex)
    {
        var episodeConfig = metaConfig.Sample();
        var episodeDir = InitializeEpisodeDirectory(episodeConfig, episodeIndex);

        ConfigureEnvironment(episodeConfig);

        var shipDirection = episodeConfig.ShipDirection;
        var shipSpeed = episodeConfig.ShipSpeed;
        var objectToBoardDistance = episodeConfig.ObjectToBoatDistance;
        var hopDistance = shipSpeed * intervalBetweenCapture;

        var objectHorizontalShift = objectToBoardDistance + paddingShots * hopDistance 
            + episodeConfig.ObjectDisplacement * hopDistance;
        shipMover.SetParamsAndReset(shipDirection, shipSpeed,
            rollOscillator: new Oscillator(episodeConfig.ShipRollBands),
            pitchOscillator: new Oscillator(episodeConfig.ShipPitchBands),
            heaveOscillator: new Oscillator(episodeConfig.ShipHeaveBands)
            );
        AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);

        var shipVelocityNomal = shipMover.gameObject.transform.forward;
        shipVelocityNomal.y = 0;
        shipVelocityNomal.Normalize();

        var objSpawnLocation = cameraCase.transform.position + shipVelocityNomal * objectToBoardDistance
            + shipDirection * (objectHorizontalShift);

        // todo: sample object
        var obj = Instantiate(objectPrefabs[0], objSpawnLocation, Quaternion.identity);
        obj.transform.localScale = episodeConfig.ObjectScale;

        capturer.AssignObjectIDs();

        yield return new WaitForSeconds(1f);    // warmup, maybe redundant

        var nSteps = (int)Math.Ceiling(objectHorizontalShift * 2 / (shipSpeed * intervalBetweenCapture)) + 1;
        for (int i = 0; i < nSteps; i++)
        {
            Debug.Log($"starting step {i} / {nSteps}");
            yield return new WaitForSeconds(intervalBetweenCapture);
            yield return new WaitForEndOfFrame();
            var timeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(toSurfaceAdjustmentPeriod);
            floatingWizard.AdjustToSurface(obj);
            Debug.Log("surface adjustment complete");
            yield return new WaitForSecondsRealtime(0.8f);
            yield return new WaitForEndOfFrame();
            Debug.Log("before capturing");

            CaptureAllCameras(i, episodeDir);
            
            Debug.Log("results saved");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();   // just to be sure
            shipMover.AdjustPosition((i + 1) * intervalBetweenCapture);
            AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);
            Time.timeScale = timeScale;
        }

        Destroy(obj);   // who needs this junk
    }

    private void CaptureAllCameras(int frameCounter, string episodeDir)
    {
        foreach (var cs in capturer.cameraSettings)
        {
            string fileName = $"{frameCounter}.png";
            var cameraFolderName = cs.camera.name;
            string filePath = Path.Combine(episodeDir, cameraFolderName, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
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

    private void ConfigureEnvironment(EpisodeConfig config)
    {
        // WATER
        targetSurface.transform.rotation = Quaternion.Euler(new Vector3(0, config.WaterRotation, 0));
        targetSurface.largeWindSpeed = config.WaterDistantWindSpeed;
        targetSurface.largeChaos = config.WaterChaos;
        targetSurface.largeOrientationValue = config.WaterCurrentOrientation;
        targetSurface.largeBand0Multiplier = config.WaterFirstBandAmplitude;
        targetSurface.largeBand1Multiplier = config.WaterSecondBandAmplitude;
        targetSurface.ripplesWindSpeed = config.WaterRipplesWindSpeed;
        targetSurface.ripplesChaos = config.WaterRipplesChaos;
        targetSurface.refractionColor = config.WaterColor;
        targetSurface.scatteringColor = config.WaterColor;
        targetSurface.absorptionDistance = config.WaterAbsorptionDistance;
        targetSurface.foamPersistenceMultiplier = config.FoamPersistenceMultipier;
        targetSurface.foamColor = config.FoamColor;
        targetSurface.foamSmoothness = config.FoamSmoothness;
        targetSurface.simulationFoamAmount = config.FoamAmount;

        // LIGHT
        sunLight.transform.rotation = Quaternion.Euler(config.LightRotation);
        if (!sunLight.gameObject.TryGetComponent<HDAdditionalLightData>(out var lightData))
        {
            throw new ArgumentException("no HDAdditionalLightData found");
        }
        lightData.angularDiameter = config.LightAngularDiameter;

        // SKY
        if (!globalVolume.profile.TryGet<PhysicallyBasedSky>(out var physicallyBasedSky))
        {
            throw new ArgumentException("physically based sky volume component not found on global volume");
        }
        physicallyBasedSky.groundTint.value = config.SkyGroundTint;
        
        // CLOUDS
        if (!globalVolume.profile.TryGet<VolumetricClouds>(out var volumetricClouds))
        {
            throw new ArgumentException("volumetric clouds volume component not found");
        }
        volumetricClouds.enable.value = true;
        volumetricClouds.shapeOffset.value = config.CloudsShapeOffset;
        switch (config.CloudsType)
        {
            case "Disabled":
                volumetricClouds.enable.value = false;
                break;
            case "Sparse":
                volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
                break;
            case "Cloudy":
                volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Cloudy;
                break;
            case "Stormy":
                volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Stormy;
                break;
            case "Overcast":
                volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Overcast;
                break;
            default:
                throw new ArgumentException($"unknown cloud type {config.CloudsType}");
        }
    }
}
