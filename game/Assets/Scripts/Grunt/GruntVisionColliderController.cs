using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroClash {
internal class GruntVisionColliderController : MonoBehaviour {

    private TEAM Team;
    private API api;

    void Awake() {
        api = new API();
    }

    void Start() {
        Team = transform.parent.GetComponent<NPC>().Team;
    }

    public void OnTriggerEnter(Collider c) {
      GameObject obj = c.gameObject;
      if(api.IsEnemy(obj, Team) && !transform.parent.GetComponent<NPC>().enemiesWithinVision.Contains(obj)) {
        transform.parent.GetComponent<NPC>().enemiesWithinVision.Add(obj);
      }
    }

    public void OnTriggerExit(Collider c) {
      GameObject obj = c.gameObject;
      if(api.IsEnemy(obj, Team) && transform.parent.GetComponent<NPC>().enemiesWithinVision.Contains(obj)) {
        transform.parent.GetComponent<NPC>().enemiesWithinVision.Remove(obj);
      }
    }
}   
}