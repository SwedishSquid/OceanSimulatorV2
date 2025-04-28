using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EpisodeConfig
{
    // SHIP
    public Vector3 ShipDirection;
    public float ShipSpeed;

    public List<Oscillator.Band> ShipRollBands;
    public List<Oscillator.Band> ShipPitchBands;
    public List<Oscillator.Band> ShipHeaveBands;

    // WATER SURFACE
    /// <summary>
    /// in angles
    /// </summary>
    public float WaterRotation;
    /// <summary>
    /// 0 - 250
    /// </summary>
    public float WaterDistantWindSpeed;
    /// <summary>
    /// [0 - 1]
    /// </summary>
    public float WaterChaos;

    /// <summary>
    /// controls waves direction somehow;
    /// angles
    /// </summary>
    public float WaterCurrentOrientation;

    /// <summary>
    /// [0 - 1]
    /// </summary>
    public float WaterFirstBandAmplitude;
    /// <summary>
    /// [0 - 1]
    /// </summary>
    public float WaterSecondBandAmplitude;
    /// <summary>
    /// [0 - 15]
    /// </summary>
    public float WaterRipplesWindSpeed;
    /// <summary>
    /// [0 - 1]
    /// </summary>
    public float WaterRipplesChaos;

    public Color WaterColor;
    /// <summary>
    /// controls transparency of water
    /// </summary>
    public float WaterAbsorptionDistance;

    /// <summary>
    /// [0 - 1]
    /// </summary>
    public float FoamPersistenceMultipier;
    
    public Color FoamColor; // can be used to simulate seaweed \ algae

    /// <summary>
    /// what does it do though?
    /// </summary>
    public float FoamSmoothness;

    /// <summary>
    /// this one is important
    /// </summary>
    public float FoamAmount;


    // LIGHT
    /// <summary>
    /// x for up-down; y for left-right; angles;
    /// </summary>
    public Vector3 LightRotation;
    /// <summary>
    /// affects sharpness of shadows + shape of flare
    /// </summary>
    public float LightAngularDiameter;

    // SKY
    /// <summary>
    /// set to something watery
    /// </summary>
    public Color SkyGroundTint;


    // CLOUDS
    public string CloudsType;
    /// <summary>
    /// all values in range [0 - 1]
    /// </summary>
    public Vector3 CloudsShapeOffset;


    public ObjectConfig[] ObjectConfigs;


    //// STRANGE
    //public int WaterRepetitionSize = 2000;
    //public float WaterCurrentSpeed = 0;
    ///// <summary>
    ///// [0 - 1]
    ///// </summary>
    //public float FoamCurrentInfluence = 0.6f;

    ///// <summary>
    ///// affects performance
    ///// </summary>
    //public float FoamTextureTiling = 0.15f;
    ///// <summary>
    ///// [0 - 1]
    ///// </summary>
    //public float CloudsTemporalAccumulation = 0f;
}
