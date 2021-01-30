using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Possibly my most useful script ever - use editor to hook events for each physics interaction
public class UnityPhysicsEventHooks : MonoBehaviour
{
    // Wrap the UnityEvents in a struct so we can collapse them in the editor
    [System.Serializable] public struct PhysicsEventHook { public UnityEvent EnterEvent, ExitEvent, StayEvent; }


    public PhysicsEventHook Collision, Trigger;

    public bool LogEnter, LogExit, LogStay;

    private void OnCollisionEnter(Collision collision) { Collision.EnterEvent.Invoke(); if (LogEnter) Debug.Log("Collide-Enter with " + collision.gameObject.name, this); }

    private void OnCollisionEnter2D(Collision2D collision) { Collision.EnterEvent.Invoke(); if (LogEnter) Debug.Log("Collide-Enter with " + collision.gameObject.name, this); }


    private void OnCollisionStay(Collision collision) { Collision.StayEvent.Invoke(); if (LogStay) Debug.Log("Collide-Stay with " + collision.gameObject.name, this); }

    private void OnCollisionStay2D(Collision2D collision) { Collision.StayEvent.Invoke(); if (LogStay) Debug.Log("Collide-Stay with " + collision.gameObject.name, this); }


    private void OnCollisionExit(Collision collision) { Collision.ExitEvent.Invoke(); if (LogExit) Debug.Log("Collide-Exit with " + collision.gameObject.name, this); }

    private void OnCollisionExit2D(Collision2D collision) { Collision.ExitEvent.Invoke(); if (LogExit) Debug.Log("Collide-Exit with " + collision.gameObject.name, this); }




    private void OnTriggerEnter(Collider other) { Trigger.EnterEvent.Invoke(); if (LogEnter) Debug.Log("Trigger-Enter with " + other.gameObject.name, this); }

    private void OnTriggerEnter2D(Collider2D collision) { Trigger.EnterEvent.Invoke(); if (LogEnter) Debug.Log("Trigger-Enter with " + collision.gameObject.name, this); }


    private void OnTriggerStay(Collider other) { Trigger.StayEvent.Invoke(); if (LogStay) Debug.Log("Trigger-Stay with " + other.gameObject.name, this); }

    private void OnTriggerStay2D(Collider2D collision) { Trigger.StayEvent.Invoke(); if (LogStay) Debug.Log("Trigger-Stay with " + collision.gameObject.name, this); }


    private void OnTriggerExit(Collider other) { Trigger.ExitEvent.Invoke(); if (LogExit) Debug.Log("Trigger-Exit with " + other.gameObject.name, this); }

    private void OnTriggerExit2D(Collider2D collision) { Trigger.ExitEvent.Invoke(); if (LogExit) Debug.Log("Trigger-Exit with " + collision.gameObject.name, this); }
}
