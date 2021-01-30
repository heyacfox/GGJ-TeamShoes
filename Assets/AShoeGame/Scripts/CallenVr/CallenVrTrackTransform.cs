using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallenVrTrackTransform : MonoBehaviour
{
    public enum TrackTargets { Head, LeftHand, RightHand }

    public TrackTargets TrackingTarget;

    public bool TrackPosition;
    public bool TrackRotation;

    public bool TrackLocal;

    public bool LockPositionY;

    void LateUpdate()
    {
        if (!CallenVrWrapper.Inst || (!TrackPosition && !TrackRotation))
            return;

        Transform targetTform = null;
        switch (TrackingTarget)
        {
            case TrackTargets.Head:
                targetTform = CallenVrWrapper.Inst.Head;
                break;
            case TrackTargets.LeftHand:
                targetTform = CallenVrWrapper.Inst.LeftHand;
                break;
            case TrackTargets.RightHand:
                targetTform = CallenVrWrapper.Inst.RightHand;
                break;
            default:
                Debug.LogWarning("Unknown TrackingTarget=" + TrackingTarget);
                break;
        }

        if(targetTform != null)
        {
            if(TrackLocal)
            {
                if (TrackRotation) transform.localRotation = targetTform.localRotation;
                if (TrackPosition)
                {
                    Vector3 targetPos = targetTform.localPosition;
                    if (LockPositionY) targetPos.y = transform.localPosition.y;
                    transform.localPosition = targetPos;
                }
            }
            else
            {
                if (TrackRotation) transform.rotation = targetTform.rotation;
                if (TrackPosition)
                {
                    Vector3 targetPos = targetTform.position;
                    if (LockPositionY) targetPos.y = transform.position.y;
                    transform.position = targetPos;
                }
            }
        }
    }
}
