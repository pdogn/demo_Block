using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    protected static readonly Dictionary<string, PoolObject> pools = new();

    public GameObject prefab;

    protected Queue<GameObject> pool = new();

    public virtual void Awake()
    {
        if (prefab != null)
        {
            SetPrefab(prefab);
        }
    }

    public void SetPrefab(GameObject newPrefab)
    {
        pools[newPrefab.name] = this;
        prefab = newPrefab;
    }

    private void OnDestroy()
    {
        pools.Clear();
    }

    protected GameObject Create()
    {
        var item = Instantiate(prefab, transform);
        item.name = prefab.name;
        return item;
    }

    private GameObject Get()
    {
        var item = pool.Count == 0 ? Create() : pool.Dequeue();
        if (item.activeSelf && pool.Count > 0)
        {
            item = pool.Dequeue();
        }

        item.SetActive(true);
        return item;
    }

    public static void Return(GameObject item)
    {
        if (item == null)
        {
            return;
        }

        item.SetActive(false);
        if (pools.TryGetValue(item.name, out var pool))
        {
            pool.pool.Enqueue(item);
        }
    }

    public static GameObject GetObject(GameObject prefab)
    {
        return GetPool(prefab);
    }

    public static GameObject GetObject(GameObject prefab, Vector3 position)
    {
        var item = GetPool(prefab);
        item.transform.position = position;
        return item;
    }

    private static GameObject GetPool(GameObject prefab)
    {
        var prefabName = prefab.name;
        if (pools.TryGetValue(prefabName, out var pool))
        {
            return pool.Get();
        }

        var poolObject = new GameObject(prefabName).AddComponent<PoolObject>();
        poolObject.transform.SetParent(GameObject.Find("Pool").transform);
        poolObject.prefab = prefab;
        poolObject.transform.localScale = Vector3.one;
        pools.Add(prefabName, poolObject);
        return poolObject.Get();
    }

}
