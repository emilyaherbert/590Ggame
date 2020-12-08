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
    private int visionRadius = 50;

    public float DamageGain => 5.0f;
    public float HealthGain => 10.0f;
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    public TEAM Team { get; set; }
    public GameObject opposingShrine {get; set; }

    private Vector3 opposingShrineDest;
    private bool activeTarget;

    private void Start() {
      nav = GetComponent<NavMeshAgent>();
      nav.speed = Self.MoveSpeed;
      nav.acceleration = Self.Accelerate;
      opposingShrineDest = opposingShrine.transform.position;
      State = STATE.MOVE;
      Them = new Target();
      activeTarget = false;
    }

    private void Update() {
      if(Self.Health < 0) {
        State = STATE.DEAD;
      }
      FSM();
    }

    public void OnTriggerStay(Collider c) {
      if(activeTarget) {
        return;
      }
      if(IsEnemy(c.gameObject)) {
        if (c.gameObject.CompareTag("Character")) {
          if (c.gameObject.TryGetComponent(out Player p) && p.hero.Team != Team) {
            Them = new Target(c, p.hero);
          } else if (c.gameObject.TryGetComponent(out NPC n) && n.hero.Team != Team) {
            Them = new Target(c, n.hero);
          } else if (c.gameObject.TryGetComponent(out Minion m) && m.Team != Team) {
            Them = new Target(c, m);
          }
        } else {
          IStructure s = c.gameObject.GetComponentInChildren<Tower>();
          if (s == null) {
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
          nav.destination = PickDest();
          break;
        case STATE.ATCK:
          StartCoroutine(Attack());
          break;
        case STATE.DEAD:
          StopCoroutine(nameof(Attack));
          activeTarget = false;
          StartCoroutine(DyingSequence());
          break;
        case STATE.DESTROY:
          Destroy(gameObject);
          break;
      }
    }

    private int TypeToRanking(GameObject g) {
      if(IsEnemyMinion(g)) {
        return 1;
      } else if(IsEnemyTower(g)) {
        return 2;
      } else if(IsEnemyHero(g)) {
        return 3;
      } else if(IsEnemyShrine(g)) {
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
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(IsEnemy(c.gameObject)) {
          Vector3 newDest = c.transform.position;
          float newDist = Vector3.Distance(transform.position, c.transform.position);
          int newRanking = TypeToRanking(c.gameObject);
          if(newRanking > ranking) {
            dest = newDest;
            dist = newDist;
            ranking = newRanking;
          } else if(newRanking == ranking && newDist < dist) {
            dest = newDest;
            dist = newDist;
            ranking = newRanking;
          }
        }
      }
      return dest;
    }

    private bool IsEnemy(GameObject g) {
      return IsEnemyHero(g) || IsEnemyMinion(g) || IsEnemyShrine(g) || IsEnemyTower(g);
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

    private bool IsEnemyTower(GameObject g) {
      return g.GetComponentInChildren<Tower>() && g.GetComponentInChildren<Tower>().Team != Team;
    }

    private bool IsEnemyShrine(GameObject g) {
      return g.GetComponentInChildren<Shrine>() && g.GetComponentInChildren<Shrine>().Team != Team;
    }

    public IEnumerator Attack() {
      State = STATE.IDLE;
      while ((Them.Character != null && Them.Character.Self.Health > 0) || (Them.Structure != null && Them.Structure.Integrity > 0)) {
        if (Them.Character != null) {
          Them.Character.Self = new Stat(Them.Character.Self, Them.Character.Self.Health - Self.Damage);
        } else if (Them.Structure != null) {
          Them.Structure.Integrity -= Self.Damage;
        }
        yield return new WaitForSeconds(Self.AtckSpeed);
      }
      State = STATE.MOVE;
      activeTarget = false;
    }

    private IEnumerator DyingSequence() {
      State = STATE.IDLE;
      nav.destination = transform.position;
      yield return new WaitForSeconds(5.0f);
      transform.position = new Vector3(100000.0f, 100000.0f, 100000.0f);
      State = STATE.DESTROY;
    }
  }
}
