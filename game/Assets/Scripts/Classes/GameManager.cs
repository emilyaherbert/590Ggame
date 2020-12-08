﻿using UnityEngine;
namespace HeroClash {
  internal class GameManager : MonoBehaviour {
    internal static bool paused;

    private const float DIST_EPSILON = 0.1f,
                        SMOOTH_SPEED = 2.0f;
    private readonly Vector3 OFFSET = (100.0f * Vector3.up) + (60.0f * Vector3.back);

    private Transform target;

    private readonly Transform[] spawns = new Transform[2];
    [SerializeReference] private GameObject[] prefabs = new GameObject[2];

    private void Awake() {
      spawns[0] = GameObject.Find("GruntSpawn").transform;
      spawns[1] = GameObject.Find("GolemSpawn").transform;
    }

    private void LateUpdate() {
      if (Vector3.Distance(transform.position,
        target.position + OFFSET) > DIST_EPSILON) {
        transform.position = Vector3.Lerp(transform.position,
          target.position + OFFSET,
          SMOOTH_SPEED * Time.deltaTime);
      }
    }

    private void Start() {
      // TODO: UI to determine which character the player wants to play
      Player p = Instantiate(prefabs[1], spawns[1].position, spawns[1].rotation).AddComponent<Player>();
      p.hero = p.GetComponent<HeroGolem>();
      target = p.transform;
      NPC npc = Instantiate(prefabs[0], spawns[0].position, spawns[0].rotation).AddComponent<NPC>();
      npc.hero = npc.GetComponent<HeroGrunt>();
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        paused = !paused;
      }
    }
  }
}
