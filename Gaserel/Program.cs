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
        public static readonly TimeSpan FrameRate = new TimeSpan(TimeSpan.TicksPerSecond / 60);
        internal static AnimationHandler Animations;

        private static readonly int Width = 10;
        private static readonly int Height = 10;
        private static ISettableMapView<double> densityMap;

        private static void Main(string[] args)
        {
            Terminal.Open();
            Terminal.Set(
                $"window: size={15}x{15}," +
                $"cellsize=auto, title='GeomanceRL';");
            Terminal.Set("font: square.ttf, size = 12x12;");
            Terminal.Set("input.filter = [keyboard]");

            Animations = new AnimationHandler();
            var r = new Random();

            var map = new Map(Width, Height, 1, GoRogue.Distance.MANHATTAN);
            ISettableMapView<bool> mapview = new ArrayMap<bool>(Width, Height);
            QuickGenerators.GenerateRectangleMap(mapview);
            map.ApplyTerrainOverlay(mapview, (pos, val) => val ? TerrainFactory.Floor(pos) : TerrainFactory.Wall(pos));

            densityMap = new ArrayMap<double>(Width, Height);
            ISettableMapView<double> velocity = new ArrayMap<double>(Width, Height);
            
            for (int i = 0; i < 10 * 10; i++)
            {
                densityMap[i] = r.NextDouble();
            }

            Render();
            Run();
        }

        private static void Run()
        {
            const int updateLimit = 10;
            bool exiting = false;
            DateTime currentTime = DateTime.UtcNow;
            var accum = new TimeSpan();

            TimeSpan maxDt = FrameRate * updateLimit;

            while (!exiting)
            {
                DateTime newTime = DateTime.UtcNow;
                TimeSpan frameTime = newTime - currentTime;
                if (frameTime > maxDt)
                {
                    frameTime = maxDt;
                }

                currentTime = newTime;
                accum += frameTime;

                while (accum >= FrameRate)
                {
                    if (Terminal.HasInput())
                    {
                        exiting = Update(Terminal.Read());
                        //RunSystems();
                    }

                    accum -= FrameRate;
                }

                double remaining = accum / FrameRate;
                Animations.Run(frameTime, remaining);
                Render();
            }

            Terminal.Close();
        }

        private static bool Update(int input)
        {
            for (int row = 0; row < 10; row++)
            {
                Vector<double> rowVector = ExtractRow(densityMap, row);
                rowVector = Diffusion.ForwardStep(rowVector, 0.1, 0.05);
                ApplyRow(densityMap, row, rowVector);
            }

            for (int col = 0; col < 10; col++)
            {
                Vector<double> colVector = ExtractCol(densityMap, col);
                colVector = Diffusion.ForwardStep(colVector, 0.1, 0.05);
                ApplyCol(densityMap, col, colVector);
            }

            Render();
            return false;
        }

        private static Vector<double> ExtractRow(ISettableMapView<double> densityMap, int row)
        {
            double[] store = new double[Width];
            for (int col = 0; col < Width; col++)
            {
                store[col] = densityMap[row, col];
            }

            return Vector<double>.Build.Dense(store);
        }

        private static void ApplyRow(ISettableMapView<double> densityMap, int row, Vector<double> rowVector)
        {
            for (int col = 0; col < Height; col++)
            {
                densityMap[row, col] = rowVector[col];
            }
        }

        private static Vector<double> ExtractCol(ISettableMapView<double> densityMap, int col)
        {
            double[] store = new double[Width];
            for (int row = 0; row < Height; row++)
            {
                store[row] = densityMap[row, col];
            }

            return Vector<double>.Build.Dense(store);
        }

        private static void ApplyCol(ISettableMapView<double> densityMap, int col, Vector<double> colVector)
        {
            for (int row = 0; row < Height; row++)
            {
                densityMap[row, col] = colVector[row];
            }
        }

        private static void Render()
        {
            IMapView<double> map = densityMap;

            for (int i = 0; i < Width * Height; i++)
            {
                Terminal.Color(Color.FromArgb(Clamp((int)(map[i] * 256), 0, 255), 255, 255, 255));
                Terminal.Put(i % 10, i / 10, '█');
            }

            Terminal.Refresh();
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }
}
