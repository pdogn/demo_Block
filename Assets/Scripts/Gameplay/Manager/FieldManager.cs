using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public RectTransform field;
    public Cell prefab;

    public Cell[,] cells;


    private void Start()
    {
        GenerateField();
    }

    public void GenerateField()
    {
        foreach (Transform child in field)
        {
            Destroy(child.gameObject);
        }
        cells = new Cell[8, 8];

        // Create cells
        for (var i = 0; i < 8; i++)
        {
            for (var j = 0; j < 8; j++)
            {
                var cell = Instantiate(prefab, field);
                cells[i, j] = cell;
                cell.name = $"Cell {i}, {j}";
                cell.InitItem();
            }
        }
    }

    public Cell[,] GetAllCells()
    {
        return cells;
    }

    public Cell[] GetEmptyCells()
    {
        return cells.Cast<Cell>().Where(cell => !cell.busy).ToArray();
    }
    
    public float GetCellSize()
    {
        if (cells != null && cells.GetLength(0) > 0 && cells[0, 0] != null)
        {
            var rectTransform = cells[0, 0].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.rect.width;
            }
        }
        return 100f; // Default size
    }
    
    public List<Cell> GetFilledLines(bool checkRows, Dictionary<Cell, Item> highlightedCells)
    {
        List<Cell> filledLines = new List<Cell>();
        
        if (cells == null || highlightedCells == null || highlightedCells.Count == 0)
        {
            return filledLines;
        }
        
        // Tạo một bản sao của trạng thái busy hiện tại và thêm các cell sẽ được fill
        bool[,] tempBusy = new bool[cells.GetLength(0), cells.GetLength(1)];
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                tempBusy[i, j] = cells[i, j].busy;
            }
        }
        
        // Đánh dấu các cell sẽ được fill
        foreach (var kvp in highlightedCells)
        {
            if (kvp.Key != null)
            {
                // Tìm vị trí của cell trong mảng cells
                for (int i = 0; i < cells.GetLength(0); i++)
                {
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        if (cells[i, j] == kvp.Key)
                        {
                            tempBusy[i, j] = true;
                            break;
                        }
                    }
                }
            }
        }
        
        if (checkRows)
        {
            // Kiểm tra các hàng (rows)
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                bool isFull = true;
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    if (!tempBusy[i, j])
                    {
                        isFull = false;
                        break;
                    }
                }
                
                if (isFull)
                {
                    // Thêm tất cả các cell trong hàng này
                    for (int j = 0; j < cells.GetLength(1); j++)
                    {
                        filledLines.Add(cells[i, j]);
                    }
                }
            }
        }
        else
        {
            // Kiểm tra các cột (columns)
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                bool isFull = true;
                for (int i = 0; i < cells.GetLength(0); i++)
                {
                    if (!tempBusy[i, j])
                    {
                        isFull = false;
                        break;
                    }
                }
                
                if (isFull)
                {
                    // Thêm tất cả các cell trong cột này
                    for (int i = 0; i < cells.GetLength(0); i++)
                    {
                        filledLines.Add(cells[i, j]);
                    }
                }
            }
        }
        
        return filledLines;
    }

    public bool CanPlaceShape(Shape shape)
    {
        if (cells == null)
        {
            return false;
        }

        var activeItems = shape.GetActiveItems();
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        // Find the bounding box of the shape
        foreach (var item in activeItems)
        {
            var pos = item.GetPosition();
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }

        var shapeWidth = maxX - minX + 1;
        var shapeHeight = maxY - minY + 1;

        // Try to place the shape at every possible position on the field
        for (var fieldY = 0; fieldY <= cells.GetLength(0) - shapeHeight; fieldY++)
        {
            for (var fieldX = 0; fieldX <= cells.GetLength(1) - shapeWidth; fieldX++)
            {
                if (CanPlaceShapeAt(activeItems, fieldX - minX, fieldY - minY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CanPlaceShapeAt(List<Item> items, int offsetX, int offsetY)
    {
        foreach (var item in items)
        {
            var pos = item.GetPosition();
            var x = offsetX + pos.x;
            var y = offsetY + pos.y;

            if (x < 0 || x >= cells.GetLength(1) || y < 0 || y >= cells.GetLength(0))
            {
                return false; // Out of bounds
            }

            if (cells[y, x].busy)
            {
                return false; // Cell is already occupied
            }
        }

        return true;
    }
    
    public List<Vector2Int> GetAllPossiblePlacements(Shape shape)
    {
        List<Vector2Int> placements = new List<Vector2Int>();
        
        if (cells == null || shape == null)
        {
            return placements;
        }

        var activeItems = shape.GetActiveItems();
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var item in activeItems)
        {
            var pos = item.GetPosition();
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }

        var shapeWidth = maxX - minX + 1;
        var shapeHeight = maxY - minY + 1;

        for (var fieldY = 0; fieldY <= cells.GetLength(0) - shapeHeight; fieldY++)
        {
            for (var fieldX = 0; fieldX <= cells.GetLength(1) - shapeWidth; fieldX++)
            {
                if (CanPlaceShapeAt(activeItems, fieldX - minX, fieldY - minY))
                {
                    placements.Add(new Vector2Int(fieldX - minX, fieldY - minY));
                }
            }
        }

        return placements;
    }
    
    public bool WouldCreateFilledLine(Shape shape, Vector2Int offset)
    {
        if (cells == null || shape == null)
        {
            return false;
        }

        bool[,] tempBusy = new bool[cells.GetLength(0), cells.GetLength(1)];
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                tempBusy[i, j] = cells[i, j].busy;
            }
        }

        var activeItems = shape.GetActiveItems();
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var item in activeItems)
        {
            var pos = item.GetPosition();
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
        }

        foreach (var item in activeItems)
        {
            var pos = item.GetPosition();
            int x = offset.x + pos.x;
            int y = offset.y + pos.y;
            if (x >= 0 && x < cells.GetLength(1) && y >= 0 && y < cells.GetLength(0))
            {
                tempBusy[y, x] = true;
            }
        }

        for (int i = 0; i < cells.GetLength(0); i++)
        {
            bool isFull = true;
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (!tempBusy[i, j])
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull) return true;
        }

        for (int j = 0; j < cells.GetLength(1); j++)
        {
            bool isFull = true;
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                if (!tempBusy[i, j])
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull) return true;
        }

        return false;
    }
    
    public void ClearFilledLines()
    {
        if (cells == null)
        {
            return;
        }
        
        List<int> rowsToClear = new List<int>();
        List<int> columnsToClear = new List<int>();
        
        // Kiểm tra các hàng đầy
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            bool isFull = true;
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (!cells[i, j].busy)
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull)
            {
                rowsToClear.Add(i);
            }
        }
        
        // Kiểm tra các cột đầy
        for (int j = 0; j < cells.GetLength(1); j++)
        {
            bool isFull = true;
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                if (!cells[i, j].busy)
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull)
            {
                columnsToClear.Add(j);
            }
        }
        
        // Xóa các hàng đầy
        foreach (int row in rowsToClear)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                cells[row, j].ClearFilledCell();
            }
        }
        
        // Xóa các cột đầy
        foreach (int col in columnsToClear)
        {
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                cells[i, col].ClearFilledCell();
            }
        }
    }
}
