using UnityEngine;
namespace HeroClash {
  internal class Player : MonoBehaviour {
    internal Hero hero;

    private void Update() {
      if (GameManager.paused) { return; }
      if (Input.GetMouseButtonDown(0) &&
          Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            out RaycastHit hit) &&
          !hit.collider.CompareTag("Character") &&
          !hit.collider.CompareTag("Structure")) {
        hero.Move(hit.point);
      } else if (Input.GetMouseButtonDown(1) &&
          Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            out hit) &&
          (hit.collider.CompareTag("Character") ||
          hit.collider.CompareTag("Structure"))) {
        hero.ToAttack(hit.collider);
      }
    }
  }
}
