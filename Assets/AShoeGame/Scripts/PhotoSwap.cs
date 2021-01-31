using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoSwap : MonoBehaviour
{
    public MeshRenderer Renderer;

    public void SetPicture(Texture2D tex)
    {
        Renderer.material.mainTexture = tex;
    }
}
