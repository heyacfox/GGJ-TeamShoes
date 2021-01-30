using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary> Attach to a gameObject to allow the CallenVrGrabHands script to grab this gameObject. </summary>
[RequireComponent(typeof(Collider))]
public class CallenVrGrabbable : MonoBehaviour
{
    public enum HandFollowType { ParentAttached, CopyTransform, FixedJoint, PhysicsForce }

    public enum HandInteraction { Both, Left, Right }

    public HandFollowType FollowStyle = HandFollowType.CopyTransform;

    public HandInteraction HandsAllowed = HandInteraction.Both;

    public float ReleaseVelocityMultiplier = 1;
    public float ReleaseSpinMultiplier = 1;

    public bool IsGrabbed { get { return IsGrabbedLeft || IsGrabbedRight; } }
    public bool IsGrabbedLeft { get; private set; }
    public bool IsGrabbedRight { get; private set; }

    public Collider Collider { get { if (!coll) coll = GetComponent<Collider>(); return coll; } }

    public bool ExcludeChildren;

    public bool ForceNonKinematicOnGrab;
    public bool ForceNonKinematicOnRelease;

    public AudioSource OnGrabSfx;

    //[Header("Changes the catch position, relative to the hand.")]
    //public Vector3 GrabPosScaling = Vector3.one;
    //public Vector3 GrabPosOffset = Vector3.zero;

    public UnityEvent OnGrab, OnRelease;

    HandFollowType curFollowStyle;
    Transform previousParent = null;
    Rigidbody rb;
    bool wasKinematic;
    bool hadGravity;
    float prevDrag, prevAngDrag;
    Collider coll;
    Vector3 posOffset;
    Quaternion rotOffset;
    Joint grabJoint;
    bool monitorTrigger, monitorGrab;

