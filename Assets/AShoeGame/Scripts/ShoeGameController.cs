using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script manages the initialization of the game, management of NPC spawns
public class ShoeGameController : MonoBehaviour
{
    public static ShoeGameController Instance { get; private set; }

    public ShoeDef GlassSlipperShoe;
    public ShoeDef[] AllShoes;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
