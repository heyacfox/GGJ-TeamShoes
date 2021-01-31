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
            text.text = "Tips: $0.00";
        else
        {
            //With a 5 stack: multiply by 25 to get 29 score to 7.25
            //with a 6 stack, multiply by 14.5 to get all 50 score
            int scoreToCents = (int) (NpcManager.Instance.Score * 25f);
            decimal moneyAmount = ((decimal)scoreToCents) / 100;
            text.text = string.Format(ScoreFormat, moneyAmount);
            //text.text = string.Format(ScoreFormat, NpcManager.Instance.Score * 25);
        }
    }
}
