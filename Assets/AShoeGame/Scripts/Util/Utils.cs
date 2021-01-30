using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{


    public static float FrustrumHeightAtDistance(float distance, float camFoV)
    {
        var frustumHeight = 2.0f * distance * Mathf.Tan(camFoV * 0.5f * Mathf.Deg2Rad);
        return frustumHeight;
    }

    public static IEnumerator SetTimeout(float time, System.Action callback, bool useRealTime = false)
    {
        for (float t = 0; t < time; t += (useRealTime ? Time.unscaledDeltaTime : Time.deltaTime))
            yield return null;
        callback();
    }

    /// <summary> Reset a transform's local position, rotation, and scale. (Set to identity matrix) </summary>
    public static void Reset(this Transform t) { Reset(t, true, true, true); }
    /// <summary> Reset a transform's local position, rotation, and/or scale. </summary>
    public static void Reset(this Transform t, bool pos, bool rot, bool scale)
    {
        if (pos) t.localPosition = Vector3.zero;
        if (rot) t.localRotation = Quaternion.identity;
        if (scale) t.localScale = Vector3.one;
    }

}
