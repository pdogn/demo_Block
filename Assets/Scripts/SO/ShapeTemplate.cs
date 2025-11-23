using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShapeRow
{
    public bool[] cells = new bool[5];
}

[CreateAssetMenu(fileName = "Shape", menuName = "BlockSo/Items/Shape", order = 1)]
public class ShapeTemplate : ScriptableObject
{
    public ShapeRow[] rows = new ShapeRow[5];

    private void OnEnable()
    {
        if (rows == null || rows.Length != 5)
        {
            rows = new ShapeRow[5];
            for (var i = 0; i < 5; i++)
            {
                rows[i] = new ShapeRow();
            }
        }
    }
}
