using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ShoeDef : MonoBehaviour
{
    public enum ShoeType
    {
        Male01,
        Male02,
        Male03,
        Female01,
        Female02,
        Female03
    }

    public enum ShoeState
    {
        None,
        OnStack,
        OffStack,
        OnNpcStartFoot,
        OnNpcTargetFoot
    }

    public enum ShoeSex
    {
        Any = 0,
        MaleOnly,
        FemaleOnly
    }

    public Collider Coll;

    public ShoeSex Sex = ShoeSex.Any;
    public ShoeState State = ShoeState.None;

    [Header("If blank, uses gameObject.name")]
    public string ShoeName;

    // idea here is the NPC would Instantiate a copy of this shoe, set IsNpcCopy=true, and attach it to their foot. we probably only need the collider, def not the grabbable...
    internal bool IsNpcCopy = false;

    Rigidbody rb;
    CallenVrGrabbable grab;

    public ShoeType Type;
    public bool FootOnShoe;
    public bool BareFootShoe;
    [SerializeField] ParticleSystem successfulParticle;
    [SerializeField] ParticleSystem unsuccessfulParticle;

    void Awake()
    {
        State = ShoeState.None;

        if (string.IsNullOrEmpty(ShoeName))
            ShoeName = gameObject.name;

        if (IsNpcCopy)
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
            Debug.LogError("ShoeDef didn't find a collider on gob=" + gameObject.name + ". creating default capsule.");
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
        {
            grab = gameObject.AddComponent<CallenVrGrabbable>();
            grab.FollowStyle = CallenVrGrabbable.HandFollowType.ParentAttached;
            grab.ForceNonKinematicOnRelease = true;
        }

        if (FootOnShoe)
        {
            if (grab)
                Destroy(grab);

            if (rb)
                Destroy(rb);
        }

        if (BareFootShoe)
        {
            if (Coll)
                Destroy(Coll);
        }

    }

    public void SuccessfulFit()
    {
        if (FootOnShoe)
            return;

        // TODO add particle effect.  Should be on grabbable shoe though because that will be in player's hand.
        // TODO stretch add haptic feedback
        gameObject.SetActive(false);
        //successfulParticle.Play();
        Destroy(this, 5f);
    }

    public void UnSuccessfulFit()
    {
        if (FootOnShoe)
            return;
        // TODO add angry response
        //unsuccessfulParticle.Play();
    }

}
