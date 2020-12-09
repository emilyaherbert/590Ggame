using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroClash {
    internal class HeroGruntHealthBarController : MonoBehaviour {

        private Stat stat;
        private Slider slider;

        // Start is called before the first frame update
        void Start()
        {
            stat = transform.parent.parent.GetComponentInChildren<HeroGrunt>().Self;
            slider = gameObject.GetComponent<Slider>();
        }

        // Update is called once per frame
        void Update()
        {
            SetHealthBarUI();
        }

        private void SetHealthBarUI() {
            slider.value = stat.Health/stat.MaxHealth * 100.0f;
        }
    }
}