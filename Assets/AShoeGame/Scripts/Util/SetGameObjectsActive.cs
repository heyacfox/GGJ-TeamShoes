using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CallenCode // a namespace for awesome, purely standalone scripts, like this one.
{
    // Script to call SetActive() on sets of GameObjects in this script's OnEnable() method.
    public class SetGameObjectsActive : MonoBehaviour
    {
        [Tooltip("Calls SetActive(true) in OnEnable() for each object in list.")]
        public GameObject[] ActivateObjects;
        [Tooltip("Calls SetActive(false) in OnEnable() for each object in list.")]
        public GameObject[] DeactivateObjects;
        [Tooltip("Calls SetActive(!activeSelf) in OnEnable() for each object in list.")]
        public GameObject[] ToggleObjects;

        void OnEnable()
        {
            int ca = ActivateObjects.Length, cd = DeactivateObjects.Length, ct = ToggleObjects.Length;
            for (int i = 0; i < ca; i++)
                ActivateObjects[i].SetActive(true);
            for (int i = 0; i < cd; i++)
                DeactivateObjects[i].SetActive(false);
            for (int i = 0; i < ct; i++)
                ToggleObjects[i].SetActive(!ToggleObjects[i].activeSelf);
        }
    }
}