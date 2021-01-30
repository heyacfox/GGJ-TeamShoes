using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Possibly my most useful script ever - use editor to hook events for each part of a game object's lifecycle! (Except Update)
public class UnityLifecycleEventHooks : MonoBehaviour
{
    // Wrap the UnityEvents in a struct so we can collapse them in the editor
    [System.Serializable] public struct LifecycleEventHook { public UnityEvent EventCallbacks; }

    public LifecycleEventHook AwakeEvent, EnableEvent, StartEvent, DisableEvent, DestroyEvent;

    void Awake() { AwakeEvent.EventCallbacks.Invoke(); }

    void Start() { StartEvent.EventCallbacks.Invoke(); }

    void OnEnable() { EnableEvent.EventCallbacks.Invoke(); }

    void OnDisable() { DisableEvent.EventCallbacks.Invoke(); }

    void OnDestroy() { DestroyEvent.EventCallbacks.Invoke(); }
}