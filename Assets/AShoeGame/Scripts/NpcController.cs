using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    public enum States
    {
        Init = 0,
        Approaching,
        WaitAtCounter,
        Success,
        Failure,
        Leaving,
        Done
    }

    public States State = States.Init;

    internal Transform OccupiedDestination = null;

    public ShoeDef TargetShoe { get { return foot ? foot.TargetShoe : null; } set { if (foot) foot.TargetShoe = value; } }

    Vector3 startPos;
    States lastState = (States)(-1);
    NavMeshAgent nav;
    Animator anim;
    Foot foot;

    void Awake()
    {
        nav = gameObject.AddComponent<NavMeshAgent>();
        nav.speed = 1.5f;
        nav.angularSpeed = 720f;

        if (!(foot = GetComponentInChildren<Foot>()))
            Debug.LogError("No foot found on NPC = " + gameObject.name);
    }

    void Start()
    {
        startPos = transform.position;
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        updateState();
        if (lastState != State)
        {
            onStateChanged();
            lastState = State;
        }
    }

    void onStateChanged()
    {
        switch (State)
        {
            case States.Init:
                break;
            case States.Approaching:
                nav.SetDestination(new Vector3(OccupiedDestination.position.x, transform.position.y, OccupiedDestination.position.z));
                break;
            case States.WaitAtCounter:
                anim.SetTrigger("LegUp");
                break;
            case States.Success:
                anim.SetTrigger("LegDown");
                StartCoroutine(leaveAfterDelay(1.5f));
                break;
            case States.Failure:
                anim.SetTrigger("LegDown");
                StartCoroutine(leaveAfterDelay(1.8f));
                break;
            case States.Leaving:
                nav.SetDestination(startPos);
                break;
            case States.Done:
                break;
            default:
                break;
        }
    }

    void updateState()
    {
        switch (State)
        {
            case States.Init:
                OccupiedDestination = NpcManager.Instance.FindOpenDestination(this);
                if (OccupiedDestination)
                    State = States.Approaching;
                break;
            case States.Approaching:
                if ((transform.position - nav.destination).sqrMagnitude < 0.05f || (!nav.pathPending && nav.isStopped))
                    State = States.WaitAtCounter;
                break;
            case States.WaitAtCounter:
                if (!foot) foot = GetComponentInChildren<Foot>();
                if(foot && foot.Complete)
                {
                    State = foot.Success ? States.Success : States.Failure;
                    foot.enabled = false;
                }
                break;
            case States.Success:
                break;
            case States.Failure:
                break;
            case States.Leaving:
                if ((transform.position - nav.destination).sqrMagnitude < 0.05f || (!nav.pathPending && nav.isStopped))
                    State = States.Done;
                break;
            case States.Done:
                break;
            default:
                break;
        }
    }

    IEnumerator leaveAfterDelay(float secs)
    {
        yield return new WaitForSeconds(secs);
        State = States.Leaving;
    }
}
