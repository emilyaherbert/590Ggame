using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HeroClash {
  internal class Minion : MonoBehaviour, ICharacter {
    internal const float START_ACCEL = 12.0f,
                          START_ATTACK = 0.5f,
                          START_DAMAGE = 10.0f,
                          START_HEALTH = 100.0f,
                          START_MOVING = 14.0f;

    private Coroutine triggerDeath;
    private NavMeshAgent nav;
    private API api;

    public float DamageGain => 5.0f;
    public float HealthGain => 10.0f;
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    public TEAM Team { get; set; }
    public GameObject opposingShrine { get; set; }

    private Vector3 opposingShrineDest;
    private bool activeTarget;

    public List<GameObject> enemiesWithinVision;
    public List<GameObject> enemiesWithinAttackRange;

    private Animator anim;
    private readonly int OTHER_ATCK_HASH = Animator.StringToHash("otherAtck"),
                          STATE_HASH = Animator.StringToHash("s");

    private void Start() {
      nav = GetComponent<NavMeshAgent>();
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
      opposingShrineDest = opposingShrine.transform.position;
      State = STATE.MOVE;
      Them = new Target();
      activeTarget = false;
      anim = GetComponent<Animator>();
      enemiesWithinVision = new List<GameObject>();
      enemiesWithinAttackRange = new List<GameObject>();
    }

    private void Update() {
      if (Self.Health < 0 && triggerDeath == null) {
        State = STATE.DEAD;
      }
      FSM();
    }

    public void OnTriggerStay(Collider c) {
      if (activeTarget) {
        return;
      }
      if (api.IsEnemy(c.gameObject, Team)) {
        if (c.gameObject.CompareTag("Character")) {
          if (c.gameObject.TryGetComponent(out Player p) && p.hero.Team != Team) {
            Them = new Target(c, p.hero);
          } else if (c.gameObject.TryGetComponent(out NPC n) && n.hero.Team != Team) {
            Them = new Target(c, n.hero);
          } else if (c.gameObject.TryGetComponent(out Minion m) && m.Team != Team) {
            Them = new Target(c, m);
          }
        } else {
          IStructure s = null;
          if(c.gameObject.GetComponentInChildren<Tower>()) {
            s = c.gameObject.GetComponentInChildren<Tower>();
          } else if(c.gameObject.GetComponentInChildren<Shrine>()) {
            s = c.gameObject.GetComponentInChildren<Shrine>();
          }
          if (s.Team != Team) {
            Them = new Target(c, s);
          }
        }
        State = STATE.ATCK;
        activeTarget = true;
      }
    }

    /*

    | state  | trigger condition       | next state |
    |--------|-------------------------|------------|
    | move   | nearby target           | attack     |
    | attack | target loses all health | move       |
    | attack | self loses all health   | dead       |
    | dead   | complete dying sequence | destroy    |

    */

    private void FSM() {
      switch (State) {
        case STATE.IDLE:
          break;
        case STATE.MOVE:
          anim.SetInteger(STATE_HASH, (int)State);
          nav.SetDestination(PickDest());
          break;
        case STATE.ATCK:
          anim.SetInteger(STATE_HASH, (int)State);
          StartCoroutine(Attack());
          break;
        case STATE.DEAD:
          anim.SetInteger(STATE_HASH, (int)State);
          activeTarget = false;
          nav.isStopped = true;
          StopCoroutine(nameof(Attack));
          triggerDeath = StartCoroutine(nameof(DyingSequence));
          break;
        case STATE.DESTROY:
          Destroy(gameObject);
          break;
      }
    }

    private int TypeToRanking(GameObject g) {
      if (api.IsEnemyMinion(g, Team)) {
        return 1;
      } else if (api.IsEnemyTower(g, Team)) {
        return 2;
      } else if (api.IsEnemyHero(g, Team)) {
        return 3;
      } else if (api.IsEnemyShrine(g, Team)) {
        return 4;
      } else {
        return -1;
      }
    }

    private Vector3 PickDest() {
      // The default movement is to move towards the opponents shrine
      Vector3 dest = opposingShrineDest;
      float dist = Vector3.Distance(transform.position, dest);
      int ranking = 0;
      foreach(GameObject enemy in enemiesWithinVision) {
        Vector3 newDest = enemy.transform.position;
        float newDist = Vector3.Distance(transform.position, enemy.transform.position);
        int newRanking = TypeToRanking(enemy.gameObject);
        if (newRanking > ranking) {
          dest = newDest;
          dist = newDist;
          ranking = newRanking;
        } else if (newRanking == ranking && newDist < dist) {
          dest = newDest;
          dist = newDist;
          ranking = newRanking;
        }
      }
      return dest;
    }

    public IEnumerator Attack() {
      State = STATE.IDLE;
      nav.isStopped = true;
      while ((Them.Character != null && Them.Character.Self.Health > 0) || (Them.Structure != null && Them.Structure.Integrity > 0)) {
        if (Them.Character != null) {
          Them.Character.Self = new Stat(Them.Character.Self, Them.Character.Self.Health - Self.Damage);
        } else if (Them.Structure != null) {
          Them.Structure.Integrity -= Self.Damage;
        }
        yield return new WaitForSeconds(Self.AtckSpeed);
      }
      State = STATE.MOVE;
      nav.isStopped = false;
      activeTarget = false;
    }

    private IEnumerator DyingSequence() {
      yield return new WaitForSeconds(5.0f);
      transform.position = new Vector3(100000.0f, 100000.0f, 1000000.0f);
      yield return new WaitForSeconds(1.0f);
      State = STATE.DESTROY;
    }
  }
}
