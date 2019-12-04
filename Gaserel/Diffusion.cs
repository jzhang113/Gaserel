using GoRogue;
using GoRogue.MapViews;
using System;

namespace Gaserel
{
    // based on Jos Stam, "Real-Time Fluid Dynamics for Games". Proceedings of the Game Developer Conference, March 2003.
    // http://www.dgp.toronto.edu/people/stam/reality/Research/pub.html
    internal class Diffusion
    {
        private const int iters = 20;

        public static void Update(ISettableMapView<double> density, ISettableMapView<double> prevDensity, ISettableMapView<(double, double)> velocity, ISettableMapView<(double, double)> prevVelocity)
        {
            UpdateVelocity(velocity, prevVelocity, 0.01, 0.01);
            UpdateDensity(density, prevDensity, velocity, 0.1, 0.01);

            foreach (Coord p in density.Positions())
            {
                density[p] *= 0.9;
                velocity[p] = (velocity[p].Item1 * 0.9, velocity[p].Item2 * 0.9);
            }
        }

        private static void UpdateDensity(ISettableMapView<double> density, ISettableMapView<double> prev, IMapView<(double, double)> velocity, double coeff, double dt)
        {
            AddSource(density, prev, dt);
            Swap(density, prev);
            Diffuse(0, density, prev, coeff, dt);
            Swap(density, prev);
            Advect(0, density, prev, velocity, dt);
        }

        private static void UpdateVelocity(ISettableMapView<(double, double)> velocity, ISettableMapView<(double, double)> prev, double visc, double dt)
        {
            LambdaSettableTranslationMap<(double, double), double> vx = GetVx(velocity);
            LambdaSettableTranslationMap<(double, double), double> vy = GetVy(velocity);
            LambdaSettableTranslationMap<(double, double), double> px = GetVx(prev);
            LambdaSettableTranslationMap<(double, double), double> py = GetVy(prev);

            AddSource(vx, px, dt);
            AddSource(vy, py, dt);
            Swap(velocity, prev);
            Diffuse(1, vx, px, visc, dt);
            Diffuse(1, vy, py, visc, dt);
            Project(velocity, px, py);
            Swap(velocity, prev);
            Advect(1, vx, px, prev, dt);
            Advect(1, vy, py, prev, dt);
            Project(velocity, px, py);
        }

        private static LambdaSettableTranslationMap<(double, double), double> GetVy(ISettableMapView<(double, double)> velocity) => new LambdaSettableTranslationMap<(double, double), double>(velocity, (_, vel) => vel.Item2, (pos, ny) => (velocity[pos].Item1, ny));
        private static LambdaSettableTranslationMap<(double, double), double> GetVx(ISettableMapView<(double, double)> velocity) => new LambdaSettableTranslationMap<(double, double), double>(velocity, (_, vel) => vel.Item1, (pos, nx) => (nx, velocity[pos].Item2));

        private static void Project(ISettableMapView<(double, double)> velocity, ISettableMapView<double> p, ISettableMapView<double> div)
        {
            double n = Math.Min(velocity.Width, velocity.Height);
            double h = 1 / n;

            for (int x = 1; x < velocity.Width - 1; x++)
            {
                for (int y = 1; y < velocity.Height - 1; y++)
                {
                    div[x, y] = -0.5 * h * (velocity[x + 1, y].Item1 - velocity[x - 1, y].Item1 + velocity[x, y + 1].Item2 + velocity[x, y - 1].Item2);
                    p[x, y] = 0;
                }
            }

            SetBoundary(0, div);
            SetBoundary(0, p);

            for (int k = 0; k < iters; k++)
            {
                for (int x = 1; x < velocity.Width - 1; x++)
                {
                    for (int y = 1; y < velocity.Height - 1; y++)
                    {
                        p[x, y] = (div[x, y] + p[x - 1, y] + p[x + 1, y] +
                                    p[x, y - 1] + p[x, y + 1]) / 4;
                    }
                }

                SetBoundary(0, p);
            }

            for (int x = 1; x < velocity.Width - 1; x++)
            {
                for (int y = 1; y < velocity.Height - 1; y++)
                {
                    (double vx, double vy) = velocity[x, y];
                    velocity[x, y] = (vx - 0.5 * (p[x + 1, y] - p[x - 1, y]) / h, vy - 0.5 * (p[x, y+1] - p[x, y-1]) / h);
                }
            }

            SetBoundary(2, GetVx(velocity));
            SetBoundary(1, GetVy(velocity));
        }

