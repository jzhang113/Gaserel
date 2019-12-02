using MathNet.Numerics.LinearAlgebra;
using System.Linq;

namespace Gaserel
{
    internal class Diffusion
    {
        public static Vector<double> ForwardStep(Vector<double> u, double dx, double dt, double K = 0.1)
        {
            Vector<double> dudt = Diffuse(u, dx, K);
            return (u + dt * dudt) * (1 - 0.1);
        }

        public static Vector<double> Diffuse(Vector<double> u, double dx, double K = 1)
        {
            u = PairwiseDiff(u);

            Vector<double> F = u / dx;
            F = -K * F;

            //F = PairwiseDiff(F);
            F = -F / dx;

            return F;
        }

        private static Vector<double> PairwiseDiff(Vector<double> u)
        {
            double[] store = new double[u.Count];
            double prev = u.First();

            for (int i = 1; i < u.Count; i++)
            {
                double item = u[i];
                double diff = item - prev;
                store[i - 1] = diff;
                prev = item;
            }

            store[u.Count - 1] = 0;
            return Vector<double>.Build.Dense(store);
        }
    }
}
