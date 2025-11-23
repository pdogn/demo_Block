using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemFactory : MonoBehaviour
{
    private ShapeTemplate[] shapes;
    protected ItemTemplate[] items;

    [SerializeField]
    private FieldManager field;

    protected virtual void Awake()
    {
        shapes = Resources.LoadAll<ShapeTemplate>("Shapes");
        items = Resources.LoadAll<ItemTemplate>("Items");
    }

    private ShapeTemplate GetNonRepeatedShapeTemplate(HashSet<ShapeTemplate> usedShapeTemplates)
    {
        ShapeTemplate shapeTemplate = null;
        if (usedShapeTemplates == null)
        {
            return GetRandomShape();
        }

        do
        {
            shapeTemplate = GetRandomShape();
        } while (usedShapeTemplates.Contains(shapeTemplate));

        usedShapeTemplates.Add(shapeTemplate);
        return shapeTemplate;
    }

    private ShapeTemplate GetRandomShape()
    {
        ShapeTemplate shapeTemplate = null;
        int randomWeight = Random.Range(0, shapes.Length);

        shapeTemplate = shapes[randomWeight];

        return shapeTemplate;
    }


    public Shape CreateRandomShape(HashSet<ShapeTemplate> usedShapeTemplates, GameObject shapeObject, HashSet<ItemTemplate> usedColors = null)
    {
        var shape = shapeObject.GetComponent<Shape>();
        shape.transform.localScale = Vector3.one;
        shape.UpdateShape(GetNonRepeatedShapeTemplate(usedShapeTemplates));

        ItemTemplate color = usedColors != null ? GetUniqueColor(usedColors) : GetColor();
        shape.UpdateColor(color);
        if (usedColors != null && color != null)
        {
            usedColors.Add(color);
        }

        return shape;
    }

    public ItemTemplate GetColor()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogError("Items array is null or empty in ItemFactory.GetColor()");
            return null;
        }
        int randomWeight = Random.Range(1, items.Length);

        return items[randomWeight];
    }

    private ItemTemplate GetUniqueColor(HashSet<ItemTemplate> usedColors)
    {
        if (items == null || items.Length == 0)
        {
            return null;
        }

        var availableColors = items.Where(c => !usedColors.Contains(c)).ToList();
        if (availableColors.Count == 0)
        {
            availableColors = items.ToList();
        }

        int randomIndex = Random.Range(1, availableColors.Count);
        return availableColors[randomIndex];
    }

    public Shape CreateRandomShapeFits(GameObject shapeObject, HashSet<ShapeTemplate> usedShapeTemplates = null, HashSet<ItemTemplate> usedColors = null)
    {
        if (field == null || shapes == null || shapes.Length == 0)
        {
            return CreateRandomShape(usedShapeTemplates ?? new HashSet<ShapeTemplate>(), shapeObject, usedColors);
        }

        var shape = shapeObject.GetComponent<Shape>();
        if (shape == null)
        {
            return null;
        }

        shape.transform.localScale = Vector3.one;

        var availableShapes = new List<ShapeTemplate>();
        
        if (usedShapeTemplates != null && usedShapeTemplates.Count > 0)
        {
            availableShapes.AddRange(shapes.Where(s => !usedShapeTemplates.Contains(s)));
        }
        else
        {
            availableShapes.AddRange(shapes);
        }

        if (availableShapes.Count == 0)
        {
            availableShapes.AddRange(shapes);
        }

        int maxAttempts = Mathf.Min(availableShapes.Count * 2, 50);
        ShapeTemplate fittingTemplate = null;
        ItemTemplate selectedColor = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomIndex = Random.Range(0, availableShapes.Count);
            ShapeTemplate testTemplate = availableShapes[randomIndex];
            
            ItemTemplate testColor = usedColors != null ? GetUniqueColor(usedColors) : GetColor();
            shape.UpdateShape(testTemplate);
            shape.UpdateColor(testColor);

            if (field.CanPlaceShape(shape))
            {
                fittingTemplate = testTemplate;
                selectedColor = testColor;
                if (usedShapeTemplates != null)
                {
                    usedShapeTemplates.Add(testTemplate);
                }
                if (usedColors != null && selectedColor != null)
                {
                    usedColors.Add(selectedColor);
                }
                break;
            }
        }

        if (fittingTemplate == null)
        {
            int randomIndex = Random.Range(0, availableShapes.Count);
            fittingTemplate = availableShapes[randomIndex];
            selectedColor = usedColors != null ? GetUniqueColor(usedColors) : GetColor();
            if (usedShapeTemplates != null)
            {
                usedShapeTemplates.Add(fittingTemplate);
            }
            if (usedColors != null && selectedColor != null)
            {
                usedColors.Add(selectedColor);
            }
        }

        shape.UpdateShape(fittingTemplate);
        shape.UpdateColor(selectedColor ?? GetColor());

        return shape;
    }

    public List<Shape> CreateThreeShapesThatCanClearLine(GameObject[] shapeObjects, HashSet<ShapeTemplate> usedShapeTemplates = null)
    {
        if (field == null || shapes == null || shapes.Length == 0 || shapeObjects == null || shapeObjects.Length < 3)
        {
            return null;
        }

        var availableShapes = new List<ShapeTemplate>();
        if (usedShapeTemplates != null && usedShapeTemplates.Count > 0)
        {
            availableShapes.AddRange(shapes.Where(s => !usedShapeTemplates.Contains(s)));
        }
        else
        {
            availableShapes.AddRange(shapes);
        }

        if (availableShapes.Count == 0)
        {
            availableShapes.AddRange(shapes);
        }

        if (items == null || items.Length < 3)
        {
            Debug.LogWarning("Not enough colors available. Need at least 3 different colors.");
        }

        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var testShapes = new List<Shape>();
            var testTemplates = new List<ShapeTemplate>();
            var tempUsedTemplates = usedShapeTemplates != null ? new HashSet<ShapeTemplate>(usedShapeTemplates) : new HashSet<ShapeTemplate>();
            var usedColors = new HashSet<ItemTemplate>();

            for (int i = 0; i < 3; i++)
            {
                if (shapeObjects[i] == null) continue;

                var shape = shapeObjects[i].GetComponent<Shape>();
                if (shape == null) continue;

                ShapeTemplate template = null;
                int templateAttempts = 0;
                do
                {
                    int randomIndex = Random.Range(0, availableShapes.Count);
                    template = availableShapes[randomIndex];
                    templateAttempts++;
                } while (tempUsedTemplates.Contains(template) && templateAttempts < 50);

                if (template == null) break;

                ItemTemplate color = GetUniqueColor(usedColors);
                if (color == null)
                {
                    break;
                }

                shape.transform.localScale = Vector3.one;
                shape.UpdateShape(template);
                shape.UpdateColor(color);
                usedColors.Add(color);

                if (!field.CanPlaceShape(shape))
                {
                    break;
                }

                testShapes.Add(shape);
                testTemplates.Add(template);
                tempUsedTemplates.Add(template);
            }

            if (testShapes.Count == 3 && usedColors.Count == 3)
            {
                if (CanThreeShapesClearLine(testShapes))
                {
                    if (usedShapeTemplates != null)
                    {
                        foreach (var template in testTemplates)
                        {
                            usedShapeTemplates.Add(template);
                        }
                    }
                    return testShapes;
                }
            }
        }

        return null;
    }

    private bool CanThreeShapesClearLine(List<Shape> shapes)
    {
        if (field == null || shapes == null || shapes.Count != 3)
        {
            return false;
        }

        bool[,] tempBusy = new bool[field.cells.GetLength(0), field.cells.GetLength(1)];
        for (int i = 0; i < field.cells.GetLength(0); i++)
        {
            for (int j = 0; j < field.cells.GetLength(1); j++)
            {
                tempBusy[i, j] = field.cells[i, j].busy;
            }
        }

        foreach (var shape in shapes)
        {
            var placements = field.GetAllPossiblePlacements(shape);
            if (placements.Count == 0)
            {
                return false;
            }

            bool placed = false;
            foreach (var offset in placements)
            {
                var activeItems = shape.GetActiveItems();
                int minX = int.MaxValue, minY = int.MaxValue;
                foreach (var item in activeItems)
                {
                    var pos = item.GetPosition();
                    minX = Mathf.Min(minX, pos.x);
                    minY = Mathf.Min(minY, pos.y);
                }

                bool canPlace = true;
                foreach (var item in activeItems)
                {
                    var pos = item.GetPosition();
                    int x = offset.x + pos.x;
                    int y = offset.y + pos.y;
                    if (x < 0 || x >= field.cells.GetLength(1) || y < 0 || y >= field.cells.GetLength(0))
                    {
                        canPlace = false;
                        break;
                    }
                    if (tempBusy[y, x])
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    foreach (var item in activeItems)
                    {
                        var pos = item.GetPosition();
                        int x = offset.x + pos.x;
                        int y = offset.y + pos.y;
                        tempBusy[y, x] = true;
                    }
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                return false;
            }
        }

        for (int i = 0; i < field.cells.GetLength(0); i++)
        {
            bool isFull = true;
            for (int j = 0; j < field.cells.GetLength(1); j++)
            {
                if (!tempBusy[i, j])
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull) return true;
        }

        for (int j = 0; j < field.cells.GetLength(1); j++)
        {
            bool isFull = true;
            for (int i = 0; i < field.cells.GetLength(0); i++)
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
}
