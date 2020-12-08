using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HeroClash {
  internal class Minion : MonoBehaviour, ICharacter {
    internal const float START_ACCEL = 10.0f,
                          START_ATTACK = 0.5f,
                          START_DAMAGE = 10.0f,
                          START_HEALTH = 100.0f,
                          START_MOVING = 20.0f;

    private NavMeshAgent nav;
    private int visionRadius = 100;

    public float DamageGain => 5.0f;
    public float HealthGain => 10.0f;
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    public TEAM Team { get; set; }
    public GameObject opposingShrine {get; set; }

    private Vector3 opposingShrineDest;
    private List<GameObject> targets;

    private void Start() {
      nav = GetComponent<NavMeshAgent>();
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
      opposingShrineDest = opposingShrine.transform.position;
      State = STATE.MOVE;
      targets = new List<GameObject>();
    }

    private void Update() {
      if(Self.Health < 0) {
        State = STATE.DEAD;
      }
      
      FSM();
    }

    public void OnTriggerEnter(Collider other) {
      if(IsEnemy(other.gameObject)) {
        Debug.Log(other.gameObject);
        targets.Add(other.gameObject);
        State = STATE.ATCK;
      }
    }

    public void OnTriggerExit(Collider other) {
      if(IsEnemy(other.gameObject)) {
        targets.Remove(other.gameObject);
      }
      if(targets.Count == 0) {
        State = STATE.IDLE;
      }
    }

    private void FSM() {
      switch (State) {
        case STATE.IDLE:
          State = STATE.MOVE;
          break;
        case STATE.MOVE:
          nav.destination = PickDest();
          break;
        case STATE.ATCK:
          MinionAttack();
          break;
        case STATE.DEAD:
          new WaitForSeconds(3.0f);
          break;
      }
    }

    private Vector3 PickDest() {
      Vector3 dest = opposingShrineDest;
      float dist = Vector3.Distance(transform.position, dest);
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(IsEnemyMinion(c.gameObject)) {
          float newDist = Vector3.Distance(transform.position, c.transform.position);
          if(newDist < dist) {
            dest = c.transform.position;
            dist = newDist;
          }
        } else if(IsEnemyHero(c.gameObject)) {
          return c.transform.position;
        }
      }
      return dest;
    }

    private bool IsEnemy(GameObject g) {
      return IsEnemyHero(g) || IsEnemyMinion(g);
    }

    private bool IsEnemyMinion(GameObject g) {
      return g.GetComponent<Minion>() && g.GetComponent<Minion>().Team != Team;
    }

    private bool IsEnemyHero(GameObject g) {
      if(g.GetComponent<HeroGolem>() && g.GetComponent<HeroGolem>().Team != Team) {
        return true;
      } else if(g.GetComponent<HeroGrunt>() && g.GetComponent<HeroGrunt>().Team != Team) {
        return true;
      }
      return false;
    }

    private void MinionAttack() {

    }

    public IEnumerator Attack() {
      yield return new WaitForSeconds(10000);
    }
  }
}
