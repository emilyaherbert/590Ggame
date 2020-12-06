using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace HeroClash {
  internal abstract class Hero : MonoBehaviour, ICharacter {
    private const float ATTACK_EPSILON = 0.1f,
                        FIX_Y = 0.1f,
                        XP_PER_LVL = 1000.0f;

    private float xp;
    private Coroutine atck;

    protected NavMeshAgent nav;

    protected abstract float AccelGain { get; }
    protected abstract float AttackLoss { get; }
    protected abstract float DamageGain { get; }
    protected abstract float HealthGain { get; }
    protected abstract float MovingGain { get; }

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
    public TEAM Team { get; set; }

    private bool NoMove() {
      Vector3 pt = Them.Box.ClosestPoint(transform.position);
      pt = new Vector3(pt.x, FIX_Y, pt.z);
      if (Vector3.Distance(transform.position, pt) > ATTACK_EPSILON) {
        nav.destination = pt;
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
      } else if (State == STATE.ATCK && atck == null && NoMove()) {
        atck = StartCoroutine(nameof(Attack));
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
      nav.destination = new Vector3(loc.x, FIX_Y, loc.z);
      Move(true);
    }

    public IEnumerator Attack() {
      while ((Them.Character != null && Them.Character.Self.Health > 0) ||
              (Them.Structure != null && Them.Structure.Integrity > 0)) {
        if (Them.Character != null && NoMove()) {
          Them.Character.Self = new Stat(Them.Character.Self,
            Them.Character.Self.Health - Self.Damage);
        } else if (Them.Structure != null) {
          Them.Structure.Integrity -= Self.Damage;
        }
        yield return new WaitForSeconds(Self.AtckSpeed);
      }
      atck = null;
      State = STATE.IDLE;
    }
  }
}
