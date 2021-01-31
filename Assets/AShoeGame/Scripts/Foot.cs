using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foot : MonoBehaviour
{
    [Header("Parent to the shoe they will be wearing to start"),Tooltip("Parent to the shoe they will be wearing to start")]
    public Transform OtherFoot;

    internal ShoeDef TargetShoe;

    public bool Complete { get; private set; }
    public bool Success { get; private set; }

    public Collider Coll;

    ShoeDef visualShoe;

    void Start()
    {
        if (!Coll) Coll = GetComponent<Collider>();
    }

    private void Update()
    {
        if(!visualShoe && TargetShoe)
        {
            visualShoe = Instantiate(TargetShoe, OtherFoot);
            visualShoe.transform.localPosition = new Vector3(-0.132f, 0, 0.028f);
            visualShoe.transform.localRotation *= Quaternion.Euler(0, 70, -90);
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if (Complete)
            return;

        var shoe = c.gameObject.GetComponent<ShoeDef>();
        if (shoe)
        {
            complete(shoe);
        }

    }

    void complete(ShoeDef otherShoe)
    {
        Complete = true;
        Success = otherShoe.ShoeName == TargetShoe.name;

        GameObject particles = null;

        if (Success)
        {
            NpcManager.Instance.Score++;
            particles = Instantiate(NpcManager.Instance.ParticleSuccess, transform.position, Quaternion.identity).gameObject;
            StartCoroutine(xferShoe(otherShoe));
        }
        else
        {
            particles = Instantiate(NpcManager.Instance.ParticleFailure, transform.position, Quaternion.identity).gameObject;
            var grab = otherShoe.GetComponent<CallenVrGrabbable>();
            var rb = otherShoe.GetComponent<Rigidbody>();
            if (grab) grab.Release();
            if (rb) rb.AddForce(new Vector3(0, 4, -7));
        }

        Destroy(particles, 5);
    }

    IEnumerator xferShoe(ShoeDef s)
    {
        s.State = ShoeDef.ShoeState.OnNpcTargetFoot;
        yield return null;
        yield return null;
        s.transform.parent = transform;
        s.transform.localPosition = new Vector3(-0.132f, 0, 0.028f);
        s.transform.localRotation = TargetShoe.transform.localRotation * Quaternion.Euler(0, 70, -90);        
    }

}
