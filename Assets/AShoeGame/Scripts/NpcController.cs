using System;
using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
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


    const string LeftFootName = "LeftFoot", RightFootName = "RightFoot";

    Vector3 startPos;
    States lastState = (States)(-1);
    NavMeshAgent nav;
    Animator anim;
    Foot foot;

    List<AudioClip> askSounds;
    List<AudioClip> rewardSounds;
    AudioSource audioSource;

    void Awake()
    {
        nav = gameObject.AddComponent<NavMeshAgent>();
        audioSource = gameObject.AddComponent<AudioSource>();
        nav.speed = 1.5f;
        nav.angularSpeed = 720f;
        nav.radius = 0.37f;

        foot = buildFootScripts();

    }


    void Start()
    {
        startPos = transform.position;
        anim = GetComponentInChildren<Animator>();
        GetComponent<DynamicCharacterAvatar>().ForceUpdate(true, true, true);
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
                nav.isStopped = true;
                audioSource.PlayOneShot(askSounds[UnityEngine.Random.Range(0, askSounds.Count)]);
                break;
            case States.Success:
                anim.SetTrigger("LegDown");
                audioSource.PlayOneShot(rewardSounds[UnityEngine.Random.Range(0, rewardSounds.Count)]);
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
                    nav.isStopped = false;
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

    private Foot buildFootScripts()
    {
        Transform left = null, right = null;
        foreach (var tf in GetComponentsInChildren<Transform>())
        {
            if (tf.gameObject.name == LeftFootName) left = tf;
            else if (tf.gameObject.name == RightFootName) right = tf;
        }
        var foot = left.gameObject.AddComponent<Foot>();
        foot.OtherFoot = right;
        var box = left.gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(0.08f, 0.06f, 0.3f);
        box.isTrigger = true;
        return foot;
    }

    public void setSoundLists(List<AudioClip> askSounds, List<AudioClip> rewardSounds)
    {
        this.askSounds = askSounds;
        this.rewardSounds = rewardSounds;
    }

}
