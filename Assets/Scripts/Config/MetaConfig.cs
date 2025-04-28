using System.IO;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.STP;
using UnityEngine.UIElements;
using System.Linq;


public class MetaConfig
{
    public EpisodeConfig Sample()
    {
        var config = new EpisodeConfig();

        ConfigureShip(config);
        ConfigureWater(config);
        ConfigureLight(config);
        ConfigureSky(config);
        ConfigureClouds(config);

        ConfigureObjects(config);
        
        // todo: sample object path

        return config;
    }

    public void SaveMetaConfig(string dstFolder)
    {
        var srcPath = Path.Combine(Application.dataPath, "Scripts/Config/MetaConfig.cs");
        var dstFile = Path.Combine(dstFolder, "MetaConfigCheckpoint.cs");
        var metaConfigContents = File.ReadAllText(srcPath);
        File.Create(dstFile).Dispose();
        File.WriteAllText(dstFile, metaConfigContents);
        Debug.Log("MetaConfig saved");
    }

    private ObjectConfig SampleObjectConfig()
    {
        var config = new ObjectConfig();

        var testMultiplier = 1f; //10f;
        var minDist = 7f;
        var maxDist = 75f;
        var farSize = 0.5f;
        var closeSize = 0.05f;
        config.ToShipboardDistance = new HyperbolicDistribution(minDist, maxDist).Sample();
        
        //Debug.Log($"distance to board = {config.ObjectToBoatDistance}");
        var t = (config.ToShipboardDistance - minDist) / (maxDist - minDist);
        var expectedLinearSize = Mathf.LerpUnclamped(closeSize, farSize, t);
        var sizeMultiplierDistribution = new ConditionalDistribution<float>(
            new MixtureDistribution<float>(new List<IDistribution<float>> {
                    new NormalDistribution(bias: 1, 0.3f),
                    new NormalDistribution(bias:1.8f, std:0.5f)
                },
                new List<double> { 10d / 13d, 3d / 13d }
            ),
            value => value >= 0.3f);
        var sizeMultiplier = sizeMultiplierDistribution.Sample();
        config.Scale = testMultiplier * sizeMultiplier * expectedLinearSize * Vector3.one;

        config.Displacement = Random.Range(0f, 1f);

        return config;
    }

    private void ConfigureObjects(EpisodeConfig config)
    {
        var objCount = Random.Range(5, 12);

        config.ObjectConfigs = Enumerable.Range(0, objCount)
            .Select(v => SampleObjectConfig())
            .ToArray();
    }

    private void ConfigureClouds(EpisodeConfig config)
    {
        config.CloudsType = SampleCloudsType();
        config.CloudsShapeOffset = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    private void ConfigureLight(EpisodeConfig config)
    {
        config.LightRotation = new Vector3(Random.Range(7f, 173f), Random.Range(-180f, 180f), 0);
        config.LightAngularDiameter = Random.Range(0.3f, 4.5f);
    }

    private void ConfigureSky(EpisodeConfig config)
    {
        config.SkyGroundTint = config.WaterColor;
    }

    private void ConfigureWater(EpisodeConfig config)
    {
        // todo: sample ripples wind with respect to large wind; the same goes to foam

        config.WaterRotation = Random.Range(-180f, 180f);
        config.WaterDistantWindSpeed = Random.Range(0f, 42f);
        config.WaterChaos = Random.Range(0.1f, 1f);
        config.WaterCurrentOrientation = Random.Range(-180f, 180f);
        config.WaterFirstBandAmplitude = 0.5f; // for the swells not to be too big
        config.WaterSecondBandAmplitude = Mathf.Min(Random.Range(0.3f, 1.2f), 1f);
        config.WaterRipplesWindSpeed = Random.Range(0f, 15f);
        config.WaterRipplesChaos = Random.Range(0.1f, 1f);
        config.WaterColor = SampleWaterColor();
        config.WaterAbsorptionDistance = Random.Range(1f, 3f);

        // foam

        config.FoamPersistenceMultipier = Random.Range(0.1f, 1f);
        config.FoamColor = Color.white;
        config.FoamSmoothness = Random.Range(0f, 1f);
        if (Random.Range(0f, 1f) < 0.1f)
        {
            config.FoamAmount = 0f;
        }
        else
        {
            config.FoamAmount = Random.Range(0f, 0.9f);
        }
    }

    private void ConfigureShip(EpisodeConfig config)
    {
        var direction = Random.onUnitSphere;
        direction.y = 0;
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("ship direction of 0 magnitude, changed to default");
            direction = Vector3.forward;
        }
        direction.Normalize();
        config.ShipDirection = direction;

        config.ShipSpeed = Random.Range(2.5f, 3.5f);    //todo: find real values

        var rollAmplitudeDistribution = new ConditionalDistribution<float>(new NormalDistribution(2, 0.5f), v => v >= 0);
        var pitchAmplitudeDistribution = new ConditionalDistribution<float>(new NormalDistribution(1f, 0.3f), v => v >= 0);
        config.ShipRollBands = Oscillator.MakeEmpty().AddBand(rollAmplitudeDistribution.Sample(), 
            Random.Range(9f, 20f), 
            Random.Range(0f, 1f), 
            Random.Range(-1f, 1f)).bands;
        config.ShipPitchBands = Oscillator.MakeEmpty().AddBand(pitchAmplitudeDistribution.Sample(), 
            Random.Range(10f, 17f),
            Random.Range(0f, 1f), Random.Range(-1f, 1f)).bands;
        config.ShipHeaveBands = Oscillator.MakeEmpty().AddBand(Random.Range(0f, 0.1f), 
            Random.Range(7f, 16f),
            Random.Range(0f, 1f), 
            Random.Range(-0.2f, 0.2f)).bands;
    }

    private Color SampleWaterColor()
    {
        return Random.ColorHSV(170f / 360f, 230f / 360f, 0.5f, 1f, 0.5f, 0.85f);
    }

    private string SampleCloudsType()
    {
        var randomValue = Random.Range(0f, 1f);
        if (randomValue < 0.3f)
        {
            return "Disabled";
        }
        if (randomValue < 0.6f)
        {
            return "Sparse";
        }
        if (randomValue < 0.9f)
        {
            return "Cloudy";
        }
        if (randomValue < 0.96f)
        {
            return "Stormy";
        }
        return "Overcast";
    }
}
