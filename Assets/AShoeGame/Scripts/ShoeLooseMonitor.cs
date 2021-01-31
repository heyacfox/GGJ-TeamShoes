using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeLooseMonitor : MonoBehaviour
{
    internal ShoeStackManager Stack;

    Color startColor;
    float rndTimer = 0.05f;

    GameObject blockBox;
    int counter;
    bool isLoose = false;
    Color blockingColor;

    const int BlockBoxFrameDelay = 10;

    private void Awake()
    {
        //startColor = GetComponentInChildren<MeshRenderer>().material.color; 
        counter = 0;// Random.Range(0, 1234);
    }

    void Update()
    {

        if (blockBox && !isLoose)
        {
            Color c = new Color(blockingColor.r, blockingColor.g, blockingColor.b, 0.15f + 0.25f * 0.5f * (1 + Mathf.Sin(Time.time * 5f)));
            blockBox.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
        }

        if ((rndTimer -= Time.deltaTime) > 0)
            return;
        rndTimer = Random.value * 0.07f + 0.04f;

        if (!Stack)
            return;
        var shoe = GetComponent<ShoeDef>();
        if (!shoe)
            return;

        isLoose = Stack.IsShoeLoose(shoe);

        //var rend = GetComponentInChildren<MeshRenderer>();
        //rend.material.color = isLoose ? startColor : Color.Lerp(startColor, Color.red, 0.5f);
        if (!isLoose && !blockBox && Stack.BlockingVisual)
        {
            blockBox = Instantiate(Stack.BlockingVisual);
            blockingColor = blockBox.GetComponent<MeshRenderer>().material.color;
            blockBox.transform.localScale = Stack.Spacing * 0.9f;
            blockBox.transform.parent = transform;
            blockBox.transform.localPosition = Vector3.up * 0.02f;
        }

        if (blockBox) blockBox.GetComponent<MeshRenderer>().enabled = !isLoose;// (!isLoose && (counter % BlockBoxFrameDelay == 0));
        counter++;


        var grab = GetComponent<CallenVrGrabbable>();
        if (grab) grab.enabled = isLoose;
        
    }
}
