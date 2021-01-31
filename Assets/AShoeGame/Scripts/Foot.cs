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
            visualShoe.transform.localRotation = Quaternion.Euler(0, 70, -90);
        }
    }

    void OnTriggerEnter(Collider c)
    {
        var shoe = c.gameObject.GetComponent<ShoeDef>();
        if (shoe)
        {
            //if (shoe.FootOnShoe)
            //    return;

            // If the other shoe is our type
            if (shoe.ShoeName == TargetShoe.name)
            {
                Complete = true;
                Success = true;
            }
            else
                Complete = true;
        }

    }



}
