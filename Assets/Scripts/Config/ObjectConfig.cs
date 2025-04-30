using System;
using UnityEngine;


public enum ObjectType
{
    Prefab = 0,
    ComplexObject = 1,
}

[Serializable]
public class ObjectConfig
{
    public float ToShipboardDistance;  // length of perpendicular to the motion vector
    
    public Vector3 Scale;

    /// <summary>
    /// [0 - 1]
    /// </summary>
    public float Displacement;

    public ObjectType ObjectType;
}

