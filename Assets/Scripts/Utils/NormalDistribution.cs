using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class NormalDistribution : IDistribution
{
    public float bias;

    public float std;

    public NormalDistribution(float bias = 0f, float std = 1f) {
        this.bias = bias;
        this.std = std;
    }

    public float Sample()
    {
        var r = Random.Range(0f, 1f);
        var phi = Random.Range(0f, 1f);
        var z = Mathf.Cos(2 * Mathf.PI * phi) * Mathf.Sqrt(-2 * Mathf.Log(r));
        return bias + z * std;
    }
}

