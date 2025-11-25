using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    private Dictionary<Cell, Item> highlightedCells = new Dictionary<Cell, Item>();
    private List<Cell> highlightedFilledLines = new List<Cell>();

    [SerializeField]
    public OutLine outLinePrefab;

    public Dictionary<Cell, Item> GetHighlightedCells()
    {
        return highlightedCells;
    }
    
    public void HighlightCell(Transform cell, Item item)
    {
        var cellComponent = cell.GetComponent<Cell>();
        if (cellComponent != null && item != null)
        {
            cellComponent.HighlightCell(item.itemTemplate);
            if (!highlightedCells.ContainsKey(cellComponent))
            {
                highlightedCells.Add(cellComponent, item);
            }
        }
    }
    
    public void ClearAllHighlights()
    {
        foreach (var kvp in highlightedCells)
        {
            if (kvp.Key != null)
            {
                kvp.Key.ClearCell();
            }
        }
        highlightedCells.Clear();
        
        ClearFilledLinesHighlight();
    }
    
    //public void HighlightFillBothRowAndCol(List<Cell> cells, ItemTemplate itemTemplate)
    //{
    //    foreach (var cell in highlightedFilledLines.ToList())
    //    {
    //        if (cell != null)
    //        {
    //            cell.ClearCell();
    //            highlightedFilledLines.Remove(cell);
    //        }
    //    }
    //    foreach (var cell in cells)
    //    {
    //        if (cell != null && itemTemplate != null && !highlightedCells.ContainsKey(cell) && !highlightedFilledLines.Contains(cell))
    //        {
    //            cell.HighlightCellFill(itemTemplate);
    //            highlightedFilledLines.Add(cell);
    //        }
    //    }
    //}

    public void HighlightFill(List<Cell> cells, ItemTemplate itemTemplate)
    {
        var cellsToKeep = new HashSet<Cell>(cells);
        
        foreach (var cell in highlightedFilledLines.ToList())
        {
            if (cell != null && !highlightedCells.ContainsKey(cell) && !cellsToKeep.Contains(cell))
            {
                cell.ClearCell();
                highlightedFilledLines.Remove(cell);
            }
        }
        
        foreach (var cell in cells)
        {
            if (cell != null && itemTemplate != null && !highlightedCells.ContainsKey(cell) && !highlightedFilledLines.Contains(cell))
            {
                cell.HighlightCellFill(itemTemplate);
                highlightedFilledLines.Add(cell);
            }
        }
    }

    public void ClearFilledLinesHighlight()
    {
        foreach (var cell in highlightedFilledLines)
        {
            if (cell != null && !highlightedCells.ContainsKey(cell))
            {
                cell.ClearCell();
            }
        }
        highlightedFilledLines.Clear();
    }

    ////unfinished///
    //public void ShowOutLine(Vector3 pos)
    //{
    //    var outline = PoolObject.GetObject(outLinePrefab.gameObject);
    //    var outLineComponent = outline.GetComponent<OutLine>();
    //    outLineComponent.SetRowOrCol(pos);
    //}

    public void OnDragEndedWithoutPlacement()
    {
    }

    public void ClearData()
    {
        highlightedCells.Clear();
        highlightedFilledLines.Clear();
    }
}

