using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns npcs according to SpawnRate anim curve, tracks game time and score.
public class NpcManager : MonoBehaviour
{
    public static NpcManager Instance { get; private set; }

    public int Score = 0;

    public NpcGenerator CharacterGenerator;
    public NPCSoundLists npcSoundLists;


    [Header("Max rate, x = Time.time, y = time between spawns"), Tooltip("Max rate, x = Time.time, y = time between spawns")]
    public AnimationCurve SpawnRate = AnimationCurve.Linear(0, 1, 300, 1);

    [Header("Particle Prefabs")]
    public ParticleSystem ParticleSuccess;
    public ParticleSystem ParticleFailure;

    Transform[] spawns, dests;

    List<NpcController> npcs = new List<NpcController>();
    
    float spawnCheckDelay = 0;
    float timer = 0;

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

    ShoeGameController.RandomShoeBin bin;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if((spawnCheckDelay -= Time.deltaTime) < 0)
        {
            // before trying to spawn, look for completed nav agents to remove from our checks
            for (int i = 0; i < npcs.Count; i++)
                if (npcs[i].State == NpcController.States.Done)
                    npcs.RemoveAt(i--);

            int rndOffset = Random.Range(0, spawns.Length);
            for (int i = 0; i < spawns.Length; i++)
            {
                int ii = (i + rndOffset) % spawns.Length;
                if (shouldSpawn(ii))
                {
                    if (bin == null) bin = ShoeGameController.Instance.CreateRandomBin();
                    var shoe = bin.GetRandomShoe();
                    //var shoe = ShoeGameController.Instance.AllShoes[Random.Range(0, ShoeGameController.Instance.AllShoes.Length)];
                    spawnOne(ii, shoe);
                    break;
                }
            }
            spawnCheckDelay = Mathf.Min(1f / SpawnRate.Evaluate(timer), 30); // converts update timer to a 'delay until next spawn', using the anim curve
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
        if (avatar.activeRace.name.Contains("Female"))
        {
            npc.setSoundLists(npcSoundLists.askSoundsFemale, npcSoundLists.rewardSoundsFemale);
        } else
        {
            npc.setSoundLists(npcSoundLists.askSoundsMale, npcSoundLists.rewardSoundsMale);
        }
        
        npcs.Add(npc);
    }

    // spawn an npc when there are no npcs within 3m of the spawn point
    bool shouldSpawn(int spawnIndex)
    {
        if (npcs.Count > 100)
            return false; // circuit breaker if it starts generating too many ppl

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
