using System;
using GoRogue;
using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;

namespace Gaserel.Components
{
    internal class EmitComponent : IGameObjectComponent
    {
        public IGameObject Parent { get; set; }
        public GasInfo GasInfo { get; }
        public double Strength { get; set; }
        public double Dx { get; }
        public double Dy { get; }

        public EmitComponent(GasInfo gas, double strength, double dx, double dy)
        {
            GasInfo = gas;
            Strength = strength;
            Dx = dx;
            Dy = dy;
        }

        public void SetGasMap(Coord pos)
        {
            GasInfo.NewDensityMap[pos] = Strength;
            GasInfo.NewVelocityMap[pos] = (Dx, Dy);
        }
    }
}
