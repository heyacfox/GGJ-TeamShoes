using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Creates and manages a pair of gameObjects with trigger colliders, allowing you to grab gameObjects with a CallenVrGrabbale script attached. </summary>
public class CallenVrGrabHands : MonoBehaviour {

    public static CallenVrGrabHands Inst { get; private set; }

    [Range(0.01f, 1.0f), Header("Grab max distance")]
    public float TriggerRadius = 0.1f;

    [Header("Left hand flips to -x value")]
    public Vector3 TriggerOffset;

    [Header("Restrict grabs to layers")]
    public LayerMask GrabbableLayers = ~0;

    public AudioSource DefaultGrabSfx;

    //public SphereCollider LeftTrigger { get; private set; }
    //public SphereCollider RightTrigger { get; private set; }

    internal Vector3 LeftOffset { get { return new Vector3(-TriggerOffset.x, TriggerOffset.y, TriggerOffset.z); } }
    internal Vector3 RightOffset { get { return TriggerOffset; } }

    readonly List<CallenVrGrabbable> leftGrabbables = new List<CallenVrGrabbable>();
    readonly List<CallenVrGrabbable> rightGrabbables = new List<CallenVrGrabbable>();

    void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(Inst.gameObject);
        }
        Inst = this;
    }

    //    setupHand(true);
    //    setupHand(false);
    //}

    //void setupHand(bool left)
    //{
    //    var hand = new GameObject((left ? "Left" : "Right") + "-GrabTrigger");
    //    hand.transform.parent = transform;
    //    var trig = hand.AddComponent<SphereCollider>();
    //    if (left) LeftTrigger = trig;
    //    else RightTrigger = trig;
    //    trig.isTrigger = true;
    //    trig.center = Vector3.Scale(TriggerOffset, !left ? Vector3.one : new Vector3(-1, 1, 1));
    //    trig.radius = TriggerRadius;
    //}

    bool wasLeftGrab, wasLeftTrig, wasRightGrab, wasRightTrig;

    void Update()
    {
        if (!CallenVrWrapper.Inst)
            return;

        //CallenVrWrapper.CopyTransformPosRot(CallenVrWrapper.Inst.LeftHand, LeftTrigger.transform);
        //CallenVrWrapper.CopyTransformPosRot(CallenVrWrapper.Inst.RightHand, RightTrigger.transform);

        bool leftGrab = CallenVrWrapper.Inst.LeftGrip;
        bool rightGrab = CallenVrWrapper.Inst.RightGrip;

        bool leftTrig = CallenVrWrapper.Inst.LeftTrigger;
        bool rightTrig = CallenVrWrapper.Inst.RightTrigger;


        if (leftGrab && !wasLeftGrab)
        {
            var leftGrabObj = pickClosest(true);
            if (leftGrabObj) leftGrabObj.Grab(true, false);
        }
        else if (leftTrig && !wasLeftTrig)
        {
            var leftGrabObj = pickClosest(true);
            if (leftGrabObj) leftGrabObj.Grab(true, true);
        }


        if (rightGrab && !wasRightGrab)
        {
            var rightGrabObj = pickClosest(false);
            if (rightGrabObj) rightGrabObj.Grab(false, false);
        }
        else if (rightTrig && !wasRightTrig)
        {
            var rightGrabObj = pickClosest(false);
            if (rightGrabObj) rightGrabObj.Grab(false, true);
        }


        wasLeftGrab = leftGrab;
        wasRightGrab = rightGrab;
        wasLeftTrig = leftTrig;
        wasRightTrig = rightTrig;
    }

    void FixedUpdate()
    {
        if (!CallenVrWrapper.Inst)
            return;
        updateHand(true, leftGrabbables);
        updateHand(false, rightGrabbables);
    }


    Collider[] collidersTemp = new Collider[512]; // used by updateHand and OverlapSphereNonAlloc

    void updateHand(bool left, List<CallenVrGrabbable> list)
    {
        list.Clear();

        Transform tform = left ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand;
        Vector3 pos = tform.TransformPoint(left ? LeftOffset : RightOffset);

        int collCount = Physics.OverlapSphereNonAlloc(pos, TriggerRadius, collidersTemp, GrabbableLayers.value);
        for (int i = 0; i < collCount; i++)
        {
            var grab = collidersTemp[i].GetComponent<CallenVrGrabbable>();
            if (grab || ((grab = collidersTemp[i].GetComponentInParent<CallenVrGrabbable>()) && !grab.ExcludeChildren)) list.Add(grab);
        }
#if UNITY_EDITOR
        Debug.DrawLine(pos - Vector3.up * 0.05f, pos + Vector3.up * 0.05f, Color.white);
        Debug.DrawLine(pos - Vector3.left * 0.05f, pos + Vector3.left * 0.05f, Color.white);
#endif
        //Debug.Log((left ? "Left" : "Right") + " saw " + colliders.Length + ", matched " + list.Count);
    }

    CallenVrGrabbable pickClosest(bool left)
    {
        var list = left ? leftGrabbables : rightGrabbables;

        Transform tform = left ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand;
        Vector3 pos = tform.TransformPoint(left ? LeftOffset : RightOffset);

        float closestDist = float.PositiveInfinity;
        int closestIx = -1;

        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i] || !list[i].enabled) continue;
            else if (list[i].IsGrabbed)
            {
                if (list[i].IsGrabbedLeft == left)
                {
#if UNITY_EDITOR
                    Debug.Log("Reselecting grabbed object " + list[i].gameObject.name);
#endif
                    return list[i];
                }
                else continue;
            }

            float dist = (list[i].Collider.ClosestPoint(pos) - pos).sqrMagnitude;
            if(dist < closestDist)
            {
                closestDist = dist;
                closestIx = i;
            }
        }

        //if (closestIx >= 0 && closestIx < list.Count) Debug.Log((left ? "Left" : "Right") + " grabs " + list[closestIx].gameObject.name + ", dist=" + Mathf.Sqrt(closestDist));

        if (closestIx >= 0 && closestIx < list.Count) return list[closestIx];
        else return null;
    }
}
