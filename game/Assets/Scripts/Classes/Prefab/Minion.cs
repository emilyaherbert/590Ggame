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
    public List<(Collider, GameObject)> enemiesWithinAttackRange;

    private Animator anim;
    private readonly int OTHER_ATCK_HASH = Animator.StringToHash("otherAtck"),
                          STATE_HASH = Animator.StringToHash("s");

    private void Awake() {
      enemiesWithinVision = new List<GameObject>();
      enemiesWithinAttackRange = new List<(Collider, GameObject)>();
    }

    private void Start() {
      nav = GetComponent<NavMeshAgent>();
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
      opposingShrineDest = opposingShrine.transform.position;
      State = STATE.MOVE;
      Them = new Target();
      activeTarget = false;
      anim = GetComponent<Animator>();
    }

    private void Update() {
      enemiesWithinVision = RemoveNulls(enemiesWithinVision);
      enemiesWithinAttackRange = RemoveNulls2(enemiesWithinAttackRange);
      if (Self.Health < 0 && triggerDeath == null) {
        State = STATE.DEAD;
      }
      FSM();
    }

    public void OnTriggerEnter(Collider c) {
      GameObject obj = c.gameObject;
      if(api.IsEnemy(obj, Team) && !enemiesWithinAttackRange.Contains((c, obj))) {
        enemiesWithinAttackRange.Add((c, obj));
      }
    }

    public void OnTriggerExit(Collider c) {
      GameObject obj = c.gameObject;
      if(api.IsEnemy(obj, Team) && enemiesWithinAttackRange.Contains((c, obj))) {
        enemiesWithinAttackRange.Remove((c, obj));
      }
    }

    private List<GameObject> RemoveNulls(List<GameObject> prev) {
      List<GameObject> ret = new List<GameObject>();
      foreach(var obj in prev) {
        if(obj != null) {
          ret.Add(obj);
        }
      }
      return ret;
    }

    private List<(Collider, GameObject)> RemoveNulls2(List<(Collider, GameObject)> prev) {
      List<(Collider, GameObject)> ret = new List<(Collider, GameObject)>();
      foreach(var obj in prev) {
        if(obj.Item2 != null) {
          ret.Add(obj);
        }
      }
      return ret;
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
          if(enemiesWithinAttackRange.Count > 0) {
            State = STATE.ATCK;
            activeTarget = true;
          } else {
            nav.SetDestination(PickDest());
          }
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

    private GameObject ClosestWorstEnemy() {
      GameObject target = opposingShrine;
      float dist = Vector3.Distance(transform.position, target.transform.position);
      int ranking = 0;
      foreach(GameObject enemy in enemiesWithinVision) {
        Vector3 newDest = enemy.transform.position;
        float newDist = Vector3.Distance(transform.position, enemy.transform.position);
        int newRanking = api.TypeToRanking(enemy.gameObject, Team);
        if (newRanking > ranking) {
          target = enemy;
          dist = newDist;
          ranking = newRanking;
        } else if (newRanking == ranking && newDist < dist) {
          target = enemy;
          dist = newDist;
          ranking = newRanking;
        }
      }
      return target;
    }

    private Vector3 PickDest() {
      return ClosestWorstEnemy().transform.position;
    }

    private void SetThem((Collider, GameObject) e) {
      GameObject obj = e.Item2;
      Collider c = e.Item1;
      if (obj.CompareTag("Character")) {
        if (obj.TryGetComponent(out Player p) && p.hero.Team != Team) {
          Them = new Target(c, p.hero);
        } else if (obj.TryGetComponent(out NPC n) && n.hero.Team != Team) {
          Them = new Target(c, n.hero);
        } else if (obj.TryGetComponent(out Minion m) && m.Team != Team) {
          Them = new Target(c, m);
        }
      } else {
        IStructure s = null;
        if(obj.GetComponentInChildren<Tower>()) {
          s = obj.GetComponentInChildren<Tower>();
        } else if(obj.GetComponentInChildren<Shrine>()) {
          s = obj.GetComponentInChildren<Shrine>();
        }
        if (s.Team != Team) {
          Them = new Target(c, s);
        }
      }
    }

    public IEnumerator Attack() {
      State = STATE.IDLE;
      nav.isStopped = true;
      (Collider, GameObject) target = enemiesWithinAttackRange[0];
      SetThem(target);
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
      enemiesWithinAttackRange.Remove(target);
      enemiesWithinVision.Remove(target.Item2);
    }

    private IEnumerator DyingSequence() {
      yield return new WaitForSeconds(5.0f);
      transform.position = new Vector3(100000.0f, 100000.0f, 1000000.0f);
      yield return new WaitForSeconds(1.0f);
      State = STATE.DESTROY;
    }
  }
}
