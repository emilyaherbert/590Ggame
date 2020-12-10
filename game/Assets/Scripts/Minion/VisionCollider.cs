using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroClash {
internal class VisionCollider : MonoBehaviour {

    private TEAM Team;
    private API api;

    void Awake() {
        api = new API();
    }

    void Start() {
        Team = transform.parent.GetComponent<Minion>().Team;
    }

    public void OnTriggerEnter(Collider c) {
      GameObject obj = c.gameObject;
      if(api.IsEnemy(obj, Team) && !transform.parent.GetComponent<Minion>().enemiesWithinVision.Contains(obj)) {
        transform.parent.GetComponent<Minion>().enemiesWithinVision.Add(obj);
      }
    }

    public void OnTriggerExit(Collider c) {
      GameObject obj = c.gameObject;
      if(api.IsEnemy(obj, Team) && transform.parent.GetComponent<Minion>().enemiesWithinVision.Contains(obj)) {
        transform.parent.GetComponent<Minion>().enemiesWithinVision.Remove(obj);
      }
    }
}   
}