using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CallbackSystem
{
    public class UpdateUIListener : MonoBehaviour
    {
        [Header("Assign both players BatteryUI component")]
        [SerializeField] private GameObject[] UIs;
        private GameObject UI;
        void Start()
        {
            EventSystem.Current.RegisterListener<ActivationUIEvent>(UpdateUI);
        }

        private void UpdateUI(ActivationUIEvent eve)
        {
            UI = eve.isPlayerOne ? UIs[0] : UIs[1];
            if (eve.isAlive)
                UI.gameObject.SetActive(true);
            else
                UI.gameObject.SetActive(false);
        }
    }
}

