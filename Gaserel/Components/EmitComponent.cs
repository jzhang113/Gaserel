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
        public int LifeTime { get; }
        public int TickRate { get; }

        private int _subticks;
        private int _ticks;

        public EmitComponent(GasInfo gas, double strength, double dx, double dy, int lifetime = -1, int tickrate = 1)
        {
            GasInfo = gas;
            Strength = strength;
            Dx = dx;
            Dy = dy;
            LifeTime = lifetime;
            TickRate = tickrate;

            _subticks = 0;
            _ticks = 0;
        }

        public void Emit()
        {
            if (++_subticks < TickRate) return;

            _ticks++;
            _subticks = 0;
            GasInfo.Set(Parent.Position, Strength, Dx, Dy);

            if (LifeTime > 0 && _ticks > LifeTime)
            {
                Parent.RemoveComponent(this);
            }
        }
    }
}
