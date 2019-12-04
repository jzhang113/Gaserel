using BearLib;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Drawing;
using Utils;

namespace Gaserel
{
    internal class Game
    {
        internal static AnimationHandler Animations;
        internal static Random rand;

        private static readonly int Width = 10;
        private static readonly int Height = 10;

        private static ISettableMapView<double> zero;
        private static ISettableMapView<double> densityMap;
        private static ISettableMapView<double> prevDensityMap;
        private static ISettableMapView<(double, double)> velocityMap;
        private static ISettableMapView<(double, double)> prevVelocityMap;

        private static void Main(string[] args)
        {
            Animations = new AnimationHandler();
            rand = new Random();

            var engine = new Engine(Width, Height, "Gaserel", Update, Render, Animations);
            var map = new Map(Width, Height, 1, GoRogue.Distance.MANHATTAN);
            ISettableMapView<bool> mapview = new ArrayMap<bool>(Width, Height);
            QuickGenerators.GenerateRectangleMap(mapview);
            map.ApplyTerrainOverlay(mapview, (pos, val) => val ? TerrainFactory.Floor(pos) : TerrainFactory.Wall(pos));

            zero = new ArrayMap<double>(Width, Height);
            densityMap = new ArrayMap<double>(Width, Height);
            prevDensityMap = new ArrayMap<double>(Width, Height);
            velocityMap = new ArrayMap<(double, double)>(Width, Height);
            prevVelocityMap = new ArrayMap<(double, double)>(Width, Height);

            engine.Start();
            engine.Run();
        }

        private static bool Update(int input)
        {
            // densityMap.ApplyOverlay(zero);

            switch (input)
            {
                case Terminal.TK_CLOSE:
                    return true;
                case Terminal.TK_Z:
                    densityMap[55] = 100;
                    velocityMap[55] = (-10, 5);
                    break;
                case Terminal.TK_SPACE:
                    Diffusion.Update(densityMap, prevDensityMap, velocityMap, prevVelocityMap);
                    break;
            }

            return false;
        }

        private static void Render(double dt)
        {
            IMapView<double> map = densityMap;

            for (int i = 0; i < Width * Height; i++)
            {
                Terminal.Color(Color.FromArgb(Math.Clamp((int)(map[i] * 256 / 10), 0, 255), 255, 255, 255));
                Terminal.Put(i % 10, i / 10, '█');
            }

            Terminal.Refresh();
        }
    }
}
