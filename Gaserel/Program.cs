using BearLib;
using Gaserel.Components;
using GoRogue;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using System;
using System.Drawing;
using System.Linq;
using Utils;

namespace Gaserel
{
    internal class Game
    {
        internal static AnimationHandler Animations;
        internal static Random rand;
        internal static Diffusion diffu;

        private static readonly int Width = 60;
        private static readonly int Height = 60;
        private static Coord me = (5, 5);

        private static Map map;
        private static ISettableMapView<double> densityMap;
        private static ISettableMapView<(double, double)> velocityMap;
        private static ISettableMapView<double> newDensityMap;
        private static ISettableMapView<(double, double)> newVelocityMap;

        private static void Main(string[] args)
        {
            Animations = new AnimationHandler();
            rand = new Random();

            var engine = new Engine(Width, Height, "Gaserel", true, StepUpdate, Render, Animations);

            map = new Map(Width, Height, 1, Distance.MANHATTAN);
            ISettableMapView<bool> mapview = new ArrayMap<bool>(Width, Height);
            QuickGenerators.GenerateRandomRoomsMap(mapview, 20, 8, 20);
            map.ApplyTerrainOverlay(mapview, (pos, val) => val ? TerrainFactory.Floor(pos) : TerrainFactory.Wall(pos));

            densityMap = new ArrayMap<double>(Width, Height);
            velocityMap = new ArrayMap<(double, double)>(Width, Height);
            newDensityMap = new ArrayMap<double>(Width, Height);
            newVelocityMap = new ArrayMap<(double, double)>(Width, Height);

            diffu = new Diffusion(densityMap, velocityMap);

            engine.Start();
            engine.Run();
        }

        private static bool StepUpdate(int input)
        {
            foreach (Coord p in newDensityMap.Positions())
            {
                newDensityMap[p] = 0;
                newVelocityMap[p] = (0, 0);
            }

            int dx = 0;
            int dy = 0;

            switch (input)
            {
                case Terminal.TK_ESCAPE:
                case Terminal.TK_CLOSE:
                    return true;
                case Terminal.TK_A:
                    dx = -1;
                    break;
                case Terminal.TK_D:
                    dx = 1;
                    break;
                case Terminal.TK_W:
                    dy = -1;
                    break;
                case Terminal.TK_S:
                    dy = 1;
                    break;
            }

            me = me.Translate(dx, dy);

            if (Terminal.Check(Terminal.TK_MOUSE_LEFT))
            {
                var pos = new Coord(Terminal.State(Terminal.TK_MOUSE_X), Terminal.State(Terminal.TK_MOUSE_Y));
                newDensityMap[me] = 200;
                var d = Coord.EuclideanDistanceMagnitude(pos, me);
                d = Math.Sqrt(d);
                newVelocityMap[me] = (d * (pos.X - me.X) * 50, d * (pos.Y - me.Y) * 50);
            }

            diffu.Update(newDensityMap, newVelocityMap, map.WalkabilityView, (double)Engine.FrameRate.Ticks / TimeSpan.TicksPerSecond);

            return false;
        }

        private static void Render(double dt)
        {
            Terminal.Layer(1);
            for (int i = 0; i < Width * Height; i++)
            {
                Terminal.Color(Color.FromArgb(Math.Clamp((int)(densityMap[i] * 256), 0, 255), 255, 255, 255));
                Terminal.Put(i % Width, i / Width, '█');
            }

            Terminal.Layer(2);
            foreach (Coord p in map.Positions())
            {
                map.Terrain[p].GetComponent<DrawComponent>()?.Draw();
            }

            Terminal.Put(me, '@');

            Terminal.Refresh();
        }
    }
}
