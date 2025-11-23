using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeDraggable : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private readonly float verticalOffset = 300;
    private Vector3 originalScale;
    private bool isDragging;
    private int activeTouchId = -1;
    private Canvas canvas;
    private Camera eventCamera;

    private Shape shape;
    private List<Item> _items = new List<Item>();
    private HighlightManager highlightManager;
    private FieldManager field;
    private CellDeck parentCellDeck;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        shape = GetComponent<Shape>();
        
        if (shape != null)
        {
            shape.OnShapeUpdated += UpdateItems;
        }
        
        UpdateItems();
        highlightManager ??= FindObjectOfType<HighlightManager>();
        field ??= FindObjectOfType<FieldManager>();
        parentCellDeck = GetComponentInParent<CellDeck>();

        canvas = GetComponentInParent<Canvas>();
        eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    private void OnDisable()
    {
        if (shape != null)
        {
            shape.OnShapeUpdated -= UpdateItems;
        }
        
        if (isDragging)
        {
            isDragging = false;
            activeTouchId = -1;
        }
    }

    private void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0)
        {
            // Handle existing active touch
            if (isDragging && activeTouchId != -1)
            {
                bool foundActiveTouch = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.fingerId == activeTouchId)
                    {
                        HandleDrag(touch.position);
                        foundActiveTouch = true;

                        // Check if touch has ended
                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            EndDrag();
                        }
                        break;
                    }
                }

                if (!foundActiveTouch)
                {
                    EndDrag();
                }
            }
            // Check for new touches if not already dragging
            else if (!isDragging)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touch.position, eventCamera))
                        {
                            activeTouchId = touch.fingerId;
                            BeginDrag(touch.position);
                            break;
                        }
                    }
                }
            }
        }
        // Handle mouse input if not already handling touch
        else if (activeTouchId == -1)
        {
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, eventCamera))
                {
                    BeginDrag(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                HandleDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                EndDrag();
            }
        }

        if (isDragging && activeTouchId == -1 && !Input.GetMouseButton(0))
        {
            EndDrag();
        }
    }

    private void UpdateItems()
    {
        if (shape != null)
        {
            _items = shape.GetActiveItems();
        }
    }

    private void BeginDrag(Vector2 position)
    {
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;

        transform.SetAsLastSibling();
        transform.localScale = Vector3.one;
    }


    private void HandleDrag(Vector2 position)
    {
        if (!isDragging || field == null)
        {
            return;
        }

        var cellSize = field.GetCellSize();
        var shapeOriginalWidth = 100f;
        var scaleFactor = cellSize / shapeOriginalWidth;

        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform, position, eventCamera, out var localPoint))
        {
            var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
            var normalizedX = localPoint.x / canvasWidth;
            var scaleFactorY = rectTransform.rect.height / canvas.GetComponent<RectTransform>().rect.height * 2.5f;

            rectTransform.anchoredPosition = new Vector2(
                normalizedX * canvasWidth,
                localPoint.y / scaleFactorY + verticalOffset + scaleFactorY
            );
        }

        bool canPlace = !AnyBusyCellsOrNoneCells();
        
        if (!canPlace)
        {
            if (highlightManager != null)
            {
                highlightManager.ClearAllHighlights();
            }
        }
        else
        {
            UpdateCellHighlights();
        }
    }

    private void EndDrag()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        activeTouchId = -1;

        if (highlightManager == null || highlightManager.GetHighlightedCells().Count == 0)
        {
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;
            
            if (highlightManager != null)
            {
                highlightManager.ClearAllHighlights();
                highlightManager.OnDragEndedWithoutPlacement();
            }
            return;
        }

        // Place shape on field - Fill các cell được highlight
        int filledCellsCount = 0;
        foreach (var kvp in highlightManager.GetHighlightedCells())
        {
            if (kvp.Key != null && kvp.Value != null && kvp.Value.itemTemplate != null)
            {
                kvp.Key.FillCell(kvp.Value.itemTemplate);
                filledCellsCount++;
            }
        }

        if (filledCellsCount == 0)
        {
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;
            
            if (highlightManager != null)
            {
                highlightManager.ClearAllHighlights();
                highlightManager.OnDragEndedWithoutPlacement();
            }
            return;
        }

        if (field != null)
        {
            field.ClearFilledLines();
        }

        if (highlightManager != null)
        {
            highlightManager.ClearAllHighlights();
        }

        Shape shapeToRemove = shape;
        CellDeck deckToClear = parentCellDeck;

        if (deckToClear != null && deckToClear.shape == shapeToRemove)
        {
            deckToClear.shape = null;
        }
        
        DeckManager deckManager = FindObjectOfType<DeckManager>();
        if (deckManager != null && shapeToRemove != null)
        {
            deckManager.FillCellDecks(shapeToRemove);
        }
        
        if (shapeToRemove != null && shapeToRemove.gameObject.activeSelf)
        {
            PoolObject.Return(shapeToRemove.gameObject);
        }
    }


    private bool AnyBusyCellsOrNoneCells()
    {
        return _items.Any(item =>
        {
            var cell = GetCellUnderShape(item);
            var cellComponent = cell?.GetComponent<Cell>();
            return cell == null || cellComponent == null || !cellComponent.IsEmpty();
        });
    }

    private void UpdateCellHighlights()
    {
        if (highlightManager == null || field == null)
        {
            return;
        }

        highlightManager.ClearAllHighlights();

        foreach (var item in _items)
        {
            var cell = GetCellUnderShape(item);
            if (cell != null)
            {
                Debug.Log("cel:: " + cell.gameObject.name + " light");
                highlightManager.HighlightCell(cell, item);
            }
        }

        if (_items.Count > 0 && _items[0].itemTemplate != null)
        {
            var highlightedCells = highlightManager.GetHighlightedCells();
            
            var filledRows = field.GetFilledLines(true, highlightedCells);
            if (filledRows.Count > 0)
            {
                highlightManager.HighlightFill(filledRows, _items[0].itemTemplate);
            }
            
            var filledColumns = field.GetFilledLines(false, highlightedCells);
            if (filledColumns.Count > 0)
            {
                highlightManager.HighlightFill(filledColumns, _items[0].itemTemplate);
            }
        }
    }

    private Transform GetCellUnderShape(Item item)
    {
        var hit = Physics2D.Raycast(item.transform.position, Vector2.zero, 1);
        if(hit.collider != null && hit.collider.CompareTag("Cell"))
        {
            Debug.Log("cuc : " + hit.collider.gameObject.name);
        }
        return hit.collider != null && hit.collider.CompareTag("Cell") ? hit.collider.transform : null;
    }
}
