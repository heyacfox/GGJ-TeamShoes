using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugShoeLoose : MonoBehaviour
{
    internal ShoeStackManager Stack;

    Color startColor;
    float rndTimer = 0.05f;


    private void Awake()
    {
        startColor = GetComponent<MeshRenderer>().material.color;   
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

        var rend = GetComponent<MeshRenderer>();
        rend.material.color = isLoose ? startColor : Color.Lerp(startColor, Color.red, 0.5f);
        
    }
}
