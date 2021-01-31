using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGripPoser : MonoBehaviour
{
    public bool IsLeft;
    public GameObject ActiveWhenGripping;
    public GameObject ActiveWhenNotGripping;

    private void Start()
    {
        if (IsLeft) ActiveWhenGripping.transform.localScale = new Vector3(-1, 1, 1);
    }

    void Update()
    {
        bool grip = false;
        grip |= (IsLeft ? CallenVrWrapper.Inst.LeftGrip : CallenVrWrapper.Inst.RightGrip);
        grip |= (IsLeft ? CallenVrWrapper.Inst.LeftTrigger : CallenVrWrapper.Inst.RightTrigger);
        ActiveWhenGripping.SetActive(grip);
        ActiveWhenNotGripping.SetActive(!grip);
    }
}
