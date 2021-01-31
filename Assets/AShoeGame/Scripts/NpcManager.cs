using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager Instance { get; private set; }


    public NpcGenerator CharacterGenerator;

    Transform[] spawns, dests;

    List<NpcController> npcs = new List<NpcController>();
    float spawnCheckDelay = 0;


    public Transform FindOpenDestination(NpcController npc)
    {
        List<Transform> openDests = new List<Transform>(dests);
        for (int i = 0; i < npcs.Count && openDests.Count > 0; i++)
        {
            if(npcs[i].OccupiedDestination)
                for (int j = 0; j < openDests.Count; j++)
                    if(openDests[j] == npcs[i].OccupiedDestination)
                    {
                        openDests.RemoveAt(j);
                        break;
                    }
        }
        if (openDests.Count == 0)
            return null;
        return openDests[Random.Range(0, openDests.Count)];
    }


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spawns = getPoints("SpawnPoints");
        dests = getPoints("CounterTargets");
    }

    // Update is called once per frame
    void Update()
    {
        if((spawnCheckDelay -= Time.deltaTime) < 0)
        {
            for (int i = 0; i < spawns.Length; i++)
            {
                if (shouldSpawn(i))
                {
                    var shoe = ShoeGameController.Instance.AllShoes[Random.Range(0, ShoeGameController.Instance.AllShoes.Length)];
                    spawnOne(i, shoe);
                }
            }
            spawnCheckDelay = 0.5f;
        }
    }

    Transform[] getPoints(string rootPath)
    {
        var root = transform.Find(rootPath);
        Transform[] ret = new Transform[root.childCount];
        for (int i = 0; i < ret.Length; i++)
            ret[i] = root.GetChild(i);
        return ret;
    }

    void spawnOne(int spawnIndex, ShoeDef targetShoe)
    {
        bool maleAllow = targetShoe.Sex != ShoeDef.ShoeSex.FemaleOnly;
        bool femAllow = targetShoe.Sex != ShoeDef.ShoeSex.MaleOnly;
        var avatar = CharacterGenerator.GenerateCharacter(maleAllow, femAllow);
        avatar.gameObject.SetActive(true);
        avatar.transform.position = spawns[spawnIndex].position;
        var npc = avatar.gameObject.AddComponent<NpcController>();
        npc.TargetShoe = targetShoe;
        npcs.Add(npc);
    }

    // spawn an npc when there are no npcs within 3m of the spawn point
    bool shouldSpawn(int spawnIndex)
    {
        const float BufferDist = 3;

        var pt = spawns[spawnIndex];
        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i].State >= NpcController.States.WaitAtCounter)
                continue;
            if ((npcs[i].transform.position - pt.position).sqrMagnitude < BufferDist * BufferDist)
                return false;
        }
        return true;
    }
}
