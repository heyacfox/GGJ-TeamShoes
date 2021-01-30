using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CallenVrWrapper : MonoBehaviour
{
    public static CallenVrWrapper Inst { get; private set; }

    public bool ReportConnected;
    public bool ReportMounted;

    [SerializeField]
    private bool DestroyPrevious;

    [Space]

    public bool UseOvrVelocity;

    /// <summary>HMD attached</summary>
    public bool Connected { get; private set; }

    /// <summary>HMD is on the user's face (or at least, the sensor thinks it is)</summary>
    public bool Mounted { get; private set; }

    public Transform Head { get; private set; }
    public Transform LeftHand { get; private set; }
    public Transform RightHand { get; private set; }

    public bool LeftTrigger { get; private set; }
    public bool LeftGrip { get; private set; }
    public bool RightTrigger { get; private set; }
    public bool RightGrip { get; private set; }

    public bool LeftThumbButton { get { return LeftThumbButton1 || LeftThumbButton2; } }
    public bool RightThumbButton { get { return RightThumbButton1 || RightThumbButton2; } }

    public bool LeftThumbButton1 { get; private set; }
    public bool LeftThumbButton2 { get; private set; }
    public bool RightThumbButton1 { get; private set; }
    public bool RightThumbButton2 { get; private set; }

    public bool LeftThumbTouch { get; private set; }
    public bool LeftTriggerTouch { get; private set; }
    public bool LeftGripTouch { get; private set; }
    public bool RightThumbTouch { get; private set; }
    public bool RightTriggerTouch { get; private set; }
    public bool RightGripTouch { get; private set; }

    public Vector3 HeadVelocity { get; private set; }
    public Vector3 LeftVelocity { get; private set; }
    public Vector3 RightVelocity { get; private set; }
    public Vector3 HeadAngularVelocity { get; private set; }
    public Vector3 LeftAngularVelocity { get; internal set; }
    public Vector3 RightAngularVelocity { get; internal set; }


    public Vector3 HeadVelocityWorldSpace { get { return !ovrRig ? HeadVelocity : ovrRig.transform.TransformVector(HeadVelocity); } }
    public Vector3 LeftVelocityWorldSpace { get { return !ovrRig ? LeftVelocity : ovrRig.transform.TransformVector(LeftVelocity); } }
    public Vector3 RightVelocityWorldSpace { get { return !ovrRig ? RightVelocity : ovrRig.transform.TransformVector(RightVelocity); } }
    public Vector3 HeadAngularVelocityWorldSpace { get { return !ovrRig ? HeadAngularVelocity : ovrRig.transform.TransformDirection(HeadAngularVelocity); } }
    public Vector3 LeftAngularVelocityWorldSpace { get { return !ovrRig ? LeftAngularVelocity : ovrRig.transform.TransformDirection(LeftAngularVelocity); } }
    public Vector3 RightAngularVelocityWorldSpace { get { return !ovrRig ? RightAngularVelocity : ovrRig.transform.TransformDirection(RightAngularVelocity); } }

    public bool All4TriggerGrips { get { return (LeftTrigger && RightTrigger) || (LeftGrip && RightGrip); } }

    public void Vibrate(bool left, float secs = 0.15f)
    {
        Debug.Log("Vibrating left=" + left);
        var curVib = left ? vibLeft : vibRight;
        if (!curVib)
        {
            curVib = gameObject.AddComponent<Vibrate>();
            if (left) vibLeft = curVib;
            else vibRight = curVib;
        }
        curVib.Left = left;
        curVib.DestroyTimeSecs = secs;
    }

    Vibrate vibLeft = null, vibRight = null;

    void Awake()
    {
        if (Inst && Inst != this)
        {
            if (!DestroyPrevious)
            {
                Destroy(this.gameObject);
                return;
            }
            Destroy(Inst.gameObject);
        }
        Inst = this;
        DontDestroyOnLoad(gameObject);

        (Head = new GameObject("Head-Tracker").transform).parent = transform;
        (LeftHand = new GameObject("Left-Hand-Tracker").transform).parent = transform;
        (RightHand = new GameObject("Right-Hand-Tracker").transform).parent = transform;

        // in the case where none of our Mounted updaters get the message, default to headset is mounted. May need to capture unknown state, and remove Mount-checks in such cases.
        Mounted = true;
    }


    void Start()
    {
        Debug.Log(UnityEngine.XR.XRSettings.supportedDevices.Print());
    }

    private void OnDestroy()
    {
        Debug.Log("Destorying CallenVrWrapper gob=" + gameObject.name);
    }


    void Update()
    {
        if(XRSettings.loadedDeviceName != activeDeviceName)
        {
            activeDeviceName = XRSettings.loadedDeviceName;
            if (activeDeviceName.ToLowerInvariant().Contains("oculus"))
                initOculus();
            else
                initOpenVr();
        }

        updateOculus();

        ReportConnected = Connected;
        ReportMounted = Mounted;

#if UNITY_EDITOR
        if (true)
        {
            drawDebug(true);
            drawDebug(false);
        }
#endif
    }


    OVRCameraRig ovrRig = null;
    Transform ovrHead, ovrLeft, ovrRight;
    bool ovrMountedEventsHooked = false;
    bool skipAnchorUpdate = false;
    string activeDeviceName = null;

    //VelocityIntegrator headVelInt, leftVelInt, rightVelInt, headAngInt, leftAngInt, rightAngInt;
    readonly VelocityIntegrator2 
        headVelInt = new VelocityIntegrator2(), leftVelInt = new VelocityIntegrator2(), rightVelInt = new VelocityIntegrator2(), 
        headAngInt = new VelocityIntegrator2(), leftAngInt = new VelocityIntegrator2(), rightAngInt = new VelocityIntegrator2();
    readonly List<UnityEngine.XR.XRNodeState> xrns = new List<UnityEngine.XR.XRNodeState>();

    float reportInputSys = 1.73f;
    UnityEngine.XR.UserPresenceState presenceState = UnityEngine.XR.UserPresenceState.Unsupported;

    void initOculus()
    {
        headVelInt.WindowOffset = leftVelInt.WindowOffset = rightVelInt.WindowOffset = 0.02f;
        headVelInt.WindowSize = leftVelInt.WindowSize = rightVelInt.WindowSize = 0.05f;
        headAngInt.WindowOffset = leftAngInt.WindowOffset = rightAngInt.WindowOffset = 0.02f;
        headAngInt.WindowSize = leftAngInt.WindowSize = rightAngInt.WindowSize = 0.05f;
    }

    void initOpenVr()
    {
        headVelInt.WindowOffset = leftVelInt.WindowOffset = rightVelInt.WindowOffset = 0.05f;
        headVelInt.WindowSize = leftVelInt.WindowSize = rightVelInt.WindowSize = 0.05f;
        headAngInt.WindowOffset = leftAngInt.WindowOffset = rightAngInt.WindowOffset = 0.05f;
        headAngInt.WindowSize = leftAngInt.WindowSize = rightAngInt.WindowSize = 0.05f;
    }

    void updateOculus()
    {
        Connected = OVRManager.isHmdPresent || UnityEngine.XR.XRDevice.isPresent;
        //if (!Connected)
        //    return;

        // useful info for hmd status: https://gamedev.stackexchange.com/questions/141414/check-if-openvr-steamvr-oculusvr-headset-is-mounted-on-your-head
        // also: https://forums.oculusvr.com/developer/discussion/1648/correct-way-to-detect-the-hardware
        if (!ovrMountedEventsHooked)
        {
            ovrMountedEventsHooked = true;
            OVRManager.HMDMounted += delegate () { Mounted = true; };
            OVRManager.HMDUnmounted += delegate () { Mounted = false; };
        }
        
        if(UnityEngine.XR.XRDevice.userPresence != presenceState)
        {
            presenceState = UnityEngine.XR.XRDevice.userPresence;
            if (presenceState == UnityEngine.XR.UserPresenceState.Present) Mounted = true;
            if (presenceState == UnityEngine.XR.UserPresenceState.NotPresent) Mounted = false;
        }

        //var steamDev = SteamVR_Controller.Input(0);
        //if (steamDev != null && steamDev.hasTracking && !steamDev.uninitialized)
        //    Mounted = SteamVR_Controller.Input(0).GetPress(Valve.VR.EVRButtonId.k_EButton_ProximitySensor);

        if (!ovrRig)
        {
            ovrRig = FindObjectOfType<OVRCameraRig>();
            if (ovrRig)// return;
                ovrRig.UpdatedAnchors += delegate (OVRCameraRig rig)
                  {
                      skipAnchorUpdate = true;
                      rig.leftHandAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
                      rig.rightHandAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
                      rig.leftHandAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
                      rig.rightHandAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
                  };
        }

        if (!skipAnchorUpdate && ovrRig)
        {
            ovrRig.centerEyeAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
            ovrRig.leftEyeAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye);
            ovrRig.rightEyeAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye);
            ovrRig.centerEyeAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
            ovrRig.leftEyeAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftEye);
            ovrRig.rightEyeAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightEye);

            ovrRig.leftHandAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
            ovrRig.rightHandAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
            ovrRig.leftHandAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
            ovrRig.rightHandAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
            skipAnchorUpdate = false;
        }

        if (ovrRig && (!ovrHead || !ovrLeft || !ovrRight))
        {
            ovrHead = ovrRig.centerEyeAnchor;
            ovrLeft = ovrRig.leftHandAnchor;
            ovrRight = ovrRig.rightHandAnchor;
            foreach (var t in ovrRig.GetComponentsInChildren<Transform>())
                if (t.gameObject.name == "CenterEyeAnchor")
                {
                    if (ovrHead && ovrHead != t) Debug.LogWarning("ovrHead mismatch, given " + ovrHead.gameObject.name + " found " + t.gameObject.name);
                    else ovrHead = t;
                }
                else if (t.gameObject.name == "LeftHandAnchor")
                {
                    if (ovrLeft && ovrLeft != t) Debug.LogWarning("ovrLeft mismatch, given " + ovrLeft.gameObject.name + " found " + t.gameObject.name);
                    else ovrLeft = t;
                }
                else if (t.gameObject.name == "RightHandAnchor")
                {
                    if (ovrRight && ovrRight != t) Debug.LogWarning("ovrHead mismatch, given " + ovrRight.gameObject.name + " found " + t.gameObject.name);
                    else ovrRight = t;
                }
            if (!ovrHead || !ovrLeft || !ovrRight)
                Debug.LogWarning("Missing anchors for oculus. head/left/right=" + (ovrHead != null) + "/" + (ovrLeft != null) + "/" + (ovrRight != null));
        }

        if (ovrHead && ovrHead.transform.localPosition != Vector3.zero) CopyTransformPosRot(ovrHead, Head);
        else
        {
            Head.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
            Head.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
        }

        if (ovrLeft && ovrLeft.transform.localPosition != Vector3.zero) CopyTransformPosRot(ovrLeft, LeftHand);
        else
        {
            LeftHand.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
            LeftHand.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
        }
        if (ovrRight && ovrRight.transform.localPosition != Vector3.zero) CopyTransformPosRot(ovrRight, RightHand);
        else
        {
            RightHand.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
            RightHand.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
        }

        //if (steamDev != null)
        //{
        //    LeftHand.localRotation *= Quaternion.Euler(30, 0, 0);
        //    RightHand.localRotation *= Quaternion.Euler(30, 0, 0);
        //}

        if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) || OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
        {
            if (reportInputSys > 0 && ((reportInputSys -= Time.deltaTime) <= 0))
                Debug.LogWarning("Using OVR Input system.");
            doOculusInput();
        }
        else
        {
            if (reportInputSys > 0 && ((reportInputSys -= Time.deltaTime) <= 0))
                Debug.LogWarning("Using XR Input system.");
            doXrInput();
        }

    }

    void doOculusInput()
    {
        LeftTrigger = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
        LeftGrip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        LeftThumbButton1 = OVRInput.Get(OVRInput.Button.Three);
        LeftThumbButton2 = OVRInput.Get(OVRInput.Button.Four);
        LeftThumbTouch = OVRInput.Get(OVRInput.Touch.PrimaryThumbRest | OVRInput.Touch.PrimaryThumbstick | OVRInput.Touch.Three | OVRInput.Touch.Four);
        LeftTriggerTouch = OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger);
        LeftGripTouch = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        RightTrigger = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        RightGrip = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
        RightThumbButton1 = OVRInput.Get(OVRInput.Button.One);
        RightThumbButton2 = OVRInput.Get(OVRInput.Button.Two);
        RightThumbTouch = OVRInput.Get(OVRInput.Touch.SecondaryThumbRest | OVRInput.Touch.SecondaryThumbstick | OVRInput.Touch.One | OVRInput.Touch.Two);
        RightTriggerTouch = OVRInput.Get(OVRInput.Touch.SecondaryIndexTrigger);
        RightGripTouch = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
        if (UseOvrVelocity)
        {
            LeftVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
            RightVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            LeftAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch) * OvrAngularVelMult;
            RightAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch) * OvrAngularVelMult;
        }
        else
        {
            float t = Time.time;
            HeadVelocity = headVelInt.UpdateVelocity(ovrHead.localPosition, t);
            LeftVelocity = leftVelInt.UpdateVelocity(ovrLeft.localPosition, t);
            RightVelocity = rightVelInt.UpdateVelocity(ovrRight.localPosition, t);
            HeadAngularVelocity = headAngInt.UpdateVelocity(ovrHead.localRotation.eulerAngles, t) * Mathf.Deg2Rad;
            LeftAngularVelocity = leftAngInt.UpdateVelocity(ovrLeft.localRotation.eulerAngles, t) * Mathf.Deg2Rad;
            RightAngularVelocity = rightAngInt.UpdateVelocity(ovrRight.localRotation.eulerAngles, t) * Mathf.Deg2Rad;
        }
    }

    void doXrInput()
    {
        bool updLeftVel = false, updRightVel = false, updLeftAng = false, updRightAng = false, updHeadVel = false, updHeadAng = false;

        LeftVelocity = RightVelocity = LeftAngularVelocity = RightAngularVelocity = HeadVelocity = HeadAngularVelocity = Vector3.zero;

        if (UseOvrVelocity)
        {
            // try to get vel/angVel through native xr interface
            xrns.Clear();
            UnityEngine.XR.InputTracking.GetNodeStates(xrns);
            Vector3 v;
            for (int i = 0; i < xrns.Count; i++)
            {
                var type = xrns[i].nodeType;
                switch (type)
                {
                    case UnityEngine.XR.XRNode.LeftHand:
                        if (xrns[i].TryGetVelocity(out v)) { updLeftVel = true; LeftVelocity += v; }
                        if (xrns[i].TryGetAngularVelocity(out v)) { updLeftAng = true; LeftAngularVelocity += v * OpenVrAngularVelMult; }
                        break;
                    case UnityEngine.XR.XRNode.RightHand:
                        if (xrns[i].TryGetVelocity(out v)) { updRightVel = true; RightVelocity += v; Debug.Log("Getting right vel XRNode"); }
                        if (xrns[i].TryGetAngularVelocity(out v)) { updRightAng = true; RightAngularVelocity += v * OpenVrAngularVelMult; }
                        break;
                    case UnityEngine.XR.XRNode.Head:
                        if (xrns[i].TryGetVelocity(out v)) { updHeadVel = true; HeadVelocity += v; }
                        if (xrns[i].TryGetAngularVelocity(out v)) { updHeadAng = true; HeadAngularVelocity += v * OpenVrAngularVelMult; }
                        break;
                    default:
                        break;
                }
            }
            xrns.Clear();
        }

        // if we failed to get values, try to calculate them?
        float t = Time.time;
        if (!updHeadVel) HeadVelocity = headVelInt.UpdateVelocity(ovrHead.localPosition, t);
        if (!updLeftVel) LeftVelocity = leftVelInt.UpdateVelocity(LeftHand.localPosition, t);
        if (!updRightVel) { RightVelocity = rightVelInt.UpdateVelocity(RightHand.localPosition, t); Debug.Log("Getting right vel Integrator"); }
        if (!updHeadAng) HeadAngularVelocity = headAngInt.UpdateVelocity(Head.localRotation.eulerAngles, t) * Mathf.Deg2Rad;
        if (!updLeftAng) LeftAngularVelocity = leftAngInt.UpdateVelocity(LeftHand.localRotation.eulerAngles, t) * Mathf.Deg2Rad;
        if (!updRightAng) RightAngularVelocity = rightAngInt.UpdateVelocity(RightHand.localRotation.eulerAngles, t) * Mathf.Deg2Rad;


        // do buttons! yay!
        float gripRight = //SteamVR_Controller.Input(1).GetPress(Valve.VR.EVRButtonId.k_EButton_Grip) ? 1 : 
            Input.GetAxis("CallenVR_Axis_Press_Grip_Right");
        float gripLeft = //SteamVR_Controller.Input(2).GetPress(Valve.VR.EVRButtonId.k_EButton_Grip) ? 1 : 
            Input.GetAxis("CallenVR_Axis_Press_Grip_Left");

        LeftTrigger = Input.GetAxis("CallenVR_Axis_Press_Trigger_Left") > AxisButtonThreshold;
        LeftTriggerTouch = Input.GetAxis("CallenVR_Axis_Press_Trigger_Left") > 0;
        LeftGrip = gripLeft > AxisButtonThreshold;
        LeftGripTouch = gripLeft > 0;
        LeftThumbButton1 = false; // TODO!
        LeftThumbButton2 = false; // TODO!
        LeftThumbTouch = LeftThumbButton1 || LeftThumbButton2 || Input.GetAxis("CallenVR_Axis_Horz_Thumb_Left") != 0 || Input.GetAxis("CallenVR_Axis_Horz_Thumb_Right") != 0;

        RightTrigger = Input.GetAxis("CallenVR_Axis_Press_Trigger_Right") > AxisButtonThreshold;
        RightTriggerTouch = Input.GetAxis("CallenVR_Axis_Press_Trigger_Right") > 0;
        RightGrip = gripRight > AxisButtonThreshold;
        RightGripTouch = gripRight > 0;
        RightThumbButton1 = false; // TODO!
        RightThumbButton2 = false; // TODO!
        RightThumbTouch = RightThumbButton1 || RightThumbButton2 || Input.GetAxis("CallenVR_Axis_Horz_Thumb_Right") != 0 || Input.GetAxis("CallenVR_Axis_Horz_Thumb_Right") != 0;

        //Debug.Log("left=" + gripLeft + ", right=" + gripRight);

        //for (int i = 0; i < FingerVals.Length; i++)
        //{
        //    FingerVals[i] = Input.GetAxis(string.Format("CallenVR_Axis_Index_{0}_{1}", Fingers[i % Fingers.Length], i < Fingers.Length ? "Left" : "Right"));
        //}
        //Debug.Log("Grips=" + FingerVals.Print());
    }


    // render crosses at each spot the left and right hands we at for the last 0.5s, with colors to indicate grip/trig state
    void drawDebug(bool isLeft)
    {
        var arr = isLeft ? debugLeft : debugRight;
        var startIx = isLeft ? leftVelInt.WindowStartIx : rightVelInt.WindowStartIx;
        var endIx = isLeft ? leftVelInt.WindowEndIx : rightVelInt.WindowEndIx;
        Color c;
        for (int i = arr.Length - 1; i > 0; i--)
        {
            arr[i] = arr[i - 1];
            if (Mathf.Abs(arr[i].w) > Time.unscaledTime - 0.7f)
            {
                c = arr[i].w < 0 ? Color.red : Color.green;
                if (startIx <= i && i <= endIx) c = Color.Lerp(c, Color.blue, 0.5f);
                drawDebugPoint((Vector3)arr[i], c);
            }
        }
        Vector3 pos = isLeft ? LeftHand.position : RightHand.position;
        bool isGrab = isLeft ? (LeftGrip || LeftTrigger) : (RightGrip || RightTrigger);
        arr[0] = new Vector4(pos.x, pos.y, pos.z, Time.unscaledTime * (isGrab ? -1 : 1));

        c = arr[0].w < 0 ? Color.red : Color.green;
        if (startIx <= 0 && 0 <= endIx) c = Color.Lerp(c, Color.blue, 0.5f);
        drawDebugPoint(pos, c);
    }

    void drawDebugPoint(Vector3 p, Color c)
    {
        Debug.DrawLine(p + Vector3.forward * DebugSize, p + Vector3.back * DebugSize, c);
        Debug.DrawLine(p + Vector3.down * DebugSize, p + Vector3.up * DebugSize, c);
        Debug.DrawLine(p + Vector3.left * DebugSize, p + Vector3.right * DebugSize, c);
    }

    Vector4[] debugLeft = new Vector4[100];
    Vector4[] debugRight = new Vector4[100];


    public string PrintXRNode(XRNode node, bool worldSpace=false)
    {
        switch (node)
        {
            case XRNode.LeftEye:
            case XRNode.RightEye:
            case XRNode.CenterEye:
            case XRNode.Head:
                return string.Format("CVR-{0}{1}: mount={2} pos={3} rot={4} vel={5} avel={6}",
                    node == XRNode.Head ? "Head" : node.ToString(),
                    node == XRNode.Head ? "" : "(Head)",
                    Mounted,
                    worldSpace ? Head.position : Head.localPosition,
                    worldSpace ? Head.rotation : Head.localRotation,
                    worldSpace ? HeadVelocityWorldSpace : HeadVelocity,
                    worldSpace ? HeadAngularVelocityWorldSpace : HeadAngularVelocity);
            case XRNode.LeftHand:
                return string.Format("CVR-LHand: grip={0} trig={1} pos={2} rot={3} vel={4} avel={5}",
                    LeftGrip,
                    LeftTrigger,
                    worldSpace ? LeftHand.position : LeftHand.localPosition,
                    worldSpace ? LeftHand.rotation : LeftHand.localRotation,
                    worldSpace ? LeftVelocityWorldSpace : LeftVelocity,
                    worldSpace ? LeftAngularVelocityWorldSpace : LeftAngularVelocity);
            case XRNode.RightHand:
                return string.Format("CVR-RHand: grip={0} trig={1} pos={2} rot={3} vel={4} avel={5}",
                    RightGrip,
                    RightTrigger,
                    worldSpace ? RightHand.position : RightHand.localPosition,
                    worldSpace ? RightHand.rotation : RightHand.localRotation,
                    worldSpace ? RightVelocityWorldSpace : RightVelocity,
                    worldSpace ? RightAngularVelocityWorldSpace : RightAngularVelocity);
            case XRNode.GameController:
            case XRNode.TrackingReference:
            case XRNode.HardwareTracker:
            default:
                return "PrintXRNode: " + node + " Not Supported";
        }
    }


    public static void CopyTransformPosRot(Transform source, Transform dest)
    {
        dest.position = source.position;
        dest.rotation = source.rotation;
    }

    const float DebugSize = 0.02f;
    const float OpenVrAngularVelMult = 10f;
    const float OvrAngularVelMult = 1f;
    const float AxisButtonThreshold = 0.025f;

    static readonly string[] Fingers = new[] { "Index", "Middle", "Ring", "Pinky" };
    static readonly float[] FingerVals = new float[8];

    struct VelocityIntegrator
    {
        public Vector3 updateVelocity(Vector3 currentPos, float currentTime)
        {
            Vector3 delta = currentPos - LastPos, vel0, vel1, vel2, ret;
            float t = currentTime - LastTime;
            if (delta != Vector3.zero)
                LastTime = currentTime;
            LastPos = currentPos;
            vel0 = delta * (1f / Mathf.Max(0.004f, t));
            vel1 = (vel0 + lastVel1) * (1f / 2);
            vel2 = (vel0 + lastVel1 + lastVel2) * (1f / 3);
            lastVel2 = lastVel1;
            lastVel1 = vel0;
            ret = vel0.sqrMagnitude > vel1.sqrMagnitude ? vel0 : vel1;
            ret = ret.sqrMagnitude > vel2.sqrMagnitude ? ret : vel2;
            return ret;
        }
        Vector3 LastPos;
        float LastTime;
        Vector3 lastVel1, lastVel2;
    }

    class VelocityIntegrator2
    {
        public float WindowSize, WindowOffset;

        internal int WindowStartIx { get; private set; }
        internal int WindowEndIx { get; private set; }
        internal readonly Vector4[] Samples = new Vector4[64];

        public Vector3 UpdateVelocity(Vector3 currentPos, float currentTime)
        {
            int i, end;
            for (i = Samples.Length - 1; i > 0; i--)
                Samples[i] = Samples[i - 1];
            Samples[0] = new Vector4(currentPos.x, currentPos.y, currentPos.z, currentTime);
            for (i = 0; i < Samples.Length; i++)
                if (Samples[i].w <= currentTime - WindowOffset)
                    break;
            if(i >= Samples.Length - 1)
            {
                //Debug.LogError("VelocityIntegrator: Not enough samples found for WindowSize=" + WindowSize);
                return Vector3.zero;
            }
            end = i + 1;
            while (end < Samples.Length - 1 && Samples[end].w > Samples[i].w - WindowSize) end++;

            WindowStartIx = i;
            WindowEndIx = end;

            //// at this point we should have 0 <= i < end <= samples.Length-1, so calculate an avg velocity using the deltas between i and end (actually not sure if this is identical to just the delta of i and end)
            //Vector3 sum = Vector3.zero, delta;
            //float td;
            //for (int j = i + 1; j <= end; j++)
            //{
            //    delta = samples[j] - samples[j - 1];
            //    td = samples[j].w - samples[j - 1].w;
            //    sum += delta * (1f / Mathf.Max(0.004f, td));
            //}
            //return sum * (1f / (end - i));
            return ((Vector3)Samples[i] - (Vector3)Samples[end]) * (1f / Mathf.Max(0.004f, Samples[i].w - Samples[end].w));
        }

    }
}