        private static void Advect(int b, ISettableMapView<double> density, IMapView<double> prev, IMapView<(double, double)> velocity, double dt)
        {
            double n = Math.Min(density.Width, density.Height);
            double dt0 = dt * n;

            for (int x = 1; x < density.Width - 1; x++)
            {
                for (int y = 1; y < density.Height - 1; y++)
                {
                    double xVal = x - dt0 * velocity[x, y].Item1;
                    double yVal = y - dt0 * velocity[x, y].Item2;

                    xVal = Math.Clamp(xVal, 0.5, density.Width - 1.5);
                    int i0 = (int)xVal;
                    int i1 = i0 + 1;

                    yVal = Math.Clamp(yVal, 0.5, density.Height - 1.5);
                    int j0 = (int)yVal;
                    int j1 = j0 + 1;

                    double s1 = xVal - i0;
                    double s0 = 1 - s1;

                    double t1 = yVal - j0;
                    double t0 = 1 - t1;

                    density[x, y] = s0 * (t0 * prev[i0, j0] + t1 * prev[i0, j1]) + s1 * (t0 * prev[i1, j0] + t1 * prev[i1, j1]);
                }
            }

            SetBoundary(b, density);
        }

        private static void Diffuse(int b, ISettableMapView<double> density, IMapView<double> prev, double coeff, double dt)
        {
            double a = coeff * dt * density.Width * density.Height;

            for (int k = 0; k < iters; k++)
            {
                for (int x = 1; x < density.Width - 1; x++)
                {
                    for (int y = 1; y < density.Height - 1; y++)
                    {
                        double diffusion = density[x - 1, y] + density[x + 1, y] + density[x, y - 1] + density[x, y + 1];
                        density[x, y] = (prev[x, y] + a * diffusion) / (1 + 4 * a);
                    }
                }

                SetBoundary(b, density);
            }
        }

        private static void AddSource(ISettableMapView<double> density, IMapView<double> source, double dt)
        {
            foreach (Coord pos in source.Positions())
            {
                density[pos] += dt * source[pos];
            }
        }

        private static void SetBoundary(int b, ISettableMapView<double> density)
        {
            int w = density.Width;
            int h = density.Height;

            for (int x = 1; x < w - 1; x++)
            {
                density[x, 0] = b == 1 ? -density[x, 1] : density[x, 1];
                density[x, h - 1] = b == 1 ? -density[x, h - 2] : density[x, h - 2];
            }

            for (int j = 1; j < h - 1; j++)
            {
                density[0, j] = b == 2 ? -density[1, j] : density[1, j];
                density[w - 1, j] = b == 2 ? -density[w - 2, j] : density[w - 2, j];
            }

            density[0, 0] = 0.5 * (density[1, 0] + density[0, 1]);
            density[0, h - 1] = 0.5 * (density[1, h - 1] + density[0, h - 2]);
            density[w - 1, 0] = 0.5 * (density[w - 2, 0] + density[w - 1, 1]);
            density[w - 1, h - 1] = 0.5 * (density[w - 2, h - 1] + density[w - 1, h - 2]);
        }

        private static void Swap<T>(ISettableMapView<T> a, ISettableMapView<T> b)
        {
            foreach (Coord pos in a.Positions())
            {
                T temp = a[pos];
                a[pos] = b[pos];
                b[pos] = temp;
            }
        }
    }
}
