using UnityEngine;
namespace HeroClash {
  internal class TowerProjectile : MonoBehaviour {
    private const float MAX_DIST_DELTA = 1.0f;

    internal float damage;
    internal Target target;

    private void OnTriggerEnter(Collider other) {
      if (other == target.Box) {
        target.Character.Self = new Stat(target.Character.Self,
          target.Character.Self.Health - damage);
        Destroy(gameObject);
      }
    }

    private void Update() {
      if (GameManager.paused) { return; }
      if (target.Box != null) {
        transform.position = Vector3.MoveTowards(transform.position,
          target.Box.transform.position,
          MAX_DIST_DELTA);
      } else {
        Destroy(gameObject);
      }
    }
  }
}
