using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class InitAmountPool : PoolObject
{
    [SerializeField]
    private int initialCapacity;

    public override void Awake()
    {
        base.Awake();
        for (var i = 0; i < initialCapacity; i++)
        {
            var item = Create();
            item.SetActive(false);
            pool.Enqueue(item);
        }
    }
}
