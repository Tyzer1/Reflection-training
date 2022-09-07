using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Randomness
{
    public class Generator<T> where T : new()
    {
        private IContinuousDistribution[] _cash;
        private Func<IContinuousDistribution[], Random, T> _generateHandler;
        public Generator()
        {
            _generateHandler = GetHandler();
        }
        public T Generate(Random rnd)
        {
            return _generateHandler(_cash, rnd);
        }

        // (dists, rnd) => new T() { A = dists.Generate(rnd) ...}
        private Func<IContinuousDistribution[], Random, T> GetHandler()
        {
            var properties = typeof(T)
                .GetProperties()
                .Where(z => z.GetCustomAttributes(typeof(FromDestribution), false).Length != 0)
                .ToArray();

            _cash = new IContinuousDistribution[properties.Length];
            var distsArg = Expression.Parameter(typeof(IContinuousDistribution[]), "dists");
            var randArg = Expression.Parameter(typeof(Random), "rnd");
            var bindings = new List<MemberBinding>();
            for (int i = 0; i < properties.Length; i++)
            {
                var dist = GetDistributionType(properties[i])();
                _cash[i] = dist;

                var generateMethod = typeof(IContinuousDistribution)
                .GetMethod(nameof(IContinuousDistribution.Generate));

                var binding = Expression.Bind(
                properties[i],
                Expression.Call(Expression.ArrayIndex(distsArg, Expression.Constant(i)), generateMethod, randArg)
                );
                bindings.Add(binding);
            }

            var body = Expression.MemberInit(
                Expression.New(typeof(T).GetConstructor(new Type[0])),
                bindings
                );
            var lambda = Expression.Lambda<Func<IContinuousDistribution[], Random, T>>(
            body,
            distsArg,
            randArg
            );
            return lambda.Compile();
        }

        static Func<IContinuousDistribution> GetDistributionType(PropertyInfo p)
        {
            var param = p.GetCustomAttribute(typeof(FromDestribution));
            var frDist = param as FromDestribution;
            if (!typeof(IContinuousDistribution).IsAssignableFrom(frDist.DistributionType))
               throw new ArgumentException("Attribute of " + p.Name + " has wrong distribution type " + frDist.DistributionType.Name);

            var properties = frDist.DistributionType
                .GetFields();

            if (properties.Length < frDist.Properties.Length)
                throw new ArgumentException("Too many arguments in " + frDist.DistributionType.Name);

            var propTypes = new Type[frDist.Properties.Length];
            var args = new Expression[frDist.Properties.Length];
            for (int i = 0; i < frDist.Properties.Length; i++)
            {
                propTypes[i] = properties[i].FieldType;
                args[i] = Expression.Constant(frDist.Properties[i]);
            }
            var body = Expression.MemberInit(
            Expression.New(frDist.DistributionType.GetConstructor(propTypes), args)
            );
            var lambda = Expression.Lambda<Func<IContinuousDistribution>>(
            body
            );
            return lambda.Compile();
        }
    }
}