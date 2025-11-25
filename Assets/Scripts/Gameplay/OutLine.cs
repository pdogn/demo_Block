using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

////unfinished///
public class OutLine : MonoBehaviour
{
    public ItemTemplate itemTemplate;
    public Image itemColor;
    public RectTransform rect;

    private void Update()
    {
        rect = transform.GetComponent<RectTransform>();
    }

    public void UpdateColor(ItemTemplate _itemTemplate)
    {
        this.itemTemplate = _itemTemplate;
        itemColor.color = _itemTemplate.itemColor;
    }

    public void SetRowOrCol(Vector3 pos, bool isRow = true)
    {
        rect.position = pos;
        rect.rotation = rect.rotation = Quaternion.Euler(0, 0, 0f);
        if(isRow == false)
        {
            rect.rotation = rect.rotation = Quaternion.Euler(0, 0, 90f);
        }
    }
}
