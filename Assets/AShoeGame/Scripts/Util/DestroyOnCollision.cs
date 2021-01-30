using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{


    public LayerMask DestroyLayers = ~0;

    public float DestroyDelay = 0;

    public bool PrintLogs = true;

    public Transform[] UnparentBeforeDestroy;

    public UnityEngine.Events.UnityEvent OnDestroy;

    bool destroying = false;

    void Start() { }

    private void OnEnable()
    {
        if (PrintLogs) Debug.Log("Enabled Destroy");
    }

    private void OnDisable()
    {
        if (PrintLogs) Debug.Log("Disabled Destroy");
    }

    void OnCollisionEnter(Collision coll)
    {
        if (PrintLogs) Debug.Log("collided. " + enabled + ", " + ((1 << coll.gameObject.layer) & DestroyLayers));
        if (!enabled || ((1 << coll.gameObject.layer) & DestroyLayers) == 0)
            return;
        destroy();
    }

    void OnCollisionStay(Collision coll)
    {
        if (PrintLogs) Debug.Log("collided. " + enabled + ", " + ((1 << coll.gameObject.layer) & DestroyLayers));
        if (!enabled || ((1 << coll.gameObject.layer) & DestroyLayers) == 0)
            return;
        destroy();
    }

    void destroy()
    {
        if (destroying) return;

        if (UnparentBeforeDestroy != null)
            for (int i = 0; i < UnparentBeforeDestroy.Length; i++)
                if (UnparentBeforeDestroy[i]) UnparentBeforeDestroy[i].parent = null;

        try { if (OnDestroy != null) OnDestroy.Invoke(); }
        catch { }

        destroying = true;
        Destroy(gameObject, DestroyDelay);
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }
}
