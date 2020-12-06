using UnityEngine;
namespace HeroClash {
  internal class GameManager : MonoBehaviour {
    internal static bool paused;

    private const float DIST_EPSILON = 0.1f,
                        SMOOTH_SPEED = 2.0f;
    private readonly Vector3 OFFSET = (100.0f * Vector3.up) + (60.0f * Vector3.back);

    // TODO: remove attr; set programmatically
    [SerializeReference] internal Transform target = default;

    private void LateUpdate() {
      if (Vector3.Distance(transform.position,
        target.position + OFFSET) > DIST_EPSILON) {
        transform.position = Vector3.Lerp(transform.position,
          target.position + OFFSET,
          SMOOTH_SPEED * Time.deltaTime);
      }
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        paused = !paused;
      }
    }
  }
}
