using BearLib;
using Gaserel.Components;
using GoRogue;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Utils;

namespace Gaserel
{
    internal class Game
    {
        internal static AnimationHandler Animations;
        internal static Random rand;

        private static readonly int Width = 60;
        private static readonly int Height = 60;

        private static Map map;

        private static IList<GasInfo> _gasLayers;
        private static GameObject e3;

        private static void Main(string[] args)
        {
            Animations = new AnimationHandler();
            rand = new Random();

            var engine = new Engine(Width, Height, "Gaserel", true, StepUpdate, Render, Animations);

            map = new Map(Width, Height, 1, Distance.MANHATTAN);
            ISettableMapView<bool> mapview = new ArrayMap<bool>(Width, Height);
            QuickGenerators.GenerateRandomRoomsMap(mapview, 20, 8, 20);
            map.ApplyTerrainOverlay(mapview, (pos, val) => val ? TerrainFactory.Floor(pos) : TerrainFactory.Wall(pos));

            _gasLayers = new List<GasInfo>()
            {
                new GasInfo(Width, Height, Color.Red),
                new GasInfo(Width, Height, Color.Blue),
                new GasInfo(Width, Height, Color.Green),
                new GasInfo(Width, Height, Color.Purple)
            };

            Coord p1 = map.Terrain.RandomPosition((_, tile) => tile.IsWalkable);
            var e1 = new GameObject(p1, 1, null);
            e1.AddComponent(new DrawComponent('%', Color.Red));
            e1.AddComponent(new EmitComponent(_gasLayers[0], 100, 6, 100));
            map.AddEntity(e1);

            Coord p2 = map.Terrain.RandomPosition((_, tile) => tile.IsWalkable);
            var e2 = new GameObject(p2, 1, null);
            e2.AddComponent(new DrawComponent('%', Color.Blue));
            e2.AddComponent(new EmitComponent(_gasLayers[1], 200, -100, -10));
            map.AddEntity(e2);

            Coord p3 = map.Terrain.RandomPosition((_, tile) => tile.IsWalkable);
            e3 = new GameObject(p3, 1, null);
            e3.AddComponent(new DrawComponent('@', Color.White));
            e3.IsWalkable = false;

            map.AddEntity(e3);

            engine.Start();
            engine.Run();
        }

        private static bool StepUpdate(int input)
        {
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
                case Terminal.TK_T:
                    break;
            }

            e3.Position = e3.Position.Translate(dx, dy);

            foreach (GasInfo gas in _gasLayers)
            {
                gas.Clear();
            }

            foreach (ISpatialTuple<IGameObject> st in map.Entities)
            {
                st.Item.GetComponent<EmitComponent>()?.Emit();
            }

            // force the walkability map to be an ArrayMap for performance
            var fastWalk = new ArrayMap<bool>(Width, Height);
            foreach (Coord p in map.Positions())
            {
                fastWalk[p] = map.WalkabilityView[p];
            }

            Parallel.ForEach(_gasLayers, gas =>
            {
                gas.Update(fastWalk);
            });

            return false;
        }

        private static void Render(double dt)
        {
            int layer = 1;

            foreach (GasInfo gas in _gasLayers)
            {
                Terminal.Layer(layer++);
                for (int i = 0; i < Width * Height; i++)
                {
                    Terminal.Color(Color.FromArgb(Math.Clamp((int)(gas.DensityMap[i] * 128), 0, 128), gas.Color));
                    Terminal.Put(i % Width, i / Width, '█');
                }
            }

            Terminal.Layer(layer++);
            foreach (Coord p in map.Positions())
            {
                map.Terrain[p].GetComponent<DrawComponent>()?.Draw();
            }

            foreach (ISpatialTuple<IGameObject> st in map.Entities)
            {
                st.Item.GetComponent<DrawComponent>()?.Draw();
            }

            Terminal.Refresh();
        }
    }
}
