using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class Minion : MonoBehaviour, ICharacter {
    internal const float START_ACCEL = 10.0f,
                          START_ATTACK = 0.5f,
                          START_DAMAGE = 10.0f,
                          START_HEALTH = 100.0f,
                          START_MOVING = 10.0f;

    public float DamageGain => 5.0f;
    public float HealthGain => 10.0f;
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    public TEAM Team { get; set; }

    public IEnumerator Attack() {
      throw new System.NotImplementedException();
    }
  }
}
