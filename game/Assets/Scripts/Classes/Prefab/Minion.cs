using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class Minion : MonoBehaviour, ICharacter {
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    public TEAM Team { get; set; }

    public void Move(Vector3 vector3) {
      throw new System.NotImplementedException();
    }

    public IEnumerator Attack() {
      throw new System.NotImplementedException();
    }
  }
}
