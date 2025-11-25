using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public CellDeck[] cellDecks;

        [SerializeField]
    private FieldManager field;

    [SerializeField]
    private ItemFactory itemFactory;

    [SerializeField]
    public Shape shapePrefab;

    private HashSet<ShapeTemplate> usedShapes = new HashSet<ShapeTemplate>();

    private void Start()
    {
        FillCellDecks();
    }

    public void FillCellDecks(Shape shape = null)
    {
        RemoveUsedShapes(shape);

        if (cellDecks.Any(x => !x.IsEmpty))
        {
            return;
        }

        var usedShapeTemplates = new HashSet<ShapeTemplate>(GetShapes().Select(s => s.shapeTemplate));

        if (cellDecks.Length >= 3)
        {
            var shapeObjects = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                shapeObjects[i] = PoolObject.GetObject(shapePrefab.gameObject);
            }

            var threeShapes = itemFactory.CreateThreeShapesThatCanClearLine(shapeObjects, usedShapeTemplates);
            
            if (threeShapes != null && threeShapes.Count == 3)
            {
                for (int i = 0; i < 3 && i < cellDecks.Length; i++)
                {
                    if (cellDecks[i].IsEmpty)
                    {
                        cellDecks[i].FillCell(threeShapes[i]);
                    }
                    else
                    {
                        PoolObject.Return(threeShapes[i].gameObject);
                    }
                }
                
                for (int i = 3; i < shapeObjects.Length; i++)
                {
                    if (shapeObjects[i] != null)
                    {
                        PoolObject.Return(shapeObjects[i]);
                    }
                }
                
                return;
            }
            else
            {
                for (int i = 0; i < shapeObjects.Length; i++)
                {
                    if (shapeObjects[i] != null)
                    {
                        PoolObject.Return(shapeObjects[i]);
                    }
                }
            }
        }

        var fitShapesCount = 0;
        var usedColors = new HashSet<ItemTemplate>();
        
        for (var index = 0; index < cellDecks.Length; index++)
        {
            var cellDeck = cellDecks[index];
            if (cellDeck.IsEmpty)
            {
                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                Shape randomShape = null;

                if (fitShapesCount < 2 && index >= cellDecks.Length - 2)
                {
                    randomShape = itemFactory.CreateRandomShapeFits(shapeObject, usedShapeTemplates, usedColors);
                }
                else
                {
                    randomShape = itemFactory.CreateRandomShape(usedShapeTemplates, shapeObject, usedColors);
                }

                if (field.CanPlaceShape(randomShape))
                {
                    fitShapesCount++;
                }

                cellDeck.FillCell(randomShape);
            }
        }
    }

    public Shape[] GetShapes()
    {
        return cellDecks.Select(x => x.shape).Where(x => x != null).ToArray();
    }

    private void RemoveUsedShapes(Shape shape)
    {
        if (shape == null)
        {
            return;
        }

        foreach (var cellDeck in cellDecks)
        {
            // Chỉ remove shape nếu reference khớp chính xác
            if (cellDeck.shape != null && cellDeck.shape == shape)
            {
                // Clear reference trước khi return về pool
                cellDeck.shape = null;
                // Chỉ return nếu shape vẫn còn active (chưa bị return rồi)
                if (shape.gameObject.activeSelf)
                {
                    PoolObject.Return(shape.gameObject);
                }
            }
        }

        int count = cellDecks.Count(d => d.shape != null);
        Debug.Log("count: " + count);
        if (count > 0)
        {
            bool checkGameOver = true;
            foreach (var cellDeck in cellDecks)
            {
                if (cellDeck.shape != null)
                {
                    if (field.CanPlaceShape(cellDeck.shape))
                    {
                        checkGameOver = false;
                        break;
                    }
                }
            }

            //bool checkGameOver = !cellDecks.Any(deck => deck.shape != null && field.CanPlaceShape(deck.shape));
            if (checkGameOver)
            {
                Debug.LogWarning("Faillllll");
                UIManager.Instance.ShowGameOverUi();
            }
        }
    }

    public void ClearData()
    {
        foreach (var cellDeck in cellDecks)
        {
            if (cellDeck.shape == null) continue;
            RemoveUsedShapes(cellDeck.shape);
            cellDeck.shape = null;
        }
        FillCellDecks();
    }
}
