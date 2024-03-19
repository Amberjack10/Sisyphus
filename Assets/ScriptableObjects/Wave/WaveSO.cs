using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSO")]
public class WaveSO : ScriptableObject
{
    public int[] normal;
    public int[] elite;
    public int[] boss;

    public int maxEnemyCnt = 50;

    public int numPerSpawn;
    public float spawnDelay;

    public int GetAllLength()
    {
        return normal.Length + elite.Length + boss.Length;
    }
}
