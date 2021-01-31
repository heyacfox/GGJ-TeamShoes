using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script manages the initialization of the shoes. npcs and main game logic actually handled by NpcManager (sorry)
public class ShoeGameController : MonoBehaviour
{
    public static ShoeGameController Instance { get; private set; }

    public ShoeDef GlassSlipperShoe;
    public ShoeDef[] AllShoes;

    public ShoeStackManager ShoeStack;

    public RandomShoeBin CreateRandomBin()
    {
        return new RandomShoeBin(this);
    }

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        if (!ShoeStack) ShoeStack = GetComponent<ShoeStackManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class RandomShoeBin
    {
        internal RandomShoeBin(ShoeGameController gc)
        {
            SGC = gc;
        }

        public readonly ShoeGameController SGC;
        readonly List<ShoeDef> bin = new List<ShoeDef>();

        public ShoeDef GetRandomShoe()
        {
            if(bin.Count == 0)
            {
                for (int i = 0; i < SGC.AllShoes.Length; i++)
                    if (SGC.AllShoes[i])
                        bin.Add(SGC.AllShoes[i]);
            }
            if (bin.Count == 0)
                return null;
            int rnd = Random.Range(0, bin.Count);
            var ret = bin[rnd];
            bin.RemoveAt(rnd);
            return ret;
        }
    }
}
