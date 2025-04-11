using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.Rendering;


public class Oscillator
{
    public struct Band
    {
        public float amplitude;
        /// <summary>
        /// in time units
        /// </summary>
        public float period;
        /// <summary>
        /// from 0 to 1
        /// </summary>
        public float phase;
    }

    public List<Band> bands;

    public Oscillator(List<Band> bands)
    {
        this.bands = bands;
    }

    public float Sample(float time)
    {
        var value = 0f;
        var pi2 = Mathf.PI * 2;
        foreach (var band in bands)
        {
            value += Mathf.Sin(pi2 * (time / band.period + band.phase)) * band.amplitude;
        }
        return value;
    }

    public static Oscillator MakeEmpty()
    {
        return new Oscillator(new List<Band>());
    }

    public Oscillator AddBand(float amplitude, float period, float phase)
    {
        bands.Add(new Band() { amplitude=amplitude,  period=period, phase=phase });
        return this;
    }
}
