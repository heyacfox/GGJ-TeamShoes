using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallenVrTurnKnob : MonoBehaviour
{

    public Vector3 BoxSize = Vector3.one;

    public CallenVrGrabbable Grabbable { get; private set; }

    public float Angle, MinAngle, MaxAngle;

    // Gives the normalized position between min/max angle, or if they arent set just returns Angle degrees / 180
    public float Angle01 { get { return (MaxAngle != 0 || MinAngle != 0) ? Mathf.InverseLerp(MinAngle, MaxAngle, Angle) : (Angle / 180); } }

    Coroutine grabCor = null;
    bool isFuckingWithGrabs = false;
    float lastAngZ = 0;

    // Use this for initialization
    void Awake()
    {
        if(MinAngle > MaxAngle)
        {
            float temp = MinAngle;
            MinAngle = MaxAngle;
            MaxAngle = MinAngle;
        }

        if (MinAngle != 0 || MaxAngle != 0) Angle = Mathf.Clamp(Angle, MinAngle, MaxAngle);
        transform.localRotation = Quaternion.Euler(0, 0, Angle);

        var gob = new GameObject("CVR-Grabbable");
        gob.AddComponent<BoxCollider>().size = BoxSize;
        Grabbable = gob.AddComponent<CallenVrGrabbable>();
        Grabbable.transform.parent = transform;
        Grabbable.transform.Reset();
        if (GetComponent<AudioSource>())
            Grabbable.OnGrabSfx = GetComponent<AudioSource>();
    }
    private void Start()
    {
        if (Grabbable.OnGrab == null) Grabbable.OnGrab = new UnityEngine.Events.UnityEvent();
        if (Grabbable.OnRelease == null) Grabbable.OnRelease = new UnityEngine.Events.UnityEvent();
        Grabbable.OnGrab.AddListener(grab);
        Grabbable.OnRelease.AddListener(release);
    }

    void grab()
    {
        if (isFuckingWithGrabs)
            return;
        if (grabCor == null)
            grabCor = StartCoroutine(grabUpdate2());
    }

    void release()
    {
        if (isFuckingWithGrabs)
            return;
        if (grabCor != null)
            StopCoroutine(grabCor);
        grabCor = null;
        Grabbable.transform.Reset();
    }

    IEnumerator grabUpdate2()
    {
        //Angle = transform.localRotation.eulerAngles.z;
        lastAngZ = Grabbable.transform.localRotation.eulerAngles.z;
        while (true)
        {
            yield return null;
            float angZ = Grabbable.transform.localRotation.eulerAngles.z;
            float deltaAngle = Mathf.DeltaAngle(lastAngZ, angZ);
            Grabbable.transform.localRotation = Quaternion.Euler(0, 0, angZ);
            lastAngZ = angZ;
            Angle += deltaAngle;

            if(MinAngle != 0 || MaxAngle != 0)
            {
                Angle = Mathf.Clamp(Angle, MinAngle, MaxAngle);
            }
            transform.localRotation = Quaternion.Euler(0, 0, Angle);
            Grabbable.transform.localRotation = Quaternion.identity;
        }
    }

    IEnumerator grabUpdate()
    {
        bool needsToFuckWithGrabs = false;
        while (true)
        {
            yield return null;
            float angZ = Grabbable.transform.localRotation.eulerAngles.z;
            Grabbable.transform.localRotation = Quaternion.Euler(0, 0, angZ);

            float deltaAngle = Mathf.DeltaAngle(lastAngZ, angZ);
            lastAngZ = angZ;
            Angle += deltaAngle;

            //if (MinAngle != 0 || MaxAngle != 0) // do clamping
            //{
            //    float clampedAng = Mathf.Clamp(Angle, MinAngle, MaxAngle);
            //    if (!Mathf.Approximately(clampedAng, Angle))
            //    {
            //        needsToFuckWithGrabs = true;
            //        Angle = clampedAng;
            //    }
            //}

            Quaternion delta = Quaternion.Inverse(transform.rotation) * Grabbable.transform.rotation;
            transform.rotation *= delta;
            Grabbable.transform.rotation *= Quaternion.Inverse(delta);

            //if (needsToFuckWithGrabs) // clamping means we must quickly let go and regrab (could have bugs when both grip+trigger pressed)
            //{
            //    bool isLeft = Grabbable.IsGrabbedLeft;
            //    bool isTrigger = isLeft ? CallenVrWrapper.Inst.LeftTrigger : CallenVrWrapper.Inst.RightTrigger;
            //    isFuckingWithGrabs = true;
            //    Grabbable.Release();
            //    Grabbable.transform.Reset();
            //    Grabbable.Grab(isLeft, isTrigger);
            //    isFuckingWithGrabs = false;
            //}
        }
    }
}
