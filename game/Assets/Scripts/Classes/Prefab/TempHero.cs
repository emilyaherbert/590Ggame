// TODO: delete class
namespace HeroClash {
  internal class TempHero : Hero {
    private const float START_ATTACK = 1.0f,
                        START_DAMAGE = 10.0f,
                        START_HEALTH = 100.0f,
                        START_MOVING = 0.5f;

    protected override float AttackLoss => 0.025f;
    protected override float DamageGain => 2.5f;
    protected override float HealthGain => 25.0f;
    protected override float MovingGain => 0.05f;

    private void Start() {
      Self = new Stat(START_ATTACK,
        START_DAMAGE,
        START_HEALTH,
        START_MOVING);
    }
  }
}
