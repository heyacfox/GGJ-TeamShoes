using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoSet : MonoBehaviour
{
    public List<Texture2D> Photographs = new List<Texture2D>();

    void Start()
    {
        if (Photographs.Count > 0)
            foreach (var ps in GetComponentsInChildren<PhotoSwap>())
            {
                int ix = Random.Range(0, Photographs.Count);
                ps.SetPicture(Photographs[ix]);
                if (Photographs.Count > 1) Photographs.RemoveAt(ix);
            }
        
    }
}
