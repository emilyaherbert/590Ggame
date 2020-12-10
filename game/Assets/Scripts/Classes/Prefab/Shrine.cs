using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class Shrine : MonoBehaviour, IStructure {
    private const float HEAL_AMNT = 0.15f,
                        HEAL_RATE = 5.0f,
                        MAX_INT = 10000.0f;

    private float integrity;
    private int wave;
    private Coroutine heal;
    private Renderer render;
    [SerializeReference]
    private GameObject opposingShrine = default,
                       prefab = default;

    [SerializeReference] private Material[] materials = new Material[2];

    public float Integrity {
      get => integrity;
      set => integrity = Mathf.Max(Mathf.Min(MAX_INT, value), 0);
    }
    public float SpawnRate => 15.0f;
    [field: SerializeField] public TEAM Team { get; set; }

    private void Start() {
      render = transform.parent.gameObject.GetComponent<Renderer>();
      Integrity = MAX_INT;
      _ = StartCoroutine(nameof(Spawn));
    }

    private IEnumerator Heal(Hero hero) {
      while (true) {
        hero.Self = new Stat(hero.Self, hero.Self.Health +
          Mathf.Ceil((hero.Self.MaxHealth - hero.Self.Health) * HEAL_AMNT));
        yield return new WaitForSeconds(HEAL_RATE);
      }
    }

    public void OnTriggerEnter(Collider other) {
      if (heal != null) { return; }

      Hero hero = (other.gameObject.TryGetComponent(out Player p) &&
        p.hero.Team == Team) ? p.hero :
        (other.gameObject.TryGetComponent(out NPC n) &&
        n.hero.Team == Team) ? n.hero : null;

      if (hero != null) {
        render.material = materials[1];
        heal = StartCoroutine(nameof(Heal), hero);
      }
    }

    public void OnTriggerExit(Collider other) {
      if (heal != null &&
        ((other.gameObject.TryGetComponent(out Player p) && p.hero.Team == Team) ||
        (other.gameObject.TryGetComponent(out NPC n) && n.hero.Team == Team))) {
        render.material = materials[0];
        StopCoroutine(nameof(Heal));
        heal = null;
      }
    }

    public IEnumerator Monitor() {
      throw new System.NotImplementedException();
    }

    public IEnumerator Spawn() {
      Transform[] spawns = new Transform[3];
      for (int i = 0, j = 0; i < transform.parent.childCount && j < 3; i++) {
        Transform child = transform.parent.GetChild(i);
        if (child.CompareTag("Respawn")) {
          spawns[j] = child;
          j++;
        }
      }
      while (true) {
        wave++;
        foreach (Transform spawn in spawns) {
          Minion m = Instantiate(prefab,
            spawn.position,
            spawn.rotation).GetComponent<Minion>();
          m.Team = Team;
          m.Self = new Stat(Minion.START_ACCEL,
            Minion.START_ATTACK,
            Minion.START_DAMAGE + (m.DamageGain * wave),
            Minion.START_HEALTH + (m.HealthGain * wave),
            Minion.START_MOVING);
          m.opposingShrine = opposingShrine;
        }
        Integrity += MAX_INT * HEAL_AMNT;
        yield return new WaitForSeconds(SpawnRate);
      }
    }
  }
}
