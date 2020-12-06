using UnityEngine.AI;
namespace HeroClash {
  internal class HeroGolem : Hero {
    private const float START_ACCEL = 8.0f,
                        START_ATTACK = 1.7f,
                        START_DAMAGE = 15.0f,
                        START_HEALTH = 120.0f,
                        START_MOVING = 5.0f;

    protected override float AccelGain => 0.25f;
    protected override float AttackLoss => 0.03f;
    protected override float MovingGain => 0.2f;

    public override float DamageGain => 3.0f;
    public override float HealthGain => 30.0f;

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
