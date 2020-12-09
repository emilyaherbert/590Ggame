using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroClash {
    internal class MinionHealthBarController : MonoBehaviour {

        private Slider slider;
        private float maxHealth;

        // Start is called before the first frame update
        void Start()
        {
            maxHealth = transform.parent.parent.GetComponentInChildren<Minion>().Self.MaxHealth;
            slider = gameObject.GetComponent<Slider>();
        }

        // Update is called once per frame
        void Update()
        {
            SetHealthBarUI();
        }

        private void SetHealthBarUI() {
            slider.value = transform.parent.parent.GetComponentInChildren<Minion>().Self.Health/maxHealth * 100.0f;
        }
    }
}