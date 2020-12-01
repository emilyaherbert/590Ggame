using UnityEngine;
namespace HeroClash {
  internal struct Stat {
    private float health;

    internal float AtckSpeed { get; }
    internal float MaxDamage { get; }
    internal float Damage => Random.Range(0.5f, 1.0f) * MaxDamage;
    internal float MaxHealth { get; }
    internal float Health {
      get => health;
      private set => health = Mathf.Max(Mathf.Min(MaxHealth, value), 0);
    }
    internal float MoveSpeed { get; }

    internal Stat(float atckSpeed,
      float maxDamage,
      float maxHealth,
      float moveSpeed,
      float health) {
      AtckSpeed = atckSpeed;
      MaxDamage = maxDamage;
      MaxHealth = maxHealth;
      MoveSpeed = moveSpeed;
      this.health = health;
    }

    internal Stat(float atckSpeed,
      float maxDamage,
      float maxHealth,
      float moveSpeed) {
      AtckSpeed = atckSpeed;
      MaxDamage = maxDamage;
      MaxHealth = maxHealth;
      MoveSpeed = moveSpeed;
      health = MaxHealth;
    }

    internal Stat(Stat oldStat, float health) {
      this = oldStat;
      this.health = health;
    }
  }
}
