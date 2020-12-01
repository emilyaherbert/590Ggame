using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class Tower : MonoBehaviour, IStructure {
    [SerializeField] private float damage = default;
    [SerializeReference] private GameObject pPrefab = default;
    private Renderer render;
    private Target target;

    [field: SerializeField] public float Integrity { get; set; }
    public float SpawnRate => 1.0f;
    [field: SerializeField] public TEAM Team { get; set; }

    private void OnTriggerStay(Collider other) {
      OnTriggerEnter(other);
    }

    private void SetTarget(Collider c, ICharacter ch) {
      target = new Target(c, ch);
      render.material.color = Color.red;
      _ = StartCoroutine(nameof(Spawn));
    }

    private void Start() {
      render = transform.parent.gameObject.GetComponent<Renderer>();
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
        render.material.color = Color.white;
        StopCoroutine(nameof(Spawn));
        target = new Target();
      }
    }

    public IEnumerator Spawn() {
      while (true) {
        GameObject pInstance = Instantiate(pPrefab,
          transform.position + Vector3.up,
          Quaternion.identity);
        pInstance.SetActive(true);
        TowerProjectile tp = pInstance.GetComponent<TowerProjectile>();
        tp.target = target;
        tp.damage = damage;
        yield return new WaitForSeconds(SpawnRate);
      }
    }
  }
}
