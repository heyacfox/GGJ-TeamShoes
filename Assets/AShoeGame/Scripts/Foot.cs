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

    //[SerializeField] ShoeType footShoeType;
    //[SerializeField] List<ShoeDef> shoes;

    // May not be needed but added JIC
    [SerializeField] GameObject activeShoeOnFoot;

    public Collider Coll;

    void Start()
    {
        if (!Coll) Coll = GetComponent<Collider>();

        // Ensure that the npc doesn't have a foot active on start.
        //foreach (var shoe in shoes)
        //    shoe.gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.GetComponent<ShoeDef>())
        {
            ShoeDef collShoe = collision.gameObject.GetComponent<ShoeDef>();

            if (collShoe.FootOnShoe)
                return;

            // If the other shoe is our type
            if (collShoe.ShoeName == TargetShoe.name)
            {
                // Call events on the shoe that the player is holding (e.g. destroy)
                //collShoe.SuccessfulFit();
                Complete = true;
                Success = true;

                // Set the shoe on our foot active
                //foreach (var shoe in shoes)
                //{
                //    if (shoe.Type == footShoeType)
                //    {
                //        shoe.gameObject.SetActive(true);
                //        activeShoeOnFoot = shoe.gameObject;
                //    }
                //}
            }
            else
                //collShoe.UnSuccessfulFit();
                Complete = true;
        }

    }



}
