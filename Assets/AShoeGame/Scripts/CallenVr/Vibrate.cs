using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vibrate : MonoBehaviour
{
    public bool Left;

    public float DestroyTimeSecs;

    // Use this for initialization
    void Start()
    {
        hapticsClipEven = OVRManager.isHmdPresent ? new OVRHapticsClip(hapticsEven, hapticsEven.Length) : null;
        hapticsClipOdd = OVRManager.isHmdPresent ? new OVRHapticsClip(hapticsOdd, hapticsOdd.Length) : null;
    }

    OVRHapticsClip hapticsClipEven, hapticsClipOdd;
    byte[] hapticsEven = new byte[] { 0, 128, 255, };
    byte[] hapticsOdd = new byte[] { 0, 128, 128, };
    bool odd = false;

    // Update is called once per frame
    void Update()
    {
        var hapticsClip = odd ? hapticsClipEven : hapticsClipOdd;
        odd = !odd;

        if (hapticsClip != null)
        {
            //Debug.Log("Vibe");
            if (Left)
                OVRHaptics.LeftChannel.Queue(hapticsClip);
            else
                OVRHaptics.RightChannel.Queue(hapticsClip);
        }
        else
            ;// Debug.Log("No vibe?");

        if(DestroyTimeSecs > 0 && (DestroyTimeSecs -= Time.deltaTime) <= 0)
        {
            Destroy(this);
        }
    }
}
