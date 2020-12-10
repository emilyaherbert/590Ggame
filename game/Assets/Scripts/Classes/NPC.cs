using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HeroClash {
  internal class NPC : MonoBehaviour {
    internal Hero hero;

    public int visionRadius = 100;
    public GameObject myShrine;
    public GameObject opponentsShrine;
    public bool activeTarget;
    public bool attackStarted;
    public Vector3 opponentsShrineDest;

    public List<GameObject> enemiesWithinVision;
    public List<(Collider, GameObject)> enemiesWithinAttackRange;
    public List<GameObject> myTowersWithinVision;
    public List<GameObject> myTowersHistory;
    public TEAM Team;

    private Node decisionTree;
    private API api;

    private void Awake() {
      api = new API();
      enemiesWithinVision = new List<GameObject>();
      enemiesWithinAttackRange = new List<(Collider, GameObject)>();
      myTowersWithinVision = new List<GameObject>();
    }

    private void Start() {
      GameObject[] objs = GameObject.FindGameObjectsWithTag("Structure");
      foreach(GameObject o in objs) {
        if(o.GetComponentInChildren<Shrine>() && o.GetComponentInChildren<Shrine>().Team == hero.Team) {
          myShrine = o;
        } else if(o.GetComponentInChildren<Shrine>() && o.GetComponentInChildren<Shrine>().Team != hero.Team) {
          opponentsShrine = o;
        }
      }

      decisionTree = CreateDecisionTree();
      activeTarget = false;
      attackStarted = false;
      Team = hero.Team;
      opponentsShrineDest = opponentsShrine.GetComponent<Collider>().ClosestPoint(transform.position);
    }

    private void Update() {
      enemiesWithinVision = FilterOutTowers(RemoveNulls(enemiesWithinVision));
      enemiesWithinAttackRange = FilterOutTowers2(RemoveNulls2(enemiesWithinAttackRange));
      myTowersWithinVision = RemoveNulls(myTowersWithinVision);
      ClassifyDT(decisionTree);
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

    private Node CreateDecisionTree() {
      return new HealthMeetsMinimumPercentage(0.25,
        new HaveActiveAttackTarget(
          new Attack(),
          new EnemiesWithinVision(
            new MoveToClosestEnemy(),
            new MoveToOpponentsShrine()
          )
        ),
        new MoveToMyShrine());
    }

    private Node CreateDecisionTree2() {
      return new HealthMeetsMinimumPercentage(0.25,
        new MoreThanNEnemiesWithinVision(1,
          new MoreThanNEnemiesWithinVision(3,
            new MyTowerWithinVision(
              new Attack(),
              new MoveToClosestMyTower()
            ),
            new Attack()
          ),
          new MoveToOpponentsShrine()
        ),
        new MoveToMyShrine()
      );
    }

    private void ClassifyDT(Node n) {
      NPC npc = gameObject.GetComponent<NPC>();
      switch(n.nodeType) {
        case NODE_TYPE.SPLIT:
          if(n.F(npc)) {
            ClassifyDT(n.left);
          } else {
            ClassifyDT(n.right);
          }
          break;
        case NODE_TYPE.LEAF:
          n.F(npc);
          break;
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

    private List<GameObject> FilterOutTowers(List<GameObject> prev) {
      List<GameObject> ret = new List<GameObject>();
      foreach(GameObject obj in prev) {
        if(!api.IsEnemyTower(obj, Team)) {
          ret.Add(obj);
        }
      }
      return ret;
    }

    private List<(Collider, GameObject)> FilterOutTowers2(List<(Collider, GameObject)> prev) {
      List<(Collider, GameObject)> ret = new List<(Collider, GameObject)>();
      foreach((Collider, GameObject) obj in prev) {
        if(!api.IsEnemyTower(obj.Item2, Team)) {
          ret.Add(obj);
        }
      }
      return ret;
    }

    public List<GameObject> NearbyEnemyCharacters() {
      List<GameObject> nearby = new List<GameObject>();
      foreach(GameObject obj in enemiesWithinVision) {
        if(api.IsEnemyCharacter(obj, hero.Team)) {
          nearby.Add(obj);
        }
      }
      return nearby;
    }

    public void Attack() {
      StartCoroutine(NPCAttack());
    }

    public void StopAttacking() {
      attackStarted = false;
      activeTarget = false;
      StopCoroutine(nameof(hero.Attack));
    }

    public IEnumerator NPCAttack() {
      attackStarted = true;
      (Collider, GameObject) target = enemiesWithinAttackRange[0];
      SetThem(target);
      yield return hero.Attack();
      attackStarted = false;
      activeTarget = false;
      enemiesWithinAttackRange.Remove(target);
      enemiesWithinVision.Remove(target.Item2);
    }

    private void SetThem((Collider, GameObject) e) {
      GameObject obj = e.Item2;
      Collider c = e.Item1;
      if (obj.CompareTag("Character")) {
        if (obj.TryGetComponent(out Player p) && p.hero.Team != Team) {
          hero.Them = new Target(c, p.hero);
        } else if (obj.TryGetComponent(out NPC n) && n.hero.Team != Team) {
          hero.Them = new Target(c, n.hero);
        } else if (obj.TryGetComponent(out Minion m) && m.Team != Team) {
          hero.Them = new Target(c, m);
        }
      } else {
        IStructure s = null;
        if(obj.GetComponentInChildren<Tower>()) {
          s = obj.GetComponentInChildren<Tower>();
        } else if(obj.GetComponentInChildren<Shrine>()) {
          s = obj.GetComponentInChildren<Shrine>();
        }
        if (s.Team != Team) {
          hero.Them = new Target(c, s);
        }
      }
    }
  }

  public enum NODE_TYPE {
    LEAF,
    SPLIT
  }

  abstract class Node {
    public NODE_TYPE nodeType;
    public Node left;
    public Node right;
    public abstract bool F(NPC npc);
  }

  class HealthMeetsMinimumPercentage : Node {
    double percentage;

    public HealthMeetsMinimumPercentage(double p, Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      percentage = p;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      return (npc.hero.Self.Health/npc.hero.Self.MaxHealth) >= percentage;
    }
  }

  class HaveActiveAttackTarget : Node {
    public HaveActiveAttackTarget(Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      return npc.activeTarget;
    }
  }

  class AttackStarted : Node {
    public AttackStarted(Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      return npc.attackStarted;
    }
  }

  class EnemiesWithinVision : Node {
    public EnemiesWithinVision(Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      return npc.enemiesWithinVision.Count > 0;
    }
  }

  class EnemyCharacterWithinVision : Node {
    public EnemyCharacterWithinVision(Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      List<GameObject> nearby = npc.NearbyEnemyCharacters();
      return nearby.Count > 0;
    }
  }

  class MoreThanNEnemiesWithinVision : Node {
    int number;

    public MoreThanNEnemiesWithinVision(int n, Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      number = n;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      return npc.enemiesWithinVision.Count > number;
    }
  }

  class MyTowerWithinVision : Node {
    public MyTowerWithinVision(Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      return npc.myTowersWithinVision.Count > 0;
    }
  }

  class MoveToMyShrine : Node {
    public MoveToMyShrine() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      npc.hero.Move(npc.myShrine.transform.position);
      return true;
    }
  }

  class MoveToOpponentsShrine : Node {
    public MoveToOpponentsShrine() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      npc.hero.Move(npc.opponentsShrineDest);
      return true;
    }
  }

  class MoveToClosestMyTower : Node {
    public MoveToClosestMyTower() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      GameObject closest = null;
      float dist = 99999999.0f;
      foreach(GameObject tower in npc.myTowersHistory) {
        float newDist = Vector3.Distance(npc.gameObject.transform.position, tower.transform.position);
        if(newDist < dist) {
          closest = tower;
          dist = newDist;
        }
      }
      Vector3 dest = closest.GetComponent<Collider>().ClosestPoint(npc.transform.position);
      npc.hero.Move(dest);
      return true;
    }
  }

  class MoveToClosestEnemy : Node {
    public MoveToClosestEnemy() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      Vector3 npcPos = npc.gameObject.transform.position;
      Vector3 dest = npc.opponentsShrine.transform.position;
      float dist = Vector3.Distance(npcPos, dest);
      foreach(GameObject enemy in npc.enemiesWithinVision) {
        Vector3 newDest = enemy.transform.position;
        float newDist = Vector3.Distance(npcPos, newDest);
        if(newDist < dist) {
          dest = newDest;
          dist = newDist;
        }
      }
      npc.hero.Move(dest);
      return true;
    }
  }

  class Attack : Node {
    public Attack() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      if(npc.hero.Them.Character == null && npc.hero.Them.Structure == null) {
        npc.StopAttacking();
      } else if(!npc.attackStarted) {
        npc.Attack();
      }
      return true;
    }
  }
}
