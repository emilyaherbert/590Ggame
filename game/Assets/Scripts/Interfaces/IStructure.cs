using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal interface IStructure {
    float Integrity { get; set; }
    float SpawnRate { get; }
    TEAM Team { get; set; }

    void OnTriggerEnter(Collider collider);
    void OnTriggerExit(Collider collider);
    IEnumerator Monitor();
    IEnumerator Spawn();
  }
}
