using BearLib;
using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;
using System.Drawing;

namespace Gaserel.Components
{
    internal class DrawComponent : IGameObjectComponent
    {
        public IGameObject Parent { get; set; }
        public char Symbol { get; }
        public Color Color { get; }

        public DrawComponent(char symbol, Color color)
        {
            Symbol = symbol;
            Color = color;
        }

        public void Draw()
        {
            Terminal.Color(Color);
            Terminal.Put(Parent.Position, Symbol);
        }
    }
}
