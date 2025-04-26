using System.Collections.Generic;
using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
    void Start()
    {
        var obj = new ComplexObjectConstructor(0.8f, minComponentCount:3).ConstructComplexRandomObject();
        obj.transform.position = transform.position;
    }
}
