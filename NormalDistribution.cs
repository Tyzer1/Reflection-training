using System;

namespace Reflection.Randomness
{
    public class NormalDistribution : IContinuousDistribution
    {
        public readonly double Sigma; //{ get; set; }
        public readonly double Mean; //{ get; set; }

        public NormalDistribution()
            : this(0.0, 1.0)
        {
        }

        public NormalDistribution(double sigma, double mean)
        {
            Sigma = sigma;
            Mean = mean;
        }

        public double Generate(Random rnd)
        {
            var u = rnd.NextDouble();
            var v = rnd.NextDouble();
            var x = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
            double res = x * Sigma + Mean;
            return res;
        }
    }
}
