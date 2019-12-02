using BearLib;
using System;

namespace Utils
{
    public class Engine
    {
        public static readonly TimeSpan FrameRate = new TimeSpan(TimeSpan.TicksPerSecond / 60);

        private Func<int, bool> Update { get; }
        private Action<double> Render { get; }
        private AnimationHandler Animations { get; }

        public Engine(int width, int height, string title, Func<int, bool> update, Action<double> render, AnimationHandler animations)
        {
            Update = update;
            Render = render;
            Animations = animations;

            Terminal.Open();
            Terminal.Set(
                $"window: size={width}x{height}," +
                $"cellsize=auto, title='{title}';");
            Terminal.Set("font: square.ttf, size = 12x12;");
            Terminal.Set("input.filter = [keyboard]");
        }

        public void Start()
        {
            Render(0);
        }

        public void Run()
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
                    }

                    accum -= FrameRate;
                }

                double remaining = accum / FrameRate;
                Animations.Run(frameTime, remaining);
                Render(remaining);
            }

            Terminal.Close();
        }
    }
}
