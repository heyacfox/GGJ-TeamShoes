using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeLooseMonitor : MonoBehaviour
{
    internal ShoeStackManager Stack;

    Color startColor;
    float rndTimer = 0.05f;


    private void Awake()
    {
        startColor = GetComponentInChildren<MeshRenderer>().material.color;   
    }

    void Update()
    {
        if ((rndTimer -= Time.deltaTime) > 0)
            return;
        rndTimer = Random.value * 0.2f;

        if (!Stack)
            return;
        var shoe = GetComponent<ShoeDef>();
        if (!shoe)
            return;

        bool isLoose = Stack.IsShoeLoose(shoe);

        var rend = GetComponentInChildren<MeshRenderer>();
        rend.material.color = isLoose ? startColor : Color.Lerp(startColor, Color.red, 0.5f);

        var grab = GetComponent<CallenVrGrabbable>();
        if (grab) grab.enabled = isLoose;
        
    }
}
