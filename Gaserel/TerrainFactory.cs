using GoRogue;
using GoRogue.GameFramework;
using Gaserel.Components;
using System.Drawing;

namespace Gaserel
{
    static class TerrainFactory
    {
        public static IGameObject Floor(Coord position)
        {
            var floor = new GameObject(position, 0, null, true, true, true);
            floor.AddComponent(new DrawComponent('.', Color.White));
            return floor;
        }

        public static IGameObject Wall(Coord position)
        {
            var wall = new GameObject(position, 0, null, true, false, false);
            wall.AddComponent(new DrawComponent('#', Color.White));
            return wall;
        }
    }
}
