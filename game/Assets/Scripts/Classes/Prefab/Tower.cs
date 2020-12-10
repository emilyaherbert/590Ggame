using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class Tower : MonoBehaviour, IStructure {

    [SerializeField] private float damage = default;
    private Renderer render;
    private Target target;

    [SerializeReference] private GameObject[] prefabs = new GameObject[2];
    [SerializeReference] private Material[] materials = new Material[2];

    [field: SerializeField] public float Integrity { get; set; }
    public float SpawnRate => 2.0f;
    [field: SerializeField] public TEAM Team { get; set; }

    private void OnTriggerStay(Collider other) {
      OnTriggerEnter(other);
    }

    private void SetTarget(Collider c, ICharacter ch) {
      target = new Target(c, ch);
      render.material = materials[1];
      _ = StartCoroutine(nameof(Spawn));
    }

    private void Start() {
      render = transform.parent.gameObject.GetComponent<Renderer>();
      _ = StartCoroutine(nameof(Monitor));
    }

    public void OnTriggerEnter(Collider other) {
      if (target.Box == null) {
        if (other.gameObject.TryGetComponent(out Player p) && p.hero.Team != Team) {
          SetTarget(other, p.hero);
        } else if (other.gameObject.TryGetComponent(out NPC n) && n.hero.Team != Team) {
          SetTarget(other, n.hero);
        } else if (other.gameObject.TryGetComponent(out Minion m) && m.Team != Team) {
          SetTarget(other, m);
        }
      }
    }

    public void OnTriggerExit(Collider other) {
      if (other == target.Box) {
        render.material = materials[0];
        StopCoroutine(nameof(Spawn));
        target = new Target();
      }
    }

    public IEnumerator Monitor() {
      while (Integrity > 0) {
        yield return null;
      }
      StopCoroutine(nameof(Spawn));
      target = new Target();
      render.material = materials[0];
      Destroy(transform.parent.gameObject);
    }

    public IEnumerator Spawn() {
      while (true && target.Box != null) {
        TowerProjectile tp = Instantiate(Random.value < 0.5f ? prefabs[0] : prefabs[1],
          transform.position, transform.rotation).GetComponent<TowerProjectile>();
        tp.target = target;
        tp.damage = damage;
        yield return new WaitForSeconds(SpawnRate);
      }
      render.material = materials[0];
    }
  }
}
