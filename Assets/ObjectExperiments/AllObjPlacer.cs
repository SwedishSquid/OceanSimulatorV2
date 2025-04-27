using UnityEngine;
using System.Collections.Generic;

public class AllObjPlacer : MonoBehaviour
{

    private List<GameObject> objectPrefabs;

    void Start()
    {
        objectPrefabs = new PrefabFetcher().FetchAllPrefabs();

        MoveForward();
        foreach (var prefab in objectPrefabs)
        {
            var obj = Instantiate(prefab);
            obj.transform.position = transform.position;
            //Debug.Log(prefab.name);
            MoveForward();
        }
    }

    private void MoveForward()
    {
        transform.position += transform.forward;
    }
}
