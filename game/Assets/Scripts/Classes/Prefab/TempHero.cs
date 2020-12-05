// TODO: delete class
using UnityEngine.AI;
namespace HeroClash {
  internal class TempHero : Hero {
    private const float START_ACCEL = 10.0f,
                        START_ATTACK = 1.0f,
                        START_DAMAGE = 10.0f,
                        START_HEALTH = 100.0f,
                        START_MOVING = 5.0f;

    protected override float AccelGain => 0.25f;
    protected override float AttackLoss => 0.025f;
    protected override float DamageGain => 2.5f;
    protected override float HealthGain => 25.0f;
    protected override float MovingGain => 0.05f;

    private void Start() {
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
