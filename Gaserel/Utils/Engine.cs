using BearLib;
using System;

namespace Utils
{
    public class Engine
    {
        public static readonly TimeSpan FrameRate = new TimeSpan(TimeSpan.TicksPerSecond / 60);

        private bool RealTime { get; }
        private Func<int, bool> StepUpdate { get; }
        private Action<double> Render { get; }
        private AnimationHandler Animations { get; }

        public Engine(int width, int height, string title, bool realTime, Func<int, bool> stepUpdate, Action<double> render, AnimationHandler animations)
        {
            RealTime = realTime;
            StepUpdate = stepUpdate;
            Render = render;
            Animations = animations;

            Terminal.Open();
            Terminal.Set(
                $"window: size={width}x{height}," +
                $"cellsize=auto, title='{title}';");
            Terminal.Set("font: square.ttf, size = 12x12;");
            Terminal.Set("input.filter = [keyboard, mouse]");
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
                        exiting = StepUpdate(Terminal.Read());
                    }
                    else if (RealTime)
                    {
                        exiting = StepUpdate(-1);
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