    public void Grab(bool grabbedByLeftHand, bool wasTriggerButton)
    {
        Debug.Log("Grabbed gob=" + gameObject.name);

        if (!CallenVrWrapper.Inst)
        {
            Debug.LogWarning("Cannot call CallenVrGrabbable.Grab when CallenVrWrapper.Inst is null.");
            return;
        }

        if (grabbedByLeftHand && HandsAllowed == HandInteraction.Right)
            return;
        if (!grabbedByLeftHand && HandsAllowed == HandInteraction.Left)
            return;

        if ((IsGrabbedLeft && !grabbedByLeftHand) || (IsGrabbedRight && grabbedByLeftHand))
        {
#if UNITY_EDITOR
            Debug.LogWarning("Grab hand switch not supported at this time.");
#endif
            return;
        }

        bool regrabbed = (IsGrabbedLeft && grabbedByLeftHand) || (IsGrabbedRight && !grabbedByLeftHand);

        if (regrabbed)
        {
            if (wasTriggerButton) monitorTrigger = true;
            else monitorGrab = true;

            return;
        }

        IsGrabbedLeft = grabbedByLeftHand;
        IsGrabbedRight = !grabbedByLeftHand;
        curFollowStyle = FollowStyle;

        monitorTrigger = wasTriggerButton;
        monitorGrab = !monitorTrigger;


        if (!rb) rb = GetComponent<Rigidbody>();

        var handTform = grabbedByLeftHand ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand;
        if (curFollowStyle == HandFollowType.ParentAttached)
        {
            if (rb)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = !ForceNonKinematicOnGrab;
            }
            previousParent = transform.parent;
            transform.parent = handTform;
        }
        else if (curFollowStyle == HandFollowType.CopyTransform)
        {
            if (rb)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = !ForceNonKinematicOnGrab;
            }
            posOffset = handTform.InverseTransformPoint(transform.position);
            rotOffset = Quaternion.Inverse(transform.rotation) * handTform.rotation;
        }
        else if (curFollowStyle == HandFollowType.PhysicsForce)
        {
            if (rb)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = false;// !ForceNonKinematicOnGrab;
            }
            posOffset = handTform.InverseTransformPoint(transform.position);
            rotOffset = Quaternion.Inverse(transform.rotation) * handTform.rotation;
        }
        else if(curFollowStyle == HandFollowType.FixedJoint)
        {
            if (!rb) throw new UnityException("Cannot use FixedJoint without a rigidbody!");
            if (grabJoint == null)
                createJoint(handTform);
        }


        CallenVrWrapper.Inst.Vibrate(grabbedByLeftHand);
        AudioSource grabSfx = OnGrabSfx ? OnGrabSfx : CallenVrGrabHands.Inst ? CallenVrGrabHands.Inst.DefaultGrabSfx : null;
        if (grabSfx && grabSfx.clip)
        {
            var newSfxObject = new GameObject("GrabSfx-OneShot");
            var newSfx = newSfxObject.AddComponent<AudioSource>();
            newSfx.volume = grabSfx.volume;
            newSfx.minDistance = grabSfx.minDistance;
            newSfx.maxDistance = grabSfx.maxDistance;
            newSfx.rolloffMode = grabSfx.rolloffMode;
            newSfx.pitch = grabSfx.pitch;
            newSfx.outputAudioMixerGroup = grabSfx.outputAudioMixerGroup;
            newSfx.dopplerLevel = grabSfx.dopplerLevel;
            newSfx.bypassEffects = grabSfx.bypassEffects;
            newSfx.bypassListenerEffects = grabSfx.bypassListenerEffects;
            newSfx.bypassReverbZones = grabSfx.bypassReverbZones;
            newSfx.reverbZoneMix = grabSfx.reverbZoneMix;
            newSfx.spatialBlend = grabSfx.spatialBlend;
            newSfx.priority = grabSfx.priority;
            newSfx.clip = grabSfx.clip;
            newSfx.transform.position = transform.position;
            newSfx.Play();
            Destroy(newSfx.gameObject, newSfx.clip.length + 0.05f);
        }

        if (OnGrab != null)
        {
            Debug.Log("Invoke Grab.OnGrab()");
            OnGrab.Invoke();
        }
    }

    public void Release()
    {
        if (!IsGrabbed) return;
        release();
    }

    void release() 
    {
        Debug.Log("CVR-Grab: Released gob=" + gameObject.name);


        bool left = IsGrabbedLeft;
        IsGrabbedLeft = IsGrabbedRight = false;

        if(curFollowStyle == HandFollowType.ParentAttached)
        {
            transform.parent = previousParent;
            if (rb) rb.isKinematic = wasKinematic && !ForceNonKinematicOnRelease;
        }
        else if(curFollowStyle == HandFollowType.CopyTransform)
        {
            if (rb) rb.isKinematic = wasKinematic && !ForceNonKinematicOnRelease;
        }
        else if(curFollowStyle == HandFollowType.FixedJoint && grabJoint)
        {
            if (rb) rb.isKinematic = wasKinematic && !ForceNonKinematicOnRelease;
            destroyJoint();
        }
        else if (curFollowStyle == HandFollowType.PhysicsForce)
        {
            if (rb) rb.isKinematic = wasKinematic && !ForceNonKinematicOnRelease;
        }

        if (ReleaseVelocityMultiplier > 0 && rb)
        {
            rb.velocity = ReleaseVelocityMultiplier * (left ? CallenVrWrapper.Inst.LeftVelocityWorldSpace : CallenVrWrapper.Inst.RightVelocityWorldSpace);
            rb.angularVelocity = ReleaseSpinMultiplier * (left ? CallenVrWrapper.Inst.LeftAngularVelocityWorldSpace : CallenVrWrapper.Inst.RightAngularVelocityWorldSpace);
            foreach (var rbody in rb.GetComponentsInChildren<Rigidbody>())
            {
                rbody.velocity = ReleaseVelocityMultiplier * (left ? CallenVrWrapper.Inst.LeftVelocityWorldSpace : CallenVrWrapper.Inst.RightVelocityWorldSpace);
                rbody.angularVelocity = ReleaseSpinMultiplier * (left ? CallenVrWrapper.Inst.LeftAngularVelocityWorldSpace : CallenVrWrapper.Inst.RightAngularVelocityWorldSpace);
            }
            Debug.Log("CVR-Grab: Velocity=" + rb.velocity + ", angVel=" + rb.angularVelocity);
        }

        if (OnRelease != null)
        {
            Debug.Log("Invoke Grab.OnRelease()");
            OnRelease.Invoke();
        }
    }

    public void MatchHandPositionRotation(float amount01, Quaternion offsetRotation = new Quaternion(), Vector3 offsetPosition = new Vector3())
    {
        if (!IsGrabbed)
            return;
        if (FollowStyle == HandFollowType.FixedJoint)
        {
            Debug.LogWarning("MatchHandPositionRotation not supported for FixedJoint Grabbable");
            return;
        }
        if (offsetRotation.x == 0 && offsetRotation.y == 0 && offsetRotation.z == 0 && offsetRotation.w == 0) offsetRotation = Quaternion.identity;

        if (FollowStyle == HandFollowType.CopyTransform || FollowStyle == HandFollowType.PhysicsForce)
        {
            var handTform = IsGrabbedLeft ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand;
            posOffset = Vector3.Lerp(posOffset, offsetPosition, amount01);
            rotOffset = Quaternion.Slerp(rotOffset, Quaternion.Inverse(offsetRotation), amount01);
        }
        else if (FollowStyle == HandFollowType.ParentAttached)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, offsetPosition, amount01);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, offsetRotation, amount01);
        }
        else Debug.LogError("Unhandled HandFollowType=" + FollowStyle);
    }

    void updateCopyTransform()
    {
        if (curFollowStyle != HandFollowType.CopyTransform)
            return;

        var tform = IsGrabbedLeft ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand;
        var pos = tform.TransformPoint(posOffset);
        var rot = tform.rotation * Quaternion.Inverse(rotOffset);

        if (rb)
        {
            //rb.MovePosition(pos);
            //rb.MoveRotation(rot);
            rb.position = pos;
            rb.rotation = rot;
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }
    

    void Update()
    {
        if (!IsGrabbed || !CallenVrWrapper.Inst) return;

        //Debug.Log("Grabbed gob=" + gameObject.name);

        if (!rb && curFollowStyle == HandFollowType.CopyTransform)
            updateCopyTransform();

        bool trigRelease = false, grabRelease = false;
        if (IsGrabbedLeft)
        {
            trigRelease = monitorTrigger && !CallenVrWrapper.Inst.LeftTrigger;
            grabRelease = monitorGrab && !CallenVrWrapper.Inst.LeftGrip;
        }
        else if (IsGrabbedRight)
        {
            trigRelease = monitorTrigger && !CallenVrWrapper.Inst.RightTrigger;
            grabRelease = monitorGrab && !CallenVrWrapper.Inst.RightGrip;
        }
        if (trigRelease || grabRelease)
            release();

    }

    private void FixedUpdate()
    {
        if (IsGrabbed && rb && CallenVrWrapper.Inst)
        {
            if (curFollowStyle == HandFollowType.CopyTransform)
                updateCopyTransform();
            else if (curFollowStyle == HandFollowType.FixedJoint && grabJoint)
                updateJoint();
            else if (curFollowStyle == HandFollowType.PhysicsForce)
                updatePhysicsForces();
        }
    }

    private void OnDisable()
    {
        if (IsGrabbed)
            release();
    }

    void createJoint(Transform handTform)
    {
        grabJoint = gameObject.AddComponent<FixedJoint>();
        posOffset = handTform.InverseTransformPoint(transform.position);
        var handRb = handTform.GetComponent<Rigidbody>();
        if (handRb)
        {
            grabJoint.connectedBody = handRb;
        }
        else
        {
            grabJoint.autoConfigureConnectedAnchor = false;
            //grabJoint.connectedAnchor = (handTform.position + posOffset);
            grabJoint.connectedAnchor = handTform.TransformPoint(new Vector3(-0.02f, 0.04f, 0));
        }

        if (rb) // set up rigidbody
        {
            prevDrag = rb.drag;
            prevAngDrag = rb.angularDrag;
            hadGravity = rb.useGravity;
            //rb.drag = 10;
            //rb.angularDrag = 10;
            //rb.useGravity = false;
        }

        

        //set up config joint
        ConfigurableJoint joint = grabJoint as ConfigurableJoint;
        if (joint)
        {
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            joint.linearLimit = new SoftJointLimit() { limit = 0.001f, bounciness = 0.02f };
            joint.lowAngularXLimit =
                joint.highAngularXLimit =
                joint.angularYLimit =
                joint.angularZLimit = new SoftJointLimit() { limit = 0.2f, bounciness = 0.02f }; ;
            joint.projectionMode = JointProjectionMode.PositionAndRotation;
        }
    }

    void destroyJoint()
    {
        if (rb)
        {
            rb.drag = prevDrag;
            rb.angularDrag = prevAngDrag;
            rb.useGravity = hadGravity;
        }
        Destroy(grabJoint);
    }

    void updateJoint()
    {
        if (!grabJoint.connectedBody)
        {
            var hand = (IsGrabbedLeft ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand);
            //Vector3 pos = hand.position + posOffset;
            Vector3 pos = hand.TransformPoint(new Vector3(-0.02f, 0.04f, 0));
            //Quaternion rot = hand.rotation * rotOffset;            
            grabJoint.connectedAnchor = pos;
            //Debug.Log("anchor = " + grabJoint.connectedAnchor + ", " + grabJoint.breakForce + ", " + grabJoint.currentForce);
        }
    }

    void updatePhysicsForces()
    {
        if (curFollowStyle != HandFollowType.PhysicsForce || !rb)
            return;

        var tform = IsGrabbedLeft ? CallenVrWrapper.Inst.LeftHand : CallenVrWrapper.Inst.RightHand;
        var pos = tform.TransformPoint(posOffset);
        //var rot = tform.rotation * Quaternion.Inverse(rotOffset);

        var linForce = (pos - rb.position) * 50f;
        Debug.Log("add force=" + linForce);

        rb.AddForce(linForce, ForceMode.Force);
       // rb.AddTorque(Quaternion.RotateTowards(rb.rotation, rot, 10).eulerAngles * 0.1f, ForceMode.Force);
    }
}
