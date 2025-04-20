using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FloatingWizard
{
    private WaterSurface targetSurface = null;

    // Internal search params
    private WaterSearchParameters searchParameters = new WaterSearchParameters();
    private WaterSearchResult searchResult = new WaterSearchResult();

    private float error;
    private int maxIterations;

    public FloatingWizard(WaterSurface targetSurface, float error = 0.001f, int maxIterations = 32)
    {
        this.targetSurface = targetSurface;
        if (this.targetSurface == null)
        {
            throw new ArgumentException($"water surface not set for FloatingWizard!");
        }

        this.error = error;
        this.maxIterations = maxIterations;
    }

    /// <summary>
    /// time shall be stopped for about 1 second or more for this to work properly (ensure this before call)
    /// </summary>
    /// <param name="obj"></param>
    public void AdjustToSurface(GameObject obj)
    {
        searchParameters.startPositionWS = obj.transform.position;
        //searchParameters.startPositionWS = searchResult.candidateLocationWS;
        searchParameters.targetPositionWS = obj.transform.position;
        searchParameters.error = 0.001f;
        searchParameters.maxIterations = 32;
        searchParameters.outputNormal = true;

        // Do the search
        if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
        {
            //Debug.Log(searchResult.projectedPositionWS);
            obj.transform.position = searchResult.projectedPositionWS;
            
            // worldUp равен forward для того, чтобы объект дополнительно не выравнивался по оси X или Y
            obj.transform.LookAt(searchResult.projectedPositionWS - searchResult.normalWS, Vector3.forward);
        }
        else Debug.LogError("Can't Find Projected Position");
    }

}
