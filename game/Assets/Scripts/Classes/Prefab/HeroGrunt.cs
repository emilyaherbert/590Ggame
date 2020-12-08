using UnityEngine;
using UnityEngine.AI;
namespace HeroClash {
  internal class HeroGrunt : Hero {
    private const float START_ACCEL = 10.0f,
                        START_ATTACK = 1.0f,
                        START_DAMAGE = 10.0f,
                        START_HEALTH = 100.0f,
                        START_MOVING = 6.0f;

    protected override float AccelGain => 0.5f;
    protected override float AttackLoss => 0.02f;
    protected override float MovingGain => 0.3f;

    public override float DamageGain => 5.0f;
    public override float HealthGain => 20.0f;

    private void Start() {
      anim = GetComponent<Animator>();
      nav = GetComponent<NavMeshAgent>();
      Self = new Stat(START_ACCEL,
        START_ATTACK,
        START_DAMAGE,
        START_HEALTH,
        START_MOVING);
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
    }
  }
}
