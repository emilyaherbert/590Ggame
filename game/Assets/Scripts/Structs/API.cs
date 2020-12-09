using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroClash {
internal struct API {
    public bool IsEnemy(GameObject g, TEAM Team) {
      return IsEnemyHero(g, Team) || IsEnemyMinion(g, Team) || IsEnemyShrine(g, Team) || IsEnemyTower(g, Team);
    }

    public bool IsEnemyMinion(GameObject g, TEAM Team) {
      return g.GetComponent<Minion>() && (g.GetComponent<Minion>().Team != Team);
    }

    public bool IsEnemyHero(GameObject g, TEAM Team) {
      if (g.GetComponent<HeroGolem>() && g.GetComponent<HeroGolem>().Team != Team) {
        return true;
      } else if (g.GetComponent<HeroGrunt>() && g.GetComponent<HeroGrunt>().Team != Team) {
        return true;
      }
      return false;
    }

    public bool IsEnemyTower(GameObject g, TEAM Team) {
      return (g.tag == "Structure") && g.GetComponentInChildren<Tower>() && (g.GetComponentInChildren<Tower>().Team != Team);
    }

    public bool IsEnemyShrine(GameObject g, TEAM Team) {
      return g.tag == "Structure" &&  g.GetComponentInChildren<Shrine>() && g.GetComponentInChildren<Shrine>().Team != Team;
    }
}   
}