using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroClash {
    internal class HeroGolemHealthBarController : MonoBehaviour {

        //public Color backgroundColor;
        //public Color barColor;
        //public Color deadColor;

        private Stat stat;
        private Slider slider;

        // Start is called before the first frame update
        void Start()
        {
            stat = transform.parent.parent.GetComponentInChildren<HeroGolem>().Self;
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