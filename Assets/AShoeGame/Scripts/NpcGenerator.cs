using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UnityEngine;

public class NpcGenerator : MonoBehaviour
{
    // first version will just choose from pre-made uma characters, but a randomizer would be nice to add later!

    [Header("Searches for all avatars as children of these"), Tooltip("Searches for all avatars as children of these")]
    public Transform[] CharacterSources;

    readonly List<DynamicCharacterAvatar> chars = new List<DynamicCharacterAvatar>();

    private void Awake()
    {
        for (int i = 0; i < CharacterSources.Length; i++)
        {
            var avatar = CharacterSources[i].GetComponent<DynamicCharacterAvatar>();
            if (avatar)
            {
                chars.Add(avatar);
                continue;
            }
            foreach (var a in CharacterSources[i].GetComponentsInChildren<DynamicCharacterAvatar>(true))
                chars.Add(a);
        }
    }

    public DynamicCharacterAvatar GenerateCharacter(bool maleAllowed, bool femaleAllowed)
    {
        DynamicCharacterAvatar ret = null;
        for (int i = 0; i < 10; i++)
        {
            ret = generate(maleAllowed, femaleAllowed);
            if (ret)
                break;
        }
        if (!ret) ret = generate(maleAllowed, femaleAllowed, true);

        return ret;
    }

    DynamicCharacterAvatar generate(bool m, bool f, bool force = false)
    {
        var ret = Instantiate(chars[Random.Range(0, chars.Count)]);
        ret.gameObject.SetActive(true);
        var foot = ret.GetComponentInChildren<Foot>();
        if (!foot)
        {
            Debug.LogError("No foot script found on npc=" + ret.gameObject.name);
            return force ? ret : null;
        }
        bool isFem = ret.activeRace.name.Contains("Female");
        if (m && !isFem) return ret;
        if (f && isFem) return ret;
        return force ? ret : null;
    }

}
