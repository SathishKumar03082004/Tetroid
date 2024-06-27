using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public GameObject squareShapeImage;
    public Vector3 shapeSelectedScale;
    public Vector2 offset = new Vector2(0f, 700f);

    [HideInInspector]
    public ShapeData currentShapeData;
    public int TotalSquareNumber { get; set; }

    private List<GameObject> currentShape = new List<GameObject>();
    private Vector3 shapeStartScale;
    private RectTransform _transform;
    private bool shapeDraggable = true;
    private Canvas canvas;
    private Vector3 startPosition;
    private bool shapeActive = true;

    public void Awake()
    {
        shapeStartScale = this.GetComponent<RectTransform>().localScale;
        _transform = this.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        startPosition = _transform.localPosition;
        shapeActive = true;
    }

    private void OnEnable()
    {
        GameEvents.MoveShapeToStartPosition += MoveShapeToStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }

    private void OnDisable()
    {
        GameEvents.MoveShapeToStartPosition -= MoveShapeToStartPosition;
        GameEvents.SetShapeInactive -= SetShapeInactive;
    }

    public bool IsOnStartPosition()
    {
        return _transform.localPosition == startPosition;
    }

    public bool IsAnyOfShapeSquareActive()
    {
        foreach (var square in currentShape)
        {
            if (square.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    public void DeactivateShape()
    {
        if (shapeActive)
        {
            foreach (var square in currentShape)
            {
                square?.GetComponent<ShapeSquare>().DeactivateShape();
            }
        }
        shapeActive = false;
    }

    private void SetShapeInactive()
    {
        if (!IsOnStartPosition() && IsAnyOfShapeSquareActive())
        {
            foreach (var square in currentShape)
            {
                square.gameObject.SetActive(false);
            }
        }
    }

    public void ActivateShape()
    {
        if (!shapeActive)
        {
            foreach (var square in currentShape)
            {
                square?.GetComponent<ShapeSquare>().ActivateShape();
            }
        }
        shapeActive = true;
    }

    public void RequestNewShape(ShapeData shapeData, Color squareColor)
    {
        CreateShape(shapeData, squareColor);
    }

    public void CreateShape(ShapeData shapeData, Color squareColor)
    {
        currentShapeData = shapeData;
        TotalSquareNumber = GetNumberOfSquares(shapeData);

        while (currentShape.Count < TotalSquareNumber)
        {
            currentShape.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }

        for (int i = 0; i < currentShape.Count; i++)
        {
            currentShape[i].gameObject.SetActive(i < TotalSquareNumber);
            currentShape[i].transform.localPosition = Vector3.zero;
            currentShape[i].GetComponent<Image>().color = squareColor;
        }

        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        var moveDistance = new Vector2(squareRect.rect.width * squareRect.localScale.x, squareRect.rect.height * squareRect.localScale.y);

        int currentIndexInList = 0;

        // set position
        for (var row = 0; row < shapeData.rows; row++)
        {
            for (var column = 0; column < shapeData.columns; column++)
            {
                if (shapeData.board[row].column[column])
                {
                    currentShape[currentIndexInList].SetActive(true);
                    currentShape[currentIndexInList].GetComponent<RectTransform>().localPosition = new Vector2(
                        GetXPositionForShapeSquare(shapeData, column, moveDistance),
                        GetYPositionForShapeSquare(shapeData, row, moveDistance));
                    currentIndexInList++;
                }
            }
        }
    }

    private float GetYPositionForShapeSquare(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        float shiftOnY = 0f;

        if (shapeData.rows > 1)
        {
            if (shapeData.rows % 2 != 0)
            {
                var middleSquareIndex = (shapeData.rows - 1) / 2;
                var multiplier = (shapeData.rows - 1) / 2;

                if (row < middleSquareIndex)
                { // move negative
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= multiplier;
                }
                else if (row > middleSquareIndex)
                { // move plus
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.rows == 2) ? 1 : (shapeData.rows / 2);
                var middleSquareIndex1 = (shapeData.rows == 2) ? 0 : shapeData.rows - 2;
                var multiplier = shapeData.rows / 2;

                if (row == middleSquareIndex1 || row == middleSquareIndex2)
                {
                    if (row == middleSquareIndex2)
                    {
                        shiftOnY = (moveDistance.y / 2) * -1;
                    }
                    if (row == middleSquareIndex1)
                    {
                        shiftOnY = (moveDistance.y / 2);
                    }

                    if (row < middleSquareIndex1 && row < middleSquareIndex2)
                    { // negative
                        shiftOnY = moveDistance.y * 1;
                        shiftOnY *= multiplier;
                    }
                    else if (row > middleSquareIndex1 && row > middleSquareIndex2)
                    { // positive
                        shiftOnY = moveDistance.y * -1;
                        shiftOnY *= multiplier;
                    }
                }
            }
        }
        return shiftOnY;
    }

    private float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        float shiftOnX = 0f;

        if (shapeData.columns > 1)
        { // vertical position
            if (shapeData.columns % 2 != 0)
            {
                var middleSquareIndex = (shapeData.columns - 1) / 2;
                var multiplier = (shapeData.columns - 1) / 2;
                if (column < middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * -1;
                    shiftOnX *= multiplier;
                }
                else if (column > middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.columns == 2) ? 1 : (shapeData.columns / 2);
                var middleSquareIndex1 = (shapeData.columns == 2) ? 0 : shapeData.columns - 1;
                var multiplier = shapeData.columns / 2;

                if (column == middleSquareIndex1 || column == middleSquareIndex2)
                {
                    if (column == middleSquareIndex2)
                    {
                        shiftOnX = moveDistance.x / 2;
                    }
                    if (column == middleSquareIndex1)
                    {
                        shiftOnX = (moveDistance.x / 2) * -1;
                    }

                    if (column < middleSquareIndex1 && column < middleSquareIndex2)
                    { // negative
                        shiftOnX = moveDistance.x * -1;
                        shiftOnX *= multiplier;
                    }
                    else if (column > middleSquareIndex1 && column > middleSquareIndex2)
                    { // plus
                        shiftOnX = moveDistance.x * 1;
                        shiftOnX *= multiplier;
                    }
                }
            }
        }
        return shiftOnX;
    }

    private int GetNumberOfSquares(ShapeData shapeData)
    {
        int number = 0;

        foreach (var rowData in shapeData.board)
        {
            foreach (var active in rowData.column)
            {
                if (active)
                {
                    number++;
                }
            }
        }
        return number;
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _transform.localScale = shapeSelectedScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _transform.anchorMin = new Vector2(0, 0);
        _transform.anchorMax = new Vector2(0, 0);
        _transform.pivot = new Vector2(0, 0);

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, Camera.main, out pos);
        _transform.localPosition = pos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _transform.localScale = shapeSelectedScale;
        GameEvents.CheckIfShapeCanBePlaced();
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    private void MoveShapeToStartPosition()
    {
        _transform.localPosition = startPosition;
    }
}
