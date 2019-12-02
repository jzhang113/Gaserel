using Gaserel.Animations;
using Gaserel.Components;
using GoRogue;
using GoRogue.GameFramework;
using System;
using System.Drawing;

namespace Gaserel
{
    static class EntityFactory
    {
        public static IGameObject Player(Coord position)
        {
            var player = new GameObject(position, 1, null, false, false, true);
            player.AddComponent(new DrawComponent('@', Color.White));
            player.AddComponent(new HealthComponent(10));
            player.Moved += OnPlayerMove;

            return player;
        }

        private static void OnPlayerMove(object sender, ItemMovedEventArgs<IGameObject> e)
        {
            Game.Animations.Add(
                sender.GetHashCode(),
                new FlashAnimation(
                    new Coord[1] { e.OldPosition },
                    Color.White,
                    TimeSpan.FromMilliseconds(600),
                    delay: TimeSpan.FromMilliseconds(100)));
        }
    }
}
