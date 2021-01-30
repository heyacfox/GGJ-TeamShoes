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
                        shoes[i, j, k].gameObject.name = shoePrefab.name + " - " + i + "," + j + "," + k;
                        shoes[i, j, k].transform.localRotation *= Quaternion.Euler(0, Random.value < 0.5f ? -90 : 90, 0);
                        shoes[i, j, k].GetComponent<Rigidbody>().isKinematic = true;
                        var debug = shoes[i, j, k].gameObject.AddComponent<ShoeLooseMonitor>();
                        debug.Stack = this;
                        int ii = i, jj = j, kk = k;
                        StartCoroutine(finishGrabLogic(i, j, k));
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

    private ShoeDef getNextShoe(int i, int j, int k)
    {
        if (Depth > 2 && i == 0 && j == Depth / 2 && k == 0 && ShoeGameController.Instance.GlassSlipperShoe)
            return ShoeGameController.Instance.GlassSlipperShoe;

        // todo: make this smarter, save ordering, for use with npc generation
        var allShoes = ShoeGameController.Instance.AllShoes;
        return allShoes[UnityEngine.Random.Range(0, allShoes.Length)];
    }

    private bool testIsLoose(ShoeDef testShoe, int i, int j, int k)
    {
        if (testShoe.gameObject.name.Contains("3,1,0"))
            Debug.Log("here");
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

    IEnumerator finishGrabLogic(int i, int j, int k)
    {
        yield return null;
        var grab = shoes[i, j, k].GetComponent<CallenVrGrabbable>();
        try
        {
            if (grab.OnGrab == null) grab.OnGrab = new UnityEngine.Events.UnityEvent();
            grab.OnGrab.AddListener(delegate () { shoes[i, j, k] = null; });
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }

    }

}
