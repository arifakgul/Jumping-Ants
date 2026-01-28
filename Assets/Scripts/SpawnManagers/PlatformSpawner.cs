using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using RangeAttribute = UnityEngine.RangeAttribute;

public class PlatformSpawner : MonoBehaviour
{
    public static event Action<float, Vector3> OnMeteorRowDetected;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] lanes;
    [SerializeField] private Transform[] lanesForDouble;

    [Header("Patterns")]
    [SerializeField] private List<PatternData> allPatterns = new List<PatternData>();

    [Header("General Spawn")]
    [SerializeField] private float spawnAhead = 12f;
    [SerializeField] private float defaultSpacing = 2f;
    [SerializeField] private float minSpacing = 1.1f; // en az bu kadar boşluk olcak aralarında.
    [SerializeField] private float randomSpacing = 0.18f;

    [Header("CleanUp")]
    [SerializeField] private Camera cam;
    [SerializeField] private float cleanUpOffSet = 2f;

    [Header("Poin Settings")]
    [SerializeField] [Range(0f, 1f)] private float poinSpawnChance = 0.3f;
    [SerializeField] private float poinYOffset = 1.5f;

    private float nextY;
    private List<GameObject> activePlatforms = new List<GameObject>();
    private GameManager gm;

    void Start()
    {
        if (player != null)
            nextY = player.position.y - 0.2f;

        gm = GameManager.Instance;
    }

    void Update()
    {
        if (gm == null || !gm.IsGameActive())
        return;
        float cameraTop = (cam != null) ? cam.transform.position.y + cam.orthographicSize : player.position.y + spawnAhead;
        while (cameraTop + spawnAhead > nextY)
        {
            SpawnNextPattern();
        }

        CleanUpBelowCamera();
    }

    // =============== Spawners ===============
    private void SpawnNextPattern()
    {
        PatternDifficulty difficulty = GetDifficulty(player.position.y);
        var availablePatterns = allPatterns.FindAll(p => p.difficulty == difficulty);
        if (availablePatterns.Count == 0) return;

        var pattern = availablePatterns[UnityEngine.Random.Range(0, availablePatterns.Count)];
        if (pattern == null || pattern.rows == null || pattern.rows.Length == 0) return;

        foreach (var row in pattern.rows)
        {
            if (row == null || row.platforms == null || row.platforms.Length == 0)
                continue;

            float spacing = Mathf.Max(minSpacing, defaultSpacing);
            float jitter = UnityEngine.Random.Range(-randomSpacing, randomSpacing);
            nextY += spacing + jitter;

            foreach (var Pdata in row.platforms)
            {
                if (Pdata == null) continue; 
                if (Pdata.lane < 0 || Pdata.lane >= lanes.Length) continue;

                float spawnX = 0f;
                if (Pdata.type == PlatformType.Double)
                {
                    if (lanesForDouble != null && Pdata.lane < lanesForDouble.Length && lanesForDouble[Pdata.lane] != null)
                    {
                        spawnX = lanesForDouble[Pdata.lane].position.x;
                    }
                }
                else
                {
                    if (Pdata.lane < lanes.Length)
                        spawnX = lanes[Pdata.lane].position.x;
                }

                Vector3 pos = new Vector3(spawnX, nextY, 0f);
                var go = ObjectPool.Instance.SpawnFromPool(Pdata.type.ToString(), pos, Quaternion.identity);
                activePlatforms.Add(go);

                if (Pdata.hasMeteor)
                {
                    float standartLaneX = lanes[Pdata.lane].position.x;
                    Vector3 meteorTargetPos = new Vector3(standartLaneX, nextY, 0f);
                    OnMeteorRowDetected?.Invoke(nextY, meteorTargetPos);   
                }

                if (!Pdata.hasMeteor && UnityEngine.Random.value < poinSpawnChance)
                {
                    Vector3 poinPos = new Vector3(spawnX, nextY + poinYOffset, 0f);
                    GameObject poinObj = ObjectPool.Instance.SpawnFromPool("Poin", poinPos,Quaternion.identity);
                    if (poinObj != null)
                    {
                        activePlatforms.Add(poinObj);                    
                    }
                }
            }
        }
    }

    private PatternDifficulty GetDifficulty(float y)
    {
        if (y <= 50f) return PatternDifficulty.Easy;
        else if (y <= 300f) return PatternDifficulty.Middle;
        else return PatternDifficulty.Hard;
    }

    // =============== Clean Up ===============
    private void CleanUpBelowCamera()
    {
        if (cam == null) return;
        float bottomY = cam.transform.position.y - cam.orthographicSize - cleanUpOffSet;

        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            var g = activePlatforms[i];
            if (g == null) { activePlatforms.RemoveAt(i); continue; }
            if (g.transform.position.y < bottomY)
            {
                string tag = g.name.Replace("(Clone)", "").Trim();
                ObjectPool.Instance.ReturnToPool(tag, g);
                activePlatforms.RemoveAt(i);
            }
        }
    }
}