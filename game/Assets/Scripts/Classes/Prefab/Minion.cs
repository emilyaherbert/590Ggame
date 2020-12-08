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
                          START_MOVING = 10.0f;

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

    private void Start() {
      nav = GetComponent<NavMeshAgent>();
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
      opposingShrineDest = opposingShrine.transform.position;
      State = STATE.MOVE;
    }

    private void Update() {
      FSM();
    }

    private void OnCollisionEnter(Collision collision) {
      foreach (ContactPoint contact in collision.contacts) {
        Debug.Log(collision);
      }
      //if (collision.relativeVelocity.magnitude > 2)
          //audioSource.Play();
    }

    private void FSM() {
      switch (State) {
        case STATE.IDLE:
          break;
        case STATE.MOVE:
          nav.destination = PickDest();
          break;
        case STATE.ATCK:
          break;
        case STATE.DEAD:
          break;
      }
    }

    private Vector3 PickDest() {
      Vector3 dest = opposingShrineDest;
      float dist = Vector3.Distance(transform.position, dest);
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(c.GetComponent<Minion>() && c.GetComponent<Minion>().Team != Team) {
          float newDist = Vector3.Distance(transform.position, c.transform.position);
          if(newDist < dist) {
            dest = c.transform.position;
            dist = newDist;
          }
        } else if(c.GetComponent<HeroGolem>() && c.GetComponent<HeroGolem>().Team != Team) {
          return c.transform.position;
        } else if(c.GetComponent<HeroGrunt>() && c.GetComponent<HeroGrunt>().Team != Team) {
          return c.transform.position;
        }
      }
      return dest;
    }

    private void MoveTowardsShrine() {
      nav.destination = opposingShrineDest;
    }

    public IEnumerator Attack() {
      throw new System.NotImplementedException();
    }
  }
}
