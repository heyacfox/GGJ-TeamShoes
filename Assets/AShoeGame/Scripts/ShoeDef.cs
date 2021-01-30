using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeDef : MonoBehaviour
{
    public Collider Coll;

    // idea here is the NPC would Instantiate a copy of this shoe, set IsNpcCopy=true, and attach it to their foot. we probably only need the collider, def not the grabbable...
    internal bool IsNpcCopy = false;

    Rigidbody rb;
    CallenVrGrabbable grab;

    void Awake()
    {
        if(IsNpcCopy)
        {
            if ((rb = GetComponent<Rigidbody>()) && rb.isKinematic) Debug.LogWarning("Kinematic rigidbody on NPC shoe?");
            if (grab = GetComponent<CallenVrGrabbable>()) Destroy(grab); // no reason for NPC shoe to be grabbable, instead of logging, just Destroy it

            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1)); // flip on x-axis
            return;
        }

        if (!Coll) Coll = GetComponent<Collider>();
        if (!Coll)
        {
            // lack of collider emits error, since it's very shoe-specific
            Debug.LogError("ShoeDef didn't find a collider on gob="+gameObject.name+". creating default capsule.");
            Coll = gameObject.AddComponent<CapsuleCollider>();
            (Coll as CapsuleCollider).radius = 0.1f;
            (Coll as CapsuleCollider).height = 0.4f;
            (Coll as CapsuleCollider).direction = 2; // z-axis, i think?
        }
        // other components below are expected to be created at runtime, so they don't emit errors.

        rb = GetComponent<Rigidbody>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.4f;
            rb.useGravity = true;
        }

        grab = GetComponent<CallenVrGrabbable>();
        if (!grab)
        {
            grab = gameObject.AddComponent<CallenVrGrabbable>();
            grab.FollowStyle = CallenVrGrabbable.HandFollowType.ParentAttached;
        }
    }
}
