using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// such a distribution, that pds(x) * x == const for fixed parameters
/// </summary>
public class HyperbolicDistribution : IDistribution
{
    public float min;
    public float max;

    public HyperbolicDistribution(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Sample()
    {
        var c = 1 / (Mathf.Log(max) - Mathf.Log(min));
        var r = Random.Range(0f, 1f);
        return Mathf.Exp((r / c) + Mathf.Log(min));
    }
}
