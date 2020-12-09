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

    private Node decisionTree;

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
    }

    private void Update() {
      //ClassifyDT(decisionTree);
    }

    public void OnTriggerStay(Collider c) {
      if(activeTarget) {
        return;
      }
      if(IsEnemy(c.gameObject)) {
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

    private bool IsEnemy(GameObject g) {
      return IsEnemyHero(g) || IsEnemyMinion(g) || IsEnemyShrine(g) || IsEnemyTower(g);
    }

    private bool IsEnemyCharacter(GameObject g) {
      return IsEnemyHero(g) || IsEnemyMinion(g);
    }

    private bool IsEnemyMinion(GameObject g) {
      return g.GetComponent<Minion>() && g.GetComponent<Minion>().Team != hero.Team;
    }

    private bool IsEnemyHero(GameObject g) {
      if(g.GetComponent<HeroGolem>() && g.GetComponent<HeroGolem>().Team != hero.Team) {
        return true;
      } else if(g.GetComponent<HeroGrunt>() && g.GetComponent<HeroGrunt>().Team != hero.Team) {
        return true;
      }
      return false;
    }

    private bool IsEnemyTower(GameObject g) {
      return g.GetComponentInChildren<Tower>() && g.GetComponentInChildren<Tower>().Team != hero.Team;
    }

    private bool IsEnemyShrine(GameObject g) {
      return g.GetComponentInChildren<Shrine>() && g.GetComponentInChildren<Shrine>().Team != hero.Team;
    }

    private bool IsMyTower(GameObject g) {
      return g.GetComponentInChildren<Tower>() && g.GetComponentInChildren<Tower>().Team == hero.Team;
    }

    public List<GameObject> NearbyEnemies() {
      List<GameObject> nearby = new List<GameObject>();
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(IsEnemy(c.gameObject)) {
          nearby.Add(c.gameObject);
        }
      }
      return nearby;
    }

    public List<GameObject> NearbyEnemyCharacters() {
      List<GameObject> nearby = new List<GameObject>();
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(IsEnemyCharacter(c.gameObject)) {
          nearby.Add(c.gameObject);
        }
      }
      return nearby;
    }

    public List<GameObject> NearbyMyTowers() {
      List<GameObject> nearby = new List<GameObject>();
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, visionRadius);
      foreach(Collider c in hitColliders) {
        if(IsMyTower(c.gameObject)) {
          nearby.Add(c.gameObject);
        }
      }
      return nearby;
    }

    public int TypeToRanking(GameObject g) {
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
      List<GameObject> nearby = npc.NearbyEnemies();
      return nearby.Count > 0;
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
      npc.hero.Move(npc.opponentsShrine.transform.position);
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
      List<GameObject> nearby = npc.NearbyEnemies();
      Vector3 npcPos = npc.gameObject.transform.position;
      Vector3 dest = npc.opponentsShrine.transform.position;
      float dist = Vector3.Distance(npcPos, dest);
      foreach(GameObject c in nearby) {
        Vector3 cPos = c.transform.position;
        Vector3 newDest = cPos;
        float newDist = Vector3.Distance(npcPos, cPos);
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
      List<GameObject> nearby = npc.NearbyEnemies();
      Vector3 npcPos = npc.gameObject.transform.position;
      Vector3 dest = npcPos;
      float dist = Vector3.Distance(npcPos, dest);
      int ranking = 0;
      foreach(GameObject c in nearby) {
        Vector3 cPos = c.transform.position;
        Vector3 newDest = cPos;
        float newDist = Vector3.Distance(npcPos, cPos);
        int newRanking = npc.TypeToRanking(c.gameObject);
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
