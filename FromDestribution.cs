using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Randomness
{
    public class FromDestribution : Attribute
    {
        public Type DistributionType { get; set; }
        public double[] Properties;

        public FromDestribution(Type distType, params double[] args)
        {
            DistributionType = distType;
            Properties = args;
        }
    }
}
