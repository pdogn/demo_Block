using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellDeck : MonoBehaviour
{
    public Shape shape;

    [SerializeField]
    private FieldManager field;

    public bool IsEmpty => shape == null;

    private void Update()
    {
        //if (shape != null)
        //{
        //    if (field != null)
        //    {
        //        if (field.CanPlaceShape(shape))
        //        {
        //            SetShapeTransparency(shape, 1.0f); // Fully opaque
        //        }
        //        else
        //        {
        //            SetShapeTransparency(shape, 0.1f); // Semi-transparent
        //        }
        //    }
        //}
    }

    public void FillCell(Shape randomShape)
    {
        shape = randomShape;
        if (shape != null)
        {
            shape.transform.SetParent(transform);
            shape.transform.localPosition = Vector3.zero;
            shape.transform.localScale = Vector3.one * 0.5f;
            var scale = shape.transform.localScale;
            shape.transform.localScale = Vector3.zero;
            shape.transform.DOScale(scale, 0.5f).SetEase(Ease.OutBack).OnComplete(() => { shape.transform.localScale = scale; });
        }
    }

    private void SetShapeTransparency(Shape shape, float alpha)
    {
        foreach (var item in shape.GetActiveItems())
        {
            item.SetTransparency(alpha);
        }
    }

    public void ClearCell()
    {
        if (shape != null)
        {
            PoolObject.Return(shape.gameObject);
            shape = null;
        }
    }

}
