using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;
using NUnit.Framework;

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

    private bool oneFrameEpisodeMode = true;

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
        targetSurface.gameObject.SetActive(true);
        metaConfig.SaveMetaConfig(storeFolder);
        StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        var iteration = 0;
        var nEpisodes = 1024;
        while (iteration < nEpisodes)
        {
            Debug.Log($"starting episode {iteration}");
            if (oneFrameEpisodeMode)
            {
                yield return StartOneFrameEpisode(iteration);
            }
            else
            {
                throw new NotImplementedException();
                //yield return StartEpisode(iteration);
            }
            iteration++;
        }
        Debug.Log("end of simulation");
        UnityEditor.EditorApplication.isPlaying = false;
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

    private string InitializeOneFrameEpisode(EpisodeConfig config, int episodeIndex)
    {
        var oneFrameDir = Path.Combine(storeFolder, "one_frame_episodes");
        Directory.CreateDirectory(oneFrameDir);
        var configJson = JsonUtility.ToJson(config, prettyPrint: true);
        var configPath = Path.Combine(oneFrameDir, "configs", $"{episodeIndex}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        if (File.Exists(configPath) && throwIfEpisodeExists)
        {
            throw new InvalidOperationException($"one frame episode [{episodeIndex}] already exists!");
        }
        File.Create(configPath).Dispose();
        File.WriteAllText(configPath, configJson);
        return oneFrameDir;
    }

    private GameObject SampleObject()
    {
        //var colorDistribution = new DiscreteDistribution<Color?>
        //    (new List<Color?>() { Color.white, null },
        //    new List<double> { 0.4d, 0.6d }
        //    );
        //return new ComplexObjectConstructor(UnityEngine.Random.Range(0.3f, 0.7f),
        //    minComponentCount:4,
        //    baseColor:colorDistribution.Sample())
        //    .ConstructComplexRandomObject();

        var obj = Instantiate(objectPrefabs[UnityEngine.Random.Range(0, objectPrefabs.Length)]);
        return obj;
    }

    private void ConfigureShipMover(EpisodeConfig episodeConfig)
    {
        var shipDirection = episodeConfig.ShipDirection;
        var shipSpeed = episodeConfig.ShipSpeed;
        shipMover.SetParamsAndReset(shipDirection, shipSpeed,
            rollOscillator: new Oscillator(episodeConfig.ShipRollBands),
            pitchOscillator: new Oscillator(episodeConfig.ShipPitchBands),
            heaveOscillator: new Oscillator(episodeConfig.ShipHeaveBands)
            );
        AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);
    }

    private IEnumerator StartOneFrameEpisode(int episodeIndex)
    {
        var episodeConfig = metaConfig.Sample();
        var directory = InitializeOneFrameEpisode(episodeConfig, episodeIndex);

        ConfigureEnvironment(episodeConfig);

        ConfigureShipMover(episodeConfig);

        var objectToBoardDistance = episodeConfig.ObjectToBoatDistance;
        var hopDistance = shipMover.Speed * intervalBetweenCapture;

        var objectHorizontalShift = objectToBoardDistance + paddingShots * hopDistance
            + episodeConfig.ObjectDisplacement * hopDistance;

        var shipVelocityNomal = shipMover.gameObject.transform.forward;
        shipVelocityNomal.y = 0;
        shipVelocityNomal.Normalize();

        var objSpawnLocation = cameraCase.transform.position + shipVelocityNomal * objectToBoardDistance
            + shipMover.MovementDirection * (objectHorizontalShift);

        // todo: sample object
        //var obj = Instantiate(objectPrefabs[0], objSpawnLocation, Quaternion.identity);
        var obj = SampleObject();
        obj.transform.position = objSpawnLocation;
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, episodeConfig.ObjectScale);

        capturer.AssignObjectIDs();

        yield return new WaitForSeconds(1f);    // warmup, maybe redundant

        var nSteps = (int)Math.Ceiling(objectHorizontalShift * 2 / (shipMover.Speed * intervalBetweenCapture)) + 1;
        var stepIndex = UnityEngine.Random.Range(0 + 3, nSteps - 1 - 3);

        Debug.Log($"chosen step is [{stepIndex} / {nSteps}]");

        var timeScale = Time.timeScale;
        Time.timeScale = 0;

        shipMover.AdjustPosition(stepIndex * intervalBetweenCapture);
        AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);

        yield return new WaitForSecondsRealtime(toSurfaceAdjustmentPeriod);
        floatingWizard.AdjustToSurface(obj);
        Debug.Log("surface adjustment complete");
        yield return new WaitForSecondsRealtime(0.8f);
        yield return new WaitForEndOfFrame();

        CaptureAllCameras(episodeIndex, directory);

        var stepLog = new EpisodeStepLogRecord()
        {
            objectPosition = obj.transform.position,
            cameraPosition = cameraCase.transform.position,
            cameraRotation = cameraCase.transform.rotation,
            shipPosition = shipMover.transform.position,
        };
        SaveEpisodeStepLog(stepLog, episodeIndex, directory);
        Debug.Log("results saved");

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();   // just to be sure
        Time.timeScale = timeScale;

        Destroy(obj);   // who needs this junk
    }

    //private IEnumerator StartEpisode(int episodeIndex)
    //{
    //    var episodeConfig = metaConfig.Sample();
    //    var episodeDir = InitializeEpisodeDirectory(episodeConfig, episodeIndex);

    //    ConfigureEnvironment(episodeConfig);

    //    var shipDirection = episodeConfig.ShipDirection;
    //    var shipSpeed = episodeConfig.ShipSpeed;
    //    var objectToBoardDistance = episodeConfig.ObjectToBoatDistance;
    //    var hopDistance = shipSpeed * intervalBetweenCapture;

    //    var objectHorizontalShift = objectToBoardDistance + paddingShots * hopDistance 
    //        + episodeConfig.ObjectDisplacement * hopDistance;
    //    shipMover.SetParamsAndReset(shipDirection, shipSpeed,
    //        rollOscillator: new Oscillator(episodeConfig.ShipRollBands),
    //        pitchOscillator: new Oscillator(episodeConfig.ShipPitchBands),
    //        heaveOscillator: new Oscillator(episodeConfig.ShipHeaveBands)
    //        );
    //    AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);

    //    var shipVelocityNomal = shipMover.gameObject.transform.forward;
    //    shipVelocityNomal.y = 0;
    //    shipVelocityNomal.Normalize();

    //    var objSpawnLocation = cameraCase.transform.position + shipVelocityNomal * objectToBoardDistance
    //        + shipDirection * (objectHorizontalShift);

    //    // todo: sample object
    //    //var obj = Instantiate(objectPrefabs[0], objSpawnLocation, Quaternion.identity);
    //    //obj.transform.localScale = episodeConfig.ObjectScale;
    //    var obj = SampleObject();
    //    obj.transform.position = objSpawnLocation;
    //    obj.transform.localScale = Vector3.Scale(obj.transform.localScale, episodeConfig.ObjectScale);

    //    capturer.AssignObjectIDs();

    //    yield return new WaitForSeconds(1f);    // warmup, maybe redundant

    //    var nSteps = (int)Math.Ceiling(objectHorizontalShift * 2 / (shipSpeed * intervalBetweenCapture)) + 1;
    //    for (int i = 0; i < nSteps; i++)
    //    {
    //        Debug.Log($"starting step {i} / {nSteps}");
    //        yield return new WaitForSeconds(intervalBetweenCapture);
    //        yield return new WaitForEndOfFrame();
    //        var timeScale = Time.timeScale;
    //        Time.timeScale = 0;
    //        yield return new WaitForSecondsRealtime(toSurfaceAdjustmentPeriod);
    //        floatingWizard.AdjustToSurface(obj);
    //        Debug.Log("surface adjustment complete");
    //        yield return new WaitForSecondsRealtime(0.8f);
    //        yield return new WaitForEndOfFrame();

    //        CaptureAllCameras(i, episodeDir);

    //        var stepLog = new EpisodeStepLogRecord()
    //        {
    //            objectPosition = obj.transform.position,
    //            cameraPosition = cameraCase.transform.position,
    //            cameraRotation = cameraCase.transform.rotation,
    //            shipPosition = shipMover.transform.position,
    //        };
    //        SaveEpisodeStepLog(stepLog, i, episodeDir);

    //        Debug.Log("results saved");
    //        yield return new WaitForEndOfFrame();
    //        yield return new WaitForEndOfFrame();   // just to be sure
    //        shipMover.AdjustPosition((i + 1) * intervalBetweenCapture);
    //        AdjustHorizonPlanePosition(cameraCase.gameObject.transform.position);
    //        Time.timeScale = timeScale;
    //    }

    //    Destroy(obj);   // who needs this junk
    //    SaveEpisodeFinishedFlag(episodeDir);
    //}

    private void SaveEpisodeStepLog(EpisodeStepLogRecord record, int frameCounter, string episodeDir)
    {
        var filepath = Path.Combine(episodeDir, "StepLogs", $"{frameCounter}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
        File.Create(filepath).Dispose();
        var content = JsonUtility.ToJson(record, prettyPrint: true);
        File.WriteAllText(filepath, content);
    }

    private void SaveEpisodeFinishedFlag(string episodeDir)
    {
        var filepath = Path.Combine(episodeDir, "finished_flag.txt");
        File.Create(filepath).Dispose();
    }

    private void CaptureAllCameras(int frameCounter, string episodeDir)
    {
        Debug.Log("start capturing");
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
