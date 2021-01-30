using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcTest : MonoBehaviour
{

    public enum State
    {
        None,
        Approach,
        AtCounter,
        Leaving
    }

    [Header("Read only")]
    public State CurrentState;

    NavMeshAgent nav;
    State state = State.None;
    Animator anim;
    float animWaitTime;
    Vector3 startPos = Vector3.zero;
    Vector3 lastPos = Vector3.zero;

    static readonly Vector3 TestDest = new Vector3(0, 0, 1.9f);

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        Vector3 targetDest = new Vector3(TestDest.x, transform.position.y, TestDest.z);
        //anim.SetFloat("Speed", (lastPos - transform.position).magnitude / Time.deltaTime);
        switch (state)
        {
            case State.None:
                startPos = transform.position;
                if ((animWaitTime -= Time.deltaTime) < 0)
                    state = State.Approach;
                break;
            case State.Approach:
                if (nav.destination != targetDest)
                    nav.SetDestination(targetDest);
                if (!nav.pathPending && (nav.isStopped || (nav.transform.position - targetDest).sqrMagnitude < (0.1f * 0.1f)))
                {
                    state = State.AtCounter;
                    anim.SetTrigger("LegUp");
                    animWaitTime = 2.8f + Random.value * 2f;
                }
                break;
            case State.AtCounter:
                transform.LookAt(Vector3.left);
                if ((animWaitTime -= Time.deltaTime) < 0)
                {
                    animWaitTime = 1.9f;
                    state = State.Leaving;
                    anim.SetTrigger("LegDown");
                }
                break;
            case State.Leaving:
                if (animWaitTime >= 0 && (animWaitTime -= Time.deltaTime) < 0)
                {
                    nav.SetDestination(startPos);
                    state = State.None;
                    animWaitTime = 10 + Random.value * 5;
                }
                break;
            default:
                break;
        }
        CurrentState = state;
    }
}
