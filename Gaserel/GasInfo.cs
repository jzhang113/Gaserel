using GoRogue;
using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Utils;

namespace Gaserel
{
    class GasInfo
    {
        public ISettableMapView<double> DensityMap { get; }
        public ISettableMapView<(double, double)> VelocityMap { get; }
        public ISettableMapView<double> NewDensityMap { get; }
        public ISettableMapView<(double, double)> NewVelocityMap { get; }

        public Color Color { get; }
        public double Visc { get; }
        public double Coeff { get; }

        private bool _hasChange;

        public GasInfo(int width, int height, Color color, double visc = 0.1, double coeff = 0.001)
        {
            DensityMap = new ArrayMap<double>(width, height);
            VelocityMap = new ArrayMap<(double, double)>(width, height);
            NewDensityMap = new ArrayMap<double>(width, height);
            NewVelocityMap = new ArrayMap<(double, double)>(width, height);
            Color = color;
            Visc = visc;
            Coeff = coeff;
            _hasChange = false;
        }

        public void Clear()
        {
            foreach (Coord p in NewDensityMap.Positions())
            {
                NewDensityMap[p] = 0;
                NewVelocityMap[p] = (0, 0);
            }

            _hasChange = false;
        }

        public void Set(Coord position, double strength, double dx, double dy)
        {
            NewDensityMap[position] = strength;
            NewVelocityMap[position] = (dx, dy);
            _hasChange = true;
        }

        public void Update(ArrayMap<bool> walkability)
        {
            if (!_hasChange) return;

            Diffusion.Update(
                DensityMap, NewDensityMap,
                VelocityMap, NewVelocityMap,
                walkability, Visc, Coeff, (double)Engine.FrameRate.Ticks / TimeSpan.TicksPerSecond);
        }
    }
}
