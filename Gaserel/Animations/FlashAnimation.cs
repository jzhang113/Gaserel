using BearLib;
using GoRogue;
using System;
using System.Collections.Generic;
using System.Drawing;
using Utils;

namespace Gaserel.Animations
{
    internal class FlashAnimation : IAnimation
    {
        public TimeSpan Duration { get; }
        public TimeSpan CurrentTime { get; private set; }
        public TimeSpan EndTime { get; }

        public bool Blocking { get; }
        public TimeSpan Delay { get; }
        public bool UpdateNext => !Blocking && CurrentTime >= Delay;

        private readonly IEnumerable<Coord> _pos;
        private readonly Color _color;

        public FlashAnimation(IEnumerable<Coord> pos, in Color color, TimeSpan duration, bool blocking = false, TimeSpan delay = default)
        {
            _pos = pos;
            _color = color;

            Duration = duration;
            CurrentTime = TimeSpan.Zero;
            EndTime = CurrentTime + Duration;

            Blocking = blocking;
            Delay = delay;
        }

        public bool Update(TimeSpan dt)
        {
            CurrentTime += dt;
            return CurrentTime >= EndTime;
        }

        public void Draw()
        {
            double fracPassed = CurrentTime / Duration;
            Color between = _color.Blend(Color.Black, fracPassed);
            Terminal.Color(between);
            Terminal.Layer(2);
            foreach (Coord pos in _pos) {
                Terminal.Put(pos.X, pos.Y, '▓');
            }
            Terminal.Layer(1);
        }
    }
}
