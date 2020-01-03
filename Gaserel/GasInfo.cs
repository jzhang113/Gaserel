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
        public ISettableMapView<double> DensityMap { get; set; }
        public ISettableMapView<(double, double)> VelocityMap { get; set; }
        public ISettableMapView<double> NewDensityMap { get; set; }
        public ISettableMapView<(double, double)> NewVelocityMap { get; set; }
        public Color Color { get; set; }

        public GasInfo(int width, int height, Color color)
        {
            Color = color;
            DensityMap = new ArrayMap<double>(width, height);
            VelocityMap = new ArrayMap<(double, double)>(width, height);
            NewDensityMap = new ArrayMap<double>(width, height);
            NewVelocityMap = new ArrayMap<(double, double)>(width, height);
        }

        public void Clear()
        {
            foreach (Coord p in NewDensityMap.Positions())
            {
                NewDensityMap[p] = 0;
                NewVelocityMap[p] = (0, 0);
            }
        }

        public void Update(IMapView<bool> walkability)
        {
            Diffusion.Update(
                DensityMap, NewDensityMap,
                VelocityMap, NewVelocityMap,
                walkability, (double)Engine.FrameRate.Ticks / TimeSpan.TicksPerSecond);
        }
    }
}
