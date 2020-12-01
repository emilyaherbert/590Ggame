using UnityEngine;
namespace HeroClash {
  internal struct Target {
    internal Collider Box { get; }
    internal ICharacter Character { get; }
    internal IStructure Structure { get; }

    internal Target(Collider c, ICharacter ch) {
      Box = c;
      Character = ch;
      Structure = null;
    }

    internal Target(Collider c, IStructure st) {
      Box = c;
      Character = null;
      Structure = st;
    }
  }
}
