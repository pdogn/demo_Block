using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class Item : MonoBehaviour
{
    public ItemTemplate itemTemplate;
    public Image itemColor;

    private Vector2Int position;

    private void Awake()
    {
        if (itemTemplate != null)
        {
            UpdateColor(itemTemplate);
        }
    }

    public void UpdateColor(ItemTemplate _itemTemplate)
    {
        this.itemTemplate = _itemTemplate;
        itemColor.color = _itemTemplate.itemColor;
    }

    public void FillIcon(ItemTemplate iconScriptable)
    {
        UpdateColor((ItemTemplate)iconScriptable);
    }

    public void SetPosition(Vector2Int vector2Int)
    {
        position = vector2Int;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }

    public void SetTransparency(float alpha)
    {
        var color = itemColor.color;
        color.a = alpha;
        itemColor.color = color;

        //UpdateEnableColors(itemTemplate);
    }
}
