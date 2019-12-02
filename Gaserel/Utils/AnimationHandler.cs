using System;
using System.Collections.Generic;

namespace Utils
{
    public class AnimationHandler
    {
        private IDictionary<int, List<IAnimation>> Current { get; }

        public AnimationHandler()
        {
            Current = new Dictionary<int, List<IAnimation>>();
        }

        public void Clear() => Current.Clear();

        public void Add(int id, IAnimation animation)
        {
            if (Current.TryGetValue(id, out List<IAnimation> queue))
                queue.Add(animation);
            else
                Current.Add(id, new List<IAnimation>() { animation });
        }

        public bool IsDone()
        {
            foreach ((int _, List<IAnimation> queue) in Current)
            {
                if (queue.Count != 0)
                    return false;
            }

            return true;
        }

        public void Run(TimeSpan frameTime, double remaining)
        {
            foreach ((int _, List<IAnimation> queue) in Current)
            {
                var removeList = new List<IAnimation>();
                foreach (IAnimation animation in queue)
                {
                    if (animation.Update(frameTime))
                        removeList.Add(animation);

                    if (!animation.UpdateNext)
                        break;
                }

                foreach (IAnimation done in removeList)
                {
                    queue.Remove(done);
                }
            }
        }

        public void Draw()
        {
            foreach ((int _, List<IAnimation> queue) in Current)
            {
                foreach (IAnimation animation in queue)
                {
                    animation.Draw();

                    if (!animation.UpdateNext)
                        break;
                }
            }
        }
    }
}
