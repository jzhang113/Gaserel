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

        private static ISettableMapView<double> densityMap;

        private static void Main(string[] args)
        {
            Animations = new AnimationHandler();
            rand = new Random();

            var engine = new Engine(Width, Height, "Gaserel", Update, Render, Animations);
            var map = new Map(Width, Height, 1, GoRogue.Distance.MANHATTAN);
            ISettableMapView<bool> mapview = new ArrayMap<bool>(Width, Height);
            QuickGenerators.GenerateRectangleMap(mapview);
            map.ApplyTerrainOverlay(mapview, (pos, val) => val ? TerrainFactory.Floor(pos) : TerrainFactory.Wall(pos));

            densityMap = new ArrayMap<double>(Width, Height);
            ISettableMapView<double> velocity = new ArrayMap<double>(Width, Height);
            
            for (int i = 0; i < 10 * 10; i++)
            {
                densityMap[i] = rand.NextDouble();
            }

            engine.Start();
            engine.Run();
        }

        private static bool Update(int input)
        {
            switch (input)
            {
                case Terminal.TK_CLOSE:
                    return true;
            }

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

            return false;
        }

        private static void Render(double dt)
        {
            IMapView<double> map = densityMap;

            for (int i = 0; i < Width * Height; i++)
            {
                Terminal.Color(Color.FromArgb(Math.Clamp((int)(map[i] * 256), 0, 255), 255, 255, 255));
                Terminal.Put(i % 10, i / 10, '█');
            }

            Terminal.Refresh();
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
    }
}
