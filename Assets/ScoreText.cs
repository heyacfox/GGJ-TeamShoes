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
            text.text = "";
        else
            text.text = string.Format(ScoreFormat, NpcManager.Instance.Score);
    }
}
