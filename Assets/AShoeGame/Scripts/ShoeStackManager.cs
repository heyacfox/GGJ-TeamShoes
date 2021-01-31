using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeStackManager : MonoBehaviour
{
    [Range(1, 10)]
    public int Depth = 4;

    public Vector3 Spacing = Vector3.one;

    ShoeDef[,,] shoes = new ShoeDef[0, 0, 0]; // order is layer, major axis (size = Depth), minor axis (size = round(Depth/2))

    Vector3 lastSpacing = Vector3.zero;

    Dictionary<string, ShoeDef> nameToShoe = new Dictionary<string, ShoeDef>();

    public bool IsShoeLoose(ShoeDef shoe)
    {
        for (int i = 0; i < shoes.GetLength(0); i++)
            for (int j = 0; j < shoes.GetLength(1); j++)
                for (int k = 0; k < shoes.GetLength(2); k++)
                    if(shoe == shoes[i, j, k])
                    {
                        return testIsLoose(shoes[i, j, k], i, j, k);
                    }
        return true; // didn't find the shoe, so its loose i guess
    }

    public ShoeDef[] GetAllShoesRandom()
    {
        if (shoes.GetLength(0) == 0)
            return null;

        ShoeDef glassShoe = null; // save for end of list
        ShoeDef firstShoe = null; // 1st shoe is always first returned
        List<ShoeDef> ret1 = new List<ShoeDef>(); // 2nd thru 5th shoes in one semi-randomized collection
        List<ShoeDef> ret2 = new List<ShoeDef>(); // remaining shoes fully randomized

        for (int i = shoes.GetLength(0)-1; i >= 0; i--)
            for (int j = 0; j < shoes.GetLength(1); j++)
                for (int k = 0; k < shoes.GetLength(2); k++)
                {
                    if (!shoes[i, j, k])
                        continue;

                    if (ShoeGameController.Instance.GlassSlipperShoe && shoes[i, j, k].ShoeName == ShoeGameController.Instance.GlassSlipperShoe.ShoeName)
                        glassShoe = nameToShoe[shoes[i, j, k].ShoeName];
                    else if (firstShoe == null)
                        firstShoe = nameToShoe[shoes[i, j, k].ShoeName];
                    else if (ret1.Count < 4)
                        ret1.Add(nameToShoe[shoes[i, j, k].ShoeName]);
                    else
                        ret2.Add(nameToShoe[shoes[i, j, k].ShoeName]);
                }

        //to form the final list, 1. shuffle ret1, 2. insert first shoe, 3. shuffle-append ret2, 4. append glass shoe at end
        for (int i = 0; i < 20; i++)
        {
            int rnd1 = Random.Range(0, ret1.Count);
            int rnd2 = Random.Range(0, ret1.Count);
            var temp = ret1[rnd1];
            ret1[rnd1] = ret1[rnd2];
            ret1[rnd2] = temp;
        }
        
        if (firstShoe) ret1.Insert(0, firstShoe);

        while(ret2.Count > 0)
        {
            int rnd = Random.Range(0, ret2.Count);
            ret1.Add(ret2[rnd]);
            ret2.RemoveAt(rnd);
        }

        if (glassShoe) ret1.Add(glassShoe);

        return ret1.ToArray();
    }

    void Awake()
    {
        if (Spacing == Vector3.zero) Spacing = 0.05f * Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        if (Depth != shoes.GetLength(0))
            rebuildStack(true);
        else if (Spacing != lastSpacing)
            rebuildStack(false);
        lastSpacing = Spacing;
    }

    private void rebuildStack(bool depthChange)
    {
        if (depthChange)
        {
            // destroy old gameobjects
            foreach (var s in shoes) if (s) Destroy(s.gameObject);
            // resize array
            shoes = new ShoeDef[Depth, Depth, (int)((Depth + 1) / 2f)];
            // triple loop to fill the stack in a pyramid-ish shape
            for (int i = 0; i < Depth; i++)
                for (int j = 0; j < Depth - i; j++)
                    for (int k = 0; k < ((Depth - i) / 2f); k++)
                    {
                        var shoePrefab = getNextShoe(i, j, k);
                        shoes[i, j, k] = Instantiate(shoePrefab, transform);
                        shoes[i, j, k].gameObject.name = shoePrefab.name;
                        shoes[i, j, k].transform.localRotation *= Quaternion.Euler(0, Random.value < 0.5f ? -90 : 90, 0);
                        shoes[i, j, k].State = ShoeDef.ShoeState.OnStack;
                        StartCoroutine(finishSetup(i, j, k));
                    }

        }
        for (int i = 0; i < Depth; i++)
            for (int j = 0; j < Depth - i; j++)
                for (int k = 0; k < ((Depth - i) / 2f); k++)
                    if (shoes[i, j, k]) shoes[i, j, k].transform.localPosition = getShoePosition(i, j, k);
    }

    private Vector3 getShoePosition(int i, int j, int k)
    {
        float x = (Depth - i) * (-0.5f) + j;
        float y = i;
        float z = k + 0.5f * ((i + Depth + 1) % 2);
        return Vector3.Scale(Spacing, new Vector3(x, y, z));
    }

    ShoeGameController.RandomShoeBin bin;

    private ShoeDef getNextShoe(int i, int j, int k)
    {
        if (Depth > 2 && i == 0 && j == Depth / 2 && k == 0 && ShoeGameController.Instance.GlassSlipperShoe)
        {
            nameToShoe[ShoeGameController.Instance.GlassSlipperShoe.ShoeName] = ShoeGameController.Instance.GlassSlipperShoe;
            return ShoeGameController.Instance.GlassSlipperShoe;
        }

        if (bin == null) bin = ShoeGameController.Instance.CreateRandomBin();
        var ret = bin.GetRandomShoe();
        nameToShoe[ret.ShoeName] = ret; // save for faster lookups in getallshoesrandom
        return ret;
        // todo: make this smarter, save ordering, for use with npc generation
        //var allShoes = ShoeGameController.Instance.AllShoes;
        //return allShoes[UnityEngine.Random.Range(0, allShoes.Length)];
    }

    private bool testIsLoose(ShoeDef testShoe, int i, int j, int k)
    {
        if (i < Depth - 1)
        {  // check layers above until we're at the top
            bool looseA = testIsLoose(testShoe, i + 1, j, k);
            bool looseB = j == 0 ? true : testIsLoose(testShoe, i + 1, j - 1, k);
            bool looseC = (i % 2 != 0) || k == 0 ? true : testIsLoose(testShoe, i + 1, j, k - 1);
            bool looseD = (i % 2 != 0) || j == 0 || k == 0 ? true : testIsLoose(testShoe, i + 1, j - 1, k - 1);
            if (!looseA || !looseB || !looseC || !looseD)
                return false;
        }
        return shoes[i, j, k] == null || shoes[i, j, k] == testShoe;
    }

    IEnumerator finishSetup(int i, int j, int k)
    {
        yield return null;
        if (shoes[i, j, k].GetComponent<Rigidbody>()) shoes[i, j, k].GetComponent<Rigidbody>().isKinematic = true;
        var debug = shoes[i, j, k].gameObject.AddComponent<ShoeLooseMonitor>();
        debug.Stack = this;

        var grab = shoes[i, j, k].GetComponent<CallenVrGrabbable>();
        if (grab.OnGrab == null) grab.OnGrab = new UnityEngine.Events.UnityEvent();
        grab.OnGrab.AddListener(delegate () { shoes[i, j, k] = null; });
    }
}
