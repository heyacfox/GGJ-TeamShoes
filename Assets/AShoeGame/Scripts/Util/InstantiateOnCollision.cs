using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstantiateOnCollision : MonoBehaviour
{
    static InstantiateOnCollision() { SceneManager.sceneUnloaded += delegate (Scene s) { cachedInstMgr.Clear(); }; }

    public Transform InstantiateSource;

    public bool RotateToNormal;

    static readonly Dictionary<Transform, InstanceMgr<Transform>> cachedInstMgr = new Dictionary<Transform, InstanceMgr<Transform>>();

    void OnCollisionEnter(Collision coll)
    {
        Vector3 posAvg = new Vector3(), normAvg = new Vector3();
        for (int i = 0; i < coll.contacts.Length; i++)
        {
            posAvg += coll.contacts[i].point;
            normAvg += coll.contacts[i].normal;
        }
        normAvg *= 1f / coll.contacts.Length;
        posAvg *= 1f / coll.contacts.Length;
        Debug.Log("normal is " + normAvg);
        var t = getInstance();
        if (RotateToNormal)
            t.rotation = Quaternion.LookRotation(normAvg);
        t.position = posAvg;
    }

    Transform getInstance()
    {
        InstanceMgr<Transform> instMgr;
        if (!cachedInstMgr.TryGetValue(InstantiateSource, out instMgr))
            cachedInstMgr.Add(InstantiateSource, instMgr = new InstanceMgr<Transform>(InstantiateSource.gameObject, InstantiateSource.parent));
        return instMgr.GetNewInstance();
    }
}
