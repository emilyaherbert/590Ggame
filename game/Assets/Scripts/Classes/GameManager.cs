using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class GameManager : MonoBehaviour {
    internal static bool paused;

    private const float DIST_EPSILON = 0.1f,
                        SMOOTH_SPEED = 2.0f;
    private readonly Vector3 OFFSET = (100.0f * Vector3.up) + (60.0f * Vector3.back);

    private AudioSource source;
    private Transform target;

    private readonly Transform[] spawns = new Transform[2];
    [SerializeReference] private GameObject[] prefabs = new GameObject[2];
    [SerializeReference] public AudioClip[] clips = new AudioClip[4];

    private IEnumerator PlayMusic() {
      while (true) {
        foreach (AudioClip clip in clips) {
          source.clip = clip;
          source.Play();
          yield return new WaitForSeconds(source.clip.length);
        }
      }
    }

    private void Awake() {
      spawns[0] = GameObject.Find("GruntSpawn").transform;
      spawns[1] = GameObject.Find("GolemSpawn").transform;
      source = GetComponent<AudioSource>();
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
      _ = StartCoroutine(nameof(PlayMusic));
      _ = Instantiate(prefabs[0], spawns[0].position, spawns[0].rotation);
      // TODO: UI determine player
      Player p = Instantiate(prefabs[1],
        spawns[1].position, spawns[1].rotation).AddComponent<Player>();
      target = p.transform;
      p.hero = p.GetComponent<HeroGolem>();
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        paused = !paused;
        AudioListener.pause = !AudioListener.pause;
      }
    }
  }
}
