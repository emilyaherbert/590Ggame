using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroClash {
    internal class TowerIntegrityBarController : MonoBehaviour {

        private Slider slider;
        private float maxIntegrity;

        // Start is called before the first frame update
        void Start()
        {
            maxIntegrity = transform.parent.parent.GetComponentInChildren<Tower>().Integrity;
            slider = gameObject.GetComponent<Slider>();
        }

        // Update is called once per frame
        void Update()
        {
            SetHealthBarUI();
        }

        private void SetHealthBarUI() {
            if(transform.parent.parent.GetComponentInChildren<Tower>()) {
                float integrity = transform.parent.parent.GetComponentInChildren<Tower>().Integrity;
                if(integrity > maxIntegrity) {
                    maxIntegrity = integrity;
                }
                slider.value = integrity/maxIntegrity * 100.0f;
            }
        }
    }
}