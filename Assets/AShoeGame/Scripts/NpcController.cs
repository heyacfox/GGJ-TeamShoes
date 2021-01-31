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
    float setSpeed = 0;
    float navPriTimer = 0;

    void Awake()
    {
        nav = gameObject.AddComponent<NavMeshAgent>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = NpcManager.Instance.voiceMixerGroup;
        audioSource.spatialBlend = 1.0f;
        nav.speed = setSpeed = setSpeed > 0 ? setSpeed : 1.5f;
        nav.angularSpeed = 720f;
        nav.radius = 0.37f;

        foot = buildFootScripts();

    }


    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        anim.SetFloat("WalkSpeed", setSpeed);
        startPos = transform.position;
        //GetComponent<DynamicCharacterAvatar>().ForceUpdate(true, true, true);
    }

    void Update()
    {
        updateState();
        if (lastState != State)
        {
            onStateChanged();
            lastState = State;
        }

        if ((navPriTimer -= Time.deltaTime) < 0)
        {
            nav.avoidancePriority = Random.Range(40, 60);
            navPriTimer = Random.value * 1.5f;
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
                StartCoroutine(leaveAfterDelay(1.7f));
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

                updateNavRad(startPos);

                if ((transform.position - nav.destination).sqrMagnitude < 0.05f || (!nav.pathPending && nav.isStopped))
                    State = States.WaitAtCounter;
                break;
            case States.WaitAtCounter:
                updateNavRad(transform.position);
                transform.position = OccupiedDestination.position;

                transform.LookAt(Vector3.left * 0.5f);
                if (!foot) foot = GetComponentInChildren<Foot>();
                if(foot && foot.Complete)
                {
                    State = foot.Success ? States.Success : States.Failure;
                    nav.isStopped = false;
                }
                break;
            case States.Success:
                break;
            case States.Failure:
                break;
            case States.Leaving:
                
                updateNavRad(Vector3.forward);

                if ((transform.position - nav.destination).sqrMagnitude < 0.05f || (!nav.pathPending && nav.isStopped))
                {
                    Debug.Log("NPC done. gob=" + gameObject.name + ", start=" + startPos + ", pos=" + transform.position + ", nav=" + nav.destination + ", pathpend=" + nav.pathPending + ", pathStop=" + nav.isStopped);
                    State = States.Done;
                }
                break;
            case States.Done:
                break;
            default:
                break;
        }
    }

    internal void ChangeSpeed(float v)
    {
        if (v == setSpeed)
            return;
        setSpeed = v;
        nav.speed = v;
        if (anim) anim.SetFloat("WalkSpeed", v);
    }

    float lastRad = -1;

    void updateNavRad(Vector3 startPoint)
    {
        Vector3 posDelta = (transform.position - nav.destination);
        Vector3 startDelta = (transform.position - startPoint);
        Vector3 minDelta = posDelta.sqrMagnitude < startDelta.sqrMagnitude ? posDelta : startDelta;
        float rad = Mathf.Clamp(minDelta.magnitude / 5, 0.15f, 0.75f);
        if (rad != lastRad) nav.radius = rad;
        lastRad = rad;
    }

    IEnumerator leaveAfterDelay(float secs)
    {
        yield return new WaitForSeconds(secs);

        nav.SetDestination(startPos);
        while ((transform.position - OccupiedDestination.position).sqrMagnitude < 1)
            yield return null;
        OccupiedDestination = null;
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
        box.size = new Vector3(0.5f, 0.15f, 0.25f);
        box.isTrigger = true;
        return foot;
    }

    public void setSoundLists(List<AudioClip> askSounds, List<AudioClip> rewardSounds)
    {
        this.askSounds = askSounds;
        this.rewardSounds = rewardSounds;
    }

}
