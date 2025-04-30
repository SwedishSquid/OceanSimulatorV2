//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ComplexObjectConstructor
{
    private readonly float colorCoherence;
    private readonly float scaleMultiplier;

    private readonly int minComponentCount;
    private readonly int maxComponentCount;

    private readonly Color baseColor;

    public ComplexObjectConstructor(float colorCoherence, float scaleMultiplier = 0.33f, 
        int minComponentCount = 5, int maxComponentCount=10, Color? baseColor = null)
    {
        this.colorCoherence = colorCoherence;
        this.scaleMultiplier = scaleMultiplier;
        this.minComponentCount = minComponentCount;
        this.maxComponentCount = maxComponentCount;

        if (baseColor != null)
        {
            this.baseColor = (Color)baseColor;
        }
        else
        {
            this.baseColor = Random.ColorHSV();
        }
    }

    public GameObject ConstructComplexRandomObject()
    {
        GameObject complexObject = new GameObject("ComplexObject");
        //complexObject.tag = "Object";

        PrimitiveType[] primitiveTypes = {
            PrimitiveType.Cube,
            PrimitiveType.Sphere,
            PrimitiveType.Capsule,
            PrimitiveType.Cylinder,
        };

        int childCount = Random.Range(minComponentCount, maxComponentCount);
        Vector3 sumPositions = Vector3.zero;
        List<Transform> children = new List<Transform>();

        // First pass: create all children and record their positions
        for (int i = 0; i < childCount; i++)
        {
            PrimitiveType randomType = primitiveTypes[Random.Range(0, primitiveTypes.Length)];
            GameObject child = GameObject.CreatePrimitive(randomType);
            child.tag = "Object";

            child.transform.parent = complexObject.transform;
            Vector3 randomPos = Random.insideUnitSphere * 2f;
            child.transform.localPosition = randomPos;
            sumPositions += randomPos;

            child.transform.localScale = new Vector3(
                Random.Range(0.5f, 2f),
                Random.Range(0.5f, 2f),
                Random.Range(0.5f, 2f));
            child.transform.localRotation = Random.rotationUniform;

            Renderer r = child.GetComponent<Renderer>();
            Color randomColor = Random.ColorHSV();
            r.material.color = Color.Lerp(baseColor, randomColor, 1f - colorCoherence);

            children.Add(child.transform);
        }

        // Calculate center offset
        Vector3 centerOffset = sumPositions / childCount;

        // Second pass: adjust positions to center the object
        foreach (Transform child in children)
        {
            child.localPosition -= centerOffset;
        }

        complexObject.transform.localScale = Vector3.one * scaleMultiplier;

        return complexObject;
    }
}
