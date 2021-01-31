using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    public string ScoreFormat;

    void Update()
    {
        var text = GetComponent<Text>();
        if (!text)
            return;
        if (!NpcManager.Instance || NpcManager.Instance.Score == 0)
            text.text = "Pay: $0.00";
        else
        {
            int scoreToCents = NpcManager.Instance.Score * 25;
            decimal moneyAmount = ((decimal)scoreToCents) / 100;
            text.text = string.Format(ScoreFormat, moneyAmount);
            //text.text = string.Format(ScoreFormat, NpcManager.Instance.Score * 25);
        }
    }
}
