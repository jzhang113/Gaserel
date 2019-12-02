using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;

namespace Gaserel.Components
{
    internal class HealthComponent : IGameObjectComponent
    {
        public IGameObject Parent { get; set; }
        public int MaxHealth { get; }
        public int Health { get; private set; }

        public HealthComponent(int maxHealth)
        {
            MaxHealth = maxHealth;
            Health = MaxHealth;
        }

        public void Damage(int amt) => Health -= amt;
    }
}
