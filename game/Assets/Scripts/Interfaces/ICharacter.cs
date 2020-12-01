using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal interface ICharacter {
    Stat Self { get; set; }
    Target Them { get; set; }
    STATE State { get; set; }
    TEAM Team { get; set; }

    void Move(Vector3 vector3);
    IEnumerator Attack();
  }
}
