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
    public List<GameObject> enemiesWithinAttackRange;
    public TEAM Team;

    private Node decisionTree;
    private API api;

    private void Awake() {
      api = new API();
      enemiesWithinVision = new List<GameObject>();
      enemiesWithinAttackRange = new List<GameObject>();
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
      ClassifyDT(decisionTree);
    }

    public void OnTriggerStay(Collider c) {
      if(activeTarget) {
        return;
      }
      if(api.IsEnemy(c.gameObject, hero.Team)) {
        if (c.gameObject.CompareTag("Character")) {
          if (c.gameObject.TryGetComponent(out Player p) && p.hero.Team != hero.Team) {
            hero.Them = new Target(c, p.hero);
          } else if (c.gameObject.TryGetComponent(out NPC n) && n.hero.Team != hero.Team) {
            hero.Them = new Target(c, n.hero);
          } else if (c.gameObject.TryGetComponent(out Minion m) && m.Team != hero.Team) {
            hero.Them = new Target(c, m);
          }
        } else {
          IStructure s = c.gameObject.GetComponentInChildren<Tower>();
          if (s == null) {
            s = c.gameObject.GetComponentInChildren<Shrine>();
          }
          if (s.Team != hero.Team) {
            hero.Them = new Target(c, s);
          }
        }
        activeTarget = true;
        attackStarted = false;
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

    public List<GameObject> NearbyEnemyCharacters() {
      List<GameObject> nearby = new List<GameObject>();
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(api.IsEnemyCharacter(c.gameObject, hero.Team)) {
          nearby.Add(c.gameObject);
        }
      }
      return nearby;
    }

    public List<GameObject> NearbyMyTowers() {
      List<GameObject> nearby = new List<GameObject>();
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(api.IsMyTower(c.gameObject, hero.Team)) {
          nearby.Add(c.gameObject);
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
      hero.Them = new Target();
      StopCoroutine(nameof(hero.Attack));
    }

    public IEnumerator NPCAttack() {
      attackStarted = true;
      yield return hero.Attack();
      attackStarted = false;
      activeTarget = false;
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

  class MyTowerWithinVision : Node {
    public MyTowerWithinVision(Node l, Node r) {
      nodeType = NODE_TYPE.SPLIT;
      left = l;
      right = r;
    }

    public override bool F(NPC npc) {
      List<GameObject> nearby = npc.NearbyMyTowers();
      return nearby.Count > 0;
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
      List<GameObject> nearby = npc.NearbyMyTowers();
      GameObject closest = null;
      float dist = 99999999.0f;
      foreach(GameObject tower in nearby) {
        float newDist = Vector3.Distance(npc.gameObject.transform.position, tower.transform.position);
        if(newDist < dist) {
          closest = tower;
          dist = newDist;
        }
      }
      npc.hero.Move(closest.transform.position);
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

  class MoveToWorstClosestEnemy : Node {
    public MoveToWorstClosestEnemy() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      API api = new API();
      Vector3 npcPos = npc.gameObject.transform.position;
      Vector3 dest = npcPos;
      float dist = Vector3.Distance(npcPos, dest);
      int ranking = 0;
      foreach(GameObject enemy in npc.enemiesWithinVision) {
        Vector3 newDest = enemy.transform.position;
        float newDist = Vector3.Distance(npcPos, newDest);
        int newRanking = api.TypeToRanking(enemy.gameObject, npc.Team);
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

  class Pass : Node {
    public Pass() {
      nodeType = NODE_TYPE.LEAF;
    }

    public override bool F(NPC npc) {
      return true;
    }
  }
}
