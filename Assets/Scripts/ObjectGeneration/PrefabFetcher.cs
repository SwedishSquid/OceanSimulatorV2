using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PrefabFetcher
{
    private GameObject[] objectPrefabs;

    private HashSet<string> skipPrefabNames = new HashSet<string> {
        "beer",
        "bottlePotionPoison",
        "boulderSpike",
        "boxGift",
        "cage",
        "chair",
        "chalice",
        "char04",
        "chestB",
        "cog1",
        "coin",
        "coinProcJam",
        "crown",
        "cupMetal",
        "diceA",
        "diceB",
        "doorA",
        "doorB",
        "egg01",
        "egg03",
        "flagA",
        "flagB",
        "gem",
        "hammer",
        "heart",
        "helmetViking",
        "key",
        "ladder",
        "lock",
        "map",
        "paintBrush",
        "pike",
        "pillar01",
        "pipe",
        "player1",
        "player4",
        "pot01",
        "pot02",
        "scissor",
        "shield",
        "skull",
        "suitCase",
        "sword",
        "table",
        "torch",
        "wallSpike01",
        "wallSpike02",
        "wallWindow",
        "wizardHat",
        "woodenBarrel",
    };

    public PrefabFetcher()
    {
        objectPrefabs = Resources.LoadAll<GameObject>("ObjectPrefabs");
        Debug.Log($"found {objectPrefabs.Length} object prefabs");
    }

    public List<GameObject> FetchAllPrefabs()
    {
        var fetched = objectPrefabs
            .Where(x => !skipPrefabNames.Contains(x.name))
            .ToList();

        Debug.Log($"fetched {fetched.Count} object prefabs");

        return fetched;
    }
}
