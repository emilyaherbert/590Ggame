using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroClash {
internal struct API {
    public bool IsEnemy(GameObject g, TEAM Team) {
      return IsEnemyHero(g, Team) || IsEnemyMinion(g, Team) || IsEnemyShrine(g, Team) || IsEnemyTower(g, Team);
    }

    public bool IsEnemyCharacter(GameObject g, TEAM Team) {
      return IsEnemyHero(g, Team) || IsEnemyMinion(g, Team);
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
      return (g.tag == "Structure") && g.transform.Find("Behavior") && g.GetComponentInChildren<Tower>() && (g.GetComponentInChildren<Tower>().Team != Team);
    }

    public bool IsEnemyShrine(GameObject g, TEAM Team) {
      return (g.tag == "Structure") && g.transform.Find("Behavior") && g.GetComponentInChildren<Shrine>() && (g.GetComponentInChildren<Shrine>().Team != Team);
    }

    public bool IsMyTower(GameObject g, TEAM Team) {
      return (g.tag == "Structure") && g.transform.Find("Behavior") && g.GetComponentInChildren<Tower>() && g.GetComponentInChildren<Tower>().Team == Team;
    }

    public int TypeToRanking(GameObject g, TEAM Team) {
      if(IsEnemyMinion(g, Team)) {
        return 1;
      } else if(IsEnemyTower(g, Team)) {
        return 2;
      } else if(IsEnemyHero(g, Team)) {
        return 3;
      } else if(IsEnemyShrine(g, Team)) {
        return 4;
      } else {
        return -1;
      }
    }
}   
}