using UnityEngine;
namespace HeroClash {
  internal class NPC : MonoBehaviour {
    internal Hero hero;

    private GameObject myShrine;
    private GameObject opponentsShrine;

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
    }

    private void Update() {
      ClassifyDT(decisionTree);
    }

    private Node CreateDecisionTree() {
      return new HealthMeetsMinimumPercentage(0.25,
        new MoveTo(opponentsShrine.transform.position),
        new MoveTo(myShrine.transform.position));
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

  class MoveTo : Node {
    Vector3 dest;

    public MoveTo(Vector3 d) {
      nodeType = NODE_TYPE.LEAF;
      dest = d;
    }

    public override bool F(NPC npc) {
      npc.hero.Move(dest);
      return true;
    }
  }

}
