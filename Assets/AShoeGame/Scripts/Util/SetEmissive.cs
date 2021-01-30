using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetEmissive : MonoBehaviour
{
    public Color EmissiveColor = Color.white;
    public float HDRMultiplier = 1;

    Color setColor = Color.clear;
    float hdrMult = float.NegativeInfinity;


    void Update()
    {
        if(EmissiveColor != setColor || HDRMultiplier != hdrMult)
        {
            setColor = EmissiveColor;
            hdrMult = HDRMultiplier;

            var rend = GetComponent<Renderer>();
            var mat = rend ? rend.material : null;
            if(mat) mat.SetColor("_EmissionColor", EmissiveColor * HDRMultiplier);
        }
    }
}
