using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Item item;
    private CanvasGroup group;
    public bool busy;
    private ItemTemplate saveTemplate;
    private BoxCollider2D _boxCollider2D;
    private Item originalItem;
    private Item customItem;

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        group = item.GetComponent<CanvasGroup>();
    }

    public void InitItem()
    {
        item.name = "Item " + name;
        StartCoroutine(UpdateItem());
    }
    private IEnumerator UpdateItem()
    {
        yield return new WaitForSeconds(0.1f);
        _boxCollider2D.size = transform.GetComponent<RectTransform>().sizeDelta;
        item.transform.position = transform.position;
    }
    
    public bool IsEmpty()
    {
        return !busy;
    }
    
    
    public void FillCell(ItemTemplate itemTemplate)
    {
        if (itemTemplate != null && item != null)
        {
            item.UpdateColor(itemTemplate);
            busy = true;
            
            if (group != null)
            {
                group.alpha = 1f;
            }
            
            if (!item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(true);
            }
        }
    }

    public void HighlightCell(ItemTemplate itemTemplate)
    {
        if (originalItem != null)
        {
            if (customItem != null)
            {
                Destroy(customItem.gameObject);
                customItem = null;
            }
            //item = originalItem;
            item.gameObject.SetActive(true);
            originalItem = null;
            group = item.GetComponent<CanvasGroup>();
        }

        item.FillIcon(itemTemplate);
        group.alpha = 0.3f;
    }

    public void HighlightCellFill(ItemTemplate itemTemplate)
    {
        if (itemTemplate != null && item != null)
        {
            if (saveTemplate == null)
            {
                saveTemplate = item.itemTemplate;
            }
            
            item.FillIcon(itemTemplate);
            
            if (group != null)
            {
                group.alpha = 1f;
            }
        }
    }

    public void ClearCell()
    {
        if (originalItem != null)
        {
            item = originalItem;
            item.gameObject.SetActive(true);
            originalItem = null;
            group = item.GetComponent<CanvasGroup>();
        }

        item.transform.localScale = Vector3.one;
        
        if (saveTemplate != null && !busy)
        {
            item.UpdateColor(saveTemplate);
            saveTemplate = null;
        }
        
        if (saveTemplate == null && !busy)
        {
            group.alpha = 0;
            busy = false;
        }
        else if (saveTemplate != null && busy)
        {
            FillCell(saveTemplate);
            saveTemplate = null;
        }
    }
    
    public void ClearFilledCell()
    {
        busy = false;
        saveTemplate = null;
        
        //if (item != null)
        //{
        //    item.gameObject.SetActive(false);
        //}
        
        if (group != null)
        {
            group.alpha = 0;
        }
    }
}
