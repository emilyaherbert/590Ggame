using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace HeroClash {
  internal class Minion : MonoBehaviour, ICharacter {
    internal const float START_ACCEL = 10.0f,
                          START_ATTACK = 0.5f,
                          START_DAMAGE = 10.0f,
                          START_HEALTH = 100.0f,
                          START_MOVING = 10.0f;

    private NavMeshAgent nav;

    public float DamageGain => 5.0f;
    public float HealthGain => 10.0f;
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    public TEAM Team { get; set; }
    public GameObject opposingShrine {get; set; }

    private void Start() {
      nav = GetComponent<NavMeshAgent>();
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
    }

    private void Update() {
      Debug.Log(opposingShrine);
      Vector3 newDest = new Vector3(opposingShrine.transform.position.x, opposingShrine.transform.position.y, opposingShrine.transform.position.z);
      nav.destination = newDest;
    }

    public IEnumerator Attack() {
      throw new System.NotImplementedException();
    }
  }
}
