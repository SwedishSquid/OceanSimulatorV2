using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

public class MixtureDistribution<T> : IDistribution<T>
{
    private DiscreteDistribution<IDistribution<T>> componentDistribution;

    public MixtureDistribution(List<IDistribution<T>> components, List<double> probabilities)
    {
        componentDistribution = new DiscreteDistribution<IDistribution<T>>(components, probabilities);
    }

    public T Sample()
    {
        var component = componentDistribution.Sample();
        return component.Sample();
    }
}

