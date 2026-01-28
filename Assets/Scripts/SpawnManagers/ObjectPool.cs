using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 20;
        public bool expandable = true;
    }

    [SerializeField] private List<PoolEntry> pools = new List<PoolEntry>();

    private  Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();
    private  Dictionary<string, PoolEntry> poolInfo = new Dictionary<string, PoolEntry>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var entry in pools)
        {
            if (entry == null || entry.prefab == null || string.IsNullOrEmpty(entry.tag))
                continue;

            poolInfo[entry.tag] = entry;

            var q = new Queue<GameObject>();
            for (int i = 0; i < Mathf.Max(1, entry.initialSize); i++)
            {
                var go = CreateInstance(entry);
                go.SetActive(false);
                q.Enqueue(go);
            }
            poolDict[entry.tag] = q;
        }
    }

    private GameObject CreateInstance(PoolEntry entry)
    {
        var go = Instantiate(entry.prefab, transform);
        go.name = entry.tag;
        return go;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(tag))
        {
            return null;
        }

        var q = poolDict[tag];
        GameObject obj = null;
        if (q.Count > 0)
        {
            obj = q.Dequeue();
        }
        else
        {
            var info = poolInfo[tag];
            if (info.expandable)
            {
                obj = CreateInstance(info);
            }
            else
            {
                Debug.Log($"Pool '{tag}' empty and not expandable.");
                return null;
            }
        }

        obj.transform.SetLocalPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(string tag, GameObject go)
    {
        if (go == null) return;

        if (!poolDict.ContainsKey(tag))
        {
            var fallback = go.name.Replace("(Clone)", "").Trim();
            if (!poolDict.ContainsKey(fallback))
            {
                Destroy(go);
                return;
            }
            tag = fallback;
        }

        go.SetActive(false);
        go.transform.SetParent(transform);
        poolDict[tag].Enqueue(go);
    }
}