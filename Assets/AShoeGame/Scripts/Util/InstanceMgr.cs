using UnityEngine;
using System.Collections.Generic;

/// <summary>Manages a recycled pool of game objects that use the activeSelf property to 
/// determine if they are in-use or ready to be reused.</summary>
class InstanceMgr<T> where T : Component
{
    public InstanceMgr(GameObject instObjPrefab) : this(instObjPrefab, null) { }
    public InstanceMgr(GameObject instObjPrefab, Transform instParent)
    {
        if ((InstancePrefab = instObjPrefab).GetComponent<T>() == null)
            throw new UnityException("InstanceMgr given prefab object that lacked component type " + typeof(T) + ": " + instObjPrefab.name);
        InstanceParent = instParent;
    }

    public readonly GameObject InstancePrefab;
    public Transform InstanceParent;

    public T GetNewInstance()
    {
        T ret;
        int c = instances.Count, i = 0;
        if (c > 0)
            for (i = (lastInactive + 1) % c; i != lastInactive; i = (i + 1) % c)
                if (!instances[i].gameObject.activeSelf)
                    break;
        if (c == 0 || (i == lastInactive && instances[i].gameObject.activeSelf))
        {
            instances.Add(ret = (Object.Instantiate(InstancePrefab) as GameObject).GetComponent<T>());
            lastInactive = c;
            listDirty = true;
        }
        else 
            ret = instances[lastInactive = i];
        //instanceIxs[ret] = lastInactive;
        ret.gameObject.SetActive(true);
        if (InstanceParent != null) 
            ret.transform.parent = InstanceParent;
        return ret;
    }

    //public void DisableInstance(T inst)
    //{
    //    //int ix;
    //    //if (!instanceIxs.TryGetValue(inst, out ix))
    //    //    throw new UnityException("Object " + inst.gameObject.name + " cannot be disabled, as it was not created with this InstanceMgr");
    //    inst.gameObject.SetActive(false);
    //}

    public List<T> GetAllInstances()
    {
        if (listDirty)
        {
            allTmp.Clear();
            for (int i = 0, c = instances.Count; i < c; i++)
                allTmp.Add(instances[i]);
            listDirty = false;
        }
        return allTmp;
    }

    //public void GetActiveInstances(List<T> destList)
    //{
    //    destList.Clear();
    //    for (int i = 0, c = instances.Count; i < c; i++)
    //        if (instances[i].gameObject.activeSelf)
    //            destList.Add(instances[i]);
    //}

    public void DisableAll()
    {
        foreach (var itm in instances)
            itm.gameObject.SetActive(false);
    }

    public void DestroyAll()
    {
        for (int i = 0, c = instances.Count; i < c; i++)
            Object.Destroy(instances[i]);
        instances.Clear();
        //instanceIxs.Clear();
        listDirty = true;
    }

    readonly List<T> instances = new List<T>();
    //readonly Dictionary<T, int> instanceIxs = new Dictionary<T, int>();
    readonly List<T> allTmp = new List<T>(); //used to get all items

    int lastInactive = 0;
    bool listDirty = false;

}
