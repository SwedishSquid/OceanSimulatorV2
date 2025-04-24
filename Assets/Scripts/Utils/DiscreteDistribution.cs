using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DiscreteDistribution<T> : IDistribution<T>
{
    private List<T> values;
    private List<double> probabilities;

    public DiscreteDistribution(List<T> values, List<double> probabilities)
    {
        if (values.Count != probabilities.Count)
        {
            throw new ArgumentException("values and probabilities of different shape");
        }
        var error = Math.Abs(probabilities.Sum() - 1);
        if (error > 1e-7)
        {
            throw new ArgumentException("probs do not sum to 1");
        }
        this.values = values;
        this.probabilities = probabilities;
    }

    public T Sample()
    {
        var threshold = UnityEngine.Random.Range(0f, 1f);
        var probSum = 0d;
        for (var i = 0; i < probabilities.Count; i++)
        {
            probSum += probabilities[i];
            if (probSum >= threshold)
            {
                return values[i];
            }
        }
        Debug.LogWarning($"discrete distribution failed to match threshold [{threshold}]; returning last value");
        return values.Last();
    }
}

