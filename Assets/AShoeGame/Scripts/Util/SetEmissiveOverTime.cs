using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetEmissiveOverTime : MonoBehaviour {

    public Gradient EmissiveColor = new Gradient() { colorKeys = new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) } };
    public AnimationCurve HdrMultiplier = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

    public float CycleTime = 1;

    public bool UseRealtime;

    float t = 0;


    void Update()
    {
        t += UseRealtime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (CycleTime <= 0)
            return;

        t = (t % CycleTime);

        var rend = GetComponent<Renderer>();
        var mat = rend ? rend.material : null;
        if (!mat)
            return;

        mat.SetColor("_EmissionColor", EmissiveColor.Evaluate(t / CycleTime) * HdrMultiplier.Evaluate(t / CycleTime));        
    }
}
