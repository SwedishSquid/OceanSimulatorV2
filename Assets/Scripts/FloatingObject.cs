using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System;
using System.Collections;
using System.Linq;


public class FloatingObject : MonoBehaviour
{
    public WaterSurface targetSurface = null;

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    private void Start()
    {
        StartCoroutine(TimeStoppingUpdate());
    }

    private IEnumerator TimeStoppingUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            var timeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(1f);
            AdjustToSurface();
            yield return new WaitForSecondsRealtime(0.3f);
            Time.timeScale = timeScale;
        }
    }

    void AdjustToSurface()
    {
        if (targetSurface == null)
        {
            throw new ArgumentException($"water surface not set in {gameObject}");
        }
        searchParameters.startPositionWS = gameObject.transform.position;
        //searchParameters.startPositionWS = searchResult.candidateLocationWS;
        searchParameters.targetPositionWS = gameObject.transform.position;
        searchParameters.error = 0.001f;
        searchParameters.maxIterations = 32;

        // Do the search
        if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
        {
            Debug.Log(searchResult.projectedPositionWS);
            gameObject.transform.position = searchResult.projectedPositionWS;
        }
        else Debug.LogError("Can't Find Projected Position");
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
