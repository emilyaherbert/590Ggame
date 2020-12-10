using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace HeroClash {
  internal abstract class Hero : MonoBehaviour, ICharacter {
    private const float ATTACK_EPSILON = 2.0f * Mathf.PI,
                        FIX_Y = 0.1f,
                        XP_MULT_LAST_HIT = 10.0f,
                        XP_MULT_STRUCT = 2.0f,
                        XP_RATE = 2.0f,
                        XP_PER_LVL = 1000.0f,
                        XP_TIME = 1.0f;
    private readonly int OTHER_ATCK_HASH = Animator.StringToHash("otherAtck"),
                          STATE_HASH = Animator.StringToHash("s");

    private float xp;
    private Coroutine atck;

    protected Animator anim;
    protected NavMeshAgent nav;

    protected abstract float AccelGain { get; }
    protected abstract float AttackLoss { get; }
    protected abstract float MovingGain { get; }

    public abstract float DamageGain { get; }
    public abstract float HealthGain { get; }

    internal int Level => (int)(XP / XP_PER_LVL);
    internal float XP {
      get => xp;
      set {
        int oldLvl = Level;
        xp += value;
        if (oldLvl < Level) {
          Self = new Stat(Self.Accelerate + AccelGain,
            Self.AtckSpeed - AttackLoss,
            Self.MaxDamage + DamageGain,
            Self.MaxHealth + HealthGain,
            Self.MoveSpeed + MovingGain,
            Self.Health / Self.MaxHealth * (Self.MaxHealth + HealthGain));
          nav.speed = Self.MoveSpeed;
          nav.acceleration = Self.Accelerate;
        }
      }
    }
    public Stat Self { get; set; }
    public Target Them { get; set; }
    public STATE State { get; set; }
    [field: SerializeField] public TEAM Team { get; set; }

    private bool NoMove() {
      Vector3 pt = Them.Box.ClosestPoint(transform.position);
      pt = new Vector3(pt.x, FIX_Y, pt.z);
      if (Vector3.Distance(transform.position, pt) > ATTACK_EPSILON) {
        nav.SetDestination(pt);
        Move(false);
        return false;
      }
      return true;
    }

    private void Move(bool isOnlyMove) {
      if (isOnlyMove) {
        Them = new Target();
      }

      StopCoroutine(nameof(Attack));
      atck = null;
      State = STATE.MOVE;
      anim.SetInteger(STATE_HASH, (int)State);
    }

    private void SetTarget(Collider c, ICharacter ch, IStructure st) {
      Them = st == null ? new Target(c, ch) : new Target(c, st);
      State = STATE.ATCK;
    }

    private void Update() {
      if (GameManager.paused) { return; }
      if (State == STATE.MOVE &&
        !nav.pathPending &&
        nav.remainingDistance < nav.stoppingDistance &&
        (!nav.hasPath || Mathf.Approximately(nav.velocity.sqrMagnitude, 0))) {
        State = Them.Box == null ? STATE.IDLE : STATE.ATCK;
        anim.SetInteger(STATE_HASH, (int)STATE.IDLE);
      } else if (State == STATE.ATCK && atck == null && NoMove()) {
        atck = StartCoroutine(nameof(Attack));
      }

      if (Self.Health < 0 && State != STATE.DEAD) {
        StopCoroutine(nameof(Attack));
        nav.isStopped = true;
        Them = new Target();
        atck = null;
        State = STATE.DEAD;
        anim.SetInteger(STATE_HASH, (int)State);
      }
    }

    protected IEnumerator XPGain() {
      while (State != STATE.DEAD) {
        XP = XP_RATE;
        yield return new WaitForSeconds(XP_TIME);
      }
    }

    internal void ToAttack(Collider c) {
      if (c.gameObject.CompareTag("Character")) {
        if (c.gameObject.TryGetComponent(out Player p) && p.hero.Team != Team) {
          SetTarget(c, p.hero, null);
        } else if (c.gameObject.TryGetComponent(out NPC n) && n.hero.Team != Team) {
          SetTarget(c, n.hero, null);
        } else if (c.gameObject.TryGetComponent(out Minion m) && m.Team != Team) {
          SetTarget(c, m, null);
        }
      } else {
        IStructure s = c.gameObject.GetComponentInChildren<Tower>();
        if (s == null) {
          s = c.gameObject.GetComponentInChildren<Shrine>();
        }
        if (s.Team != Team) {
          SetTarget(c, null, s);
        }
      }
    }

    public void Move(Vector3 loc) {
      nav.SetDestination(new Vector3(loc.x, FIX_Y, loc.z));
      Move(true);
    }

    public IEnumerator Attack() {
      anim.SetInteger(STATE_HASH, (int)State);
      while ((Them.Character != null && Them.Character.Self.Health > 0) ||
              (Them.Structure != null && Them.Structure.Integrity > 0)) {
        if (Them.Character != null && NoMove()) {
          Them.Character.Self = new Stat(Them.Character.Self,
            Them.Character.Self.Health - Self.Damage);
          XP = (Level + 1) * XP_RATE;
        } else if (Them.Structure != null) {
          Them.Structure.Integrity -= Self.Damage;
          XP = (Level + 1) * (XP_MULT_STRUCT * XP_RATE);
        }
        transform.LookAt(Them.Box.transform);
        anim.SetBool(OTHER_ATCK_HASH, Random.value < 0.5f);
        yield return new WaitForSeconds(Self.AtckSpeed);
      }
      XP = (Level + 1) * (XP_MULT_LAST_HIT * XP_RATE);
      Them = new Target();
      atck = null;
      State = STATE.IDLE;
      anim.SetInteger(STATE_HASH, (int)State);
    }
  }
}
