using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ConditionalDistribution<T> : IDistribution<T>
{
    private readonly IDistribution<T> baseDistribution;
    private readonly Func<T, bool> suitabilityPredicate;

    public ConditionalDistribution(IDistribution<T> baseDistribution, Func<T, bool> suitabilityPredicate)
    {
        this.baseDistribution = baseDistribution;
        this.suitabilityPredicate = suitabilityPredicate;
    }

    public T Sample()
    {
        var value = baseDistribution.Sample();
        while (!suitabilityPredicate(value))
        {
            value = baseDistribution.Sample();
        }
        return value;
    }
}
