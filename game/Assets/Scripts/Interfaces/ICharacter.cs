using System.Collections;
namespace HeroClash {
  internal interface ICharacter {
    float DamageGain { get; }
    float HealthGain { get; }
    Stat Self { get; set; }
    Target Them { get; set; }
    STATE State { get; set; }
    TEAM Team { get; set; }

    IEnumerator Attack();
  }
}
