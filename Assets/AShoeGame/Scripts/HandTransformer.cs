using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTransformer : MonoBehaviour
{
    public GameObject BoxHand;
    public GameObject pumpkinObject;
    public GameObject slipperObject;


    public void WinGame()
    {
        BoxHand.SetActive(false);
        slipperObject.SetActive(true);
    }

    public void LoseGame()
    {
        BoxHand.SetActive(false);
        pumpkinObject.SetActive(true);
    }
}
