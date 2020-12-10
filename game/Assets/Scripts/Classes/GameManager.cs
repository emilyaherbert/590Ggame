using System.Collections;
using UnityEngine;
namespace HeroClash {
  internal class GameManager : MonoBehaviour {
    internal static bool paused;

    private const float DIST_EPSILON = 0.1f,
                        SMOOTH_SPEED = 2.0f;
    private readonly Vector3 OFFSET = (80.0f * Vector3.up) + (60.0f * Vector3.back);

    [SerializeReference] private Canvas pauseCanvas = default;
    private AudioSource source;
    private Transform target;

    private readonly Transform[] spawns = new Transform[2];
    private readonly Hero[] heroes = new Hero[2];
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

    private IEnumerator Respawn(Hero h) {
      while (true) {
        if (h.State == STATE.DEAD) {
          bool isP = h.gameObject.GetComponent<Player>();
          yield return new WaitForSeconds(1.0f);
          if (isP) {
            target = null;
          }
          Destroy(h.gameObject);
          yield return new WaitForSeconds(1.0f);
          if (isP) {
            Spawn(TEAM.GOOD);
          } else {
            Spawn(TEAM.EVIL);
          }
          break;
        }
        yield return null;
      }
    }

    private void Awake() {
      spawns[0] = GameObject.Find("GruntSpawn").transform;
      spawns[1] = GameObject.Find("GolemSpawn").transform;
      source = GetComponent<AudioSource>();
    }

    private void Start() {
      Spawn(TEAM.GOOD);
      Spawn(TEAM.EVIL);
      pauseCanvas.enabled = false;
      _ = StartCoroutine(nameof(PlayMusic));
    }

    private void Spawn(TEAM t) {
      if (t == TEAM.GOOD) {
        Player p = Instantiate(prefabs[1], spawns[1].position, spawns[1].rotation).AddComponent<Player>();
        p.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        p.hero = p.GetComponent<HeroGolem>();
        heroes[0] = p.hero;
        target = p.transform;
        _ = StartCoroutine(nameof(Respawn), heroes[0]);
      } else {
        NPC n = Instantiate(prefabs[0], spawns[0].position, spawns[0].rotation).AddComponent<NPC>();
        n.hero = n.GetComponent<HeroGrunt>();
        heroes[1] = n.hero;
        _ = StartCoroutine(nameof(Respawn), heroes[1]);
      }
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        paused = !paused;
        pauseCanvas.transform.position = transform.position;
        pauseCanvas.enabled = paused;
        AudioListener.pause = !AudioListener.pause;
      }
    }

    private void LateUpdate() {
      if (target != null && Vector3.Distance(transform.position,
        target.position + OFFSET) > DIST_EPSILON) {
        transform.position = Vector3.Lerp(transform.position,
          target.position + OFFSET,
          SMOOTH_SPEED * Time.deltaTime);
      }
    }
  }
}
