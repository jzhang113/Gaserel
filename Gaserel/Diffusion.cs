using GoRogue;
using GoRogue.MapViews;
using System;

namespace Gaserel
{
    // based on Jos Stam, "Real-Time Fluid Dynamics for Games". Proceedings of the Game Developer Conference, March 2003.
    // https://pdfs.semanticscholar.org/847f/819a4ea14bd789aca8bc88e85e906cfc657c.pdf
    internal class Diffusion
    {
        private const int iters = 20;

        private ISettableMapView<double> Density { get; }
        private ISettableMapView<(double, double)> Velocity { get; }

        public Diffusion(ISettableMapView<double> density, ISettableMapView<(double, double)> velocity)
        {
            Density = density;
            Velocity = velocity;
        }

        public void Update(ISettableMapView<double> newDensity, ISettableMapView<(double, double)> newVelocity, IMapView<bool> boundary, double dt)
        {
            //foreach (Coord p in Density.Positions())
            //{
            //    double decay = Math.Pow(1 - 0.4, dt);
            //    Density[p] *= decay;
            //}

            int w = newDensity.Width;
            int h = newDensity.Height;

            var vx = new ArrayMap<double>(w, h);
            var vy = new ArrayMap<double>(w, h);
            var px = new ArrayMap<double>(w, h);
            var py = new ArrayMap<double>(w, h);
            var walk = new ArrayMap<bool>(w, h);

            foreach (Coord p in Velocity.Positions())
            {
                (vx[p], vy[p]) = Velocity[p];
                (px[p], py[p]) = newVelocity[p];
                walk[p] = boundary[p];
            }

            UpdateVelocity(vx, vy, px, py, walk, 0.01, dt);
            UpdateDensity(Density, newDensity, vx, vy, walk, 0.1, dt);

            foreach (Coord p in vx.Positions())
            {
                Velocity[p] = (vx[p], vy[p]);
            }
        }

        private static void UpdateDensity(
            ISettableMapView<double> density, ISettableMapView<double> prev,
            IMapView<double> vx, IMapView<double> vy,
            IMapView<bool> boundary,
            double coeff, double dt)
        {
            AddSource(density, prev, dt);
            Diffuse(0, prev, density, boundary, coeff, dt);
            Advect(0, density, prev, vx, vy, boundary, dt);
        }

        private static void UpdateVelocity(
            ISettableMapView<double> vx, ISettableMapView<double> vy,
            ISettableMapView<double> px, ISettableMapView<double> py,
            IMapView<bool> boundary,
            double visc, double dt)
        {
            AddSource(vx, px, dt);
            AddSource(vy, py, dt);

            Diffuse(1, px, vx, boundary, visc, dt);
            Diffuse(2, py, vy, boundary, visc, dt);
            Project(px, py, vx, vy, boundary);

            Advect(1, vx, px, px, py, boundary, dt);
            Advect(2, vy, py, px, py, boundary, dt);
            Project(vx, vy, px, py, boundary);
        }

        private static void Project(
            ISettableMapView<double> vx, ISettableMapView<double> vy,
            ISettableMapView<double> p, ISettableMapView<double> div,
            IMapView<bool> boundary)
        {
            for (int x = 1; x < vx.Width - 1; x++)
            {
                for (int y = 1; y < vy.Height - 1; y++)
                {
                    div[x, y] = -0.5 / vx.Width * (vx[x + 1, y] - vx[x - 1, y]) - 0.5 / vy.Height * (vy[x, y + 1] - vy[x, y - 1]);
                    p[x, y] = 0;
                }
            }

            SetBoundary(0, div);
            SetBoundary(0, p);

            for (int k = 0; k < iters; k++)
            {
                for (int x = 1; x < vx.Width - 1; x++)
                {
                    for (int y = 1; y < vy.Height - 1; y++)
                    {
                        if (boundary[x, y])
                        {
                            p[x, y] = (div[x, y] + p[x - 1, y] + p[x + 1, y] +
                                        p[x, y - 1] + p[x, y + 1]) / 4;
                        }
                        else
                        {
                            p[x, y] = 0;
                        }
                    }
                }

                SetBoundary(0, p);
            }

            for (int x = 1; x < vx.Width - 1; x++)
            {
                for (int y = 1; y < vy.Height - 1; y++)
                {

                    vx[x, y] = vx[x, y] - 0.5 * (p[x + 1, y] - p[x - 1, y]) * vx.Width;
                    vy[x, y] = vy[x, y] - 0.5 * (p[x, y + 1] - p[x, y - 1]) * vy.Height;

                }
            }

            SetBoundary(1, vx);
            SetBoundary(2, vy);
        }

        private static void Advect(int b,
            ISettableMapView<double> density, IMapView<double> prev,
            IMapView<double> vx, IMapView<double> vy,
            IMapView<bool> boundary,
            double dt)
        {
            for (int x = 1; x < density.Width - 1; x++)
            {
                for (int y = 1; y < density.Height - 1; y++)
                {
                    double xVal = x - dt * density.Width * vx[x, y];
                    double yVal = y - dt * density.Height * vy[x, y];

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

        private static void Diffuse(int b, ISettableMapView<double> density, IMapView<double> prev, IMapView<bool> boundary, double coeff, double dt)
        {
            double a = coeff * dt * density.Width * density.Height;

            for (int k = 0; k < iters; k++)
            {
                for (int x = 1; x < density.Width - 1; x++)
                {
                    for (int y = 1; y < density.Height - 1; y++)
                    {
                        if (!boundary[x, y])
                        {
                            density[x, y] = 0;
                            continue;
                        }

                        double diffusion = 0;
                        int open = 0;

                        if (boundary[x, y - 1])
                        {
                            diffusion += density[x, y - 1];
                            open++;
                        }

                        if (boundary[x, y + 1])
                        {
                            diffusion += density[x, y + 1];
                            open++;
                        }

                        if (boundary[x - 1, y])
                        {
                            diffusion += density[x - 1, y];
                            open++;
                        }

                        if (boundary[x + 1, y])
                        {
                            diffusion += density[x + 1, y];
                            open++;
                        }

                        density[x, y] = (prev[x, y] + a * diffusion) / (1 + open * a);
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
                density[x, 0] = b == 2 ? -density[x, 1] : 0;
                density[x, h - 1] = b == 2 ? -density[x, h - 2] : 0;
            }

            for (int j = 1; j < h - 1; j++)
            {
                density[0, j] = b == 1 ? -density[1, j] : 0;
                density[w - 1, j] = b == 1 ? -density[w - 2, j] : 0;
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
