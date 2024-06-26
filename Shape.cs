using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour,IPointerClickHandler,IPointerUpHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerDownHandler
{
    public GameObject squareShapeImage;
    public Vector3 shapeSelectedScale;
    public Vector2 offset =new Vector2(0f,700f);


    [HideInInspector]
    public ShapeData currentshapedata;
    public int TotalSquareNumber{get;set;}

    private List<GameObject> currentshape = new List<GameObject>();
    private Vector3 shapeStartScale;
    private RectTransform _transform;
    private bool shapeDraggable=true;
    private Canvas canvas;
    private Vector3 startposition;
    private bool shapeactive=true;

    public void Awake() {
        shapeStartScale=this.GetComponent<RectTransform>().localScale;
        _transform=this.GetComponent<RectTransform>();
        canvas=GetComponentInParent<Canvas>();
        shapeDraggable=true;
        startposition=_transform.localPosition;
        shapeactive=true;

        //startposition=_transform.localPosition;
    }

    private void OnEnable() {
        GameEvents.MoveShapeToStartPosition += MoveShapeToStartPosition;
        //GameEvents.SetShapeInactive+=SetShapeInactive;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }

    private void OnDisable() {
        GameEvents.MoveShapeToStartPosition -= MoveShapeToStartPosition;
        GameEvents.SetShapeInactive-=SetShapeInactive;
    }


    public bool IsOnStartPosition(){
        return _transform.localPosition==startposition;
    }

    public bool IsAnyOfShapeSquareActive(){
        foreach(var square in currentshape){
            if(square.gameObject.activeSelf){
                return true;
            }
        }
        return false;
    }

    public void DeactivateShape(){
        if(shapeactive){
            foreach(var square in currentshape){
                square?.GetComponent<ShapeSquare>().DeactivateShape();
            }
        }
        shapeactive=false;
    }

    private void SetShapeInactive(){
        if(IsOnStartPosition() == false && IsAnyOfShapeSquareActive()){
            foreach(var square in currentshape){
                square.gameObject.SetActive(false);
            }
        }
    }

    public void ActivateShape(){
        if(!shapeactive){
            foreach(var square in currentshape){
                square?.GetComponent<ShapeSquare>().ActivateShape();
            }
        }
        shapeactive=true;
    }

    public void RequestNewShape(ShapeData shapeData)
    {
        _transform.localPosition=startposition;
        CreateShape(shapeData);
    }

    public void CreateShape(ShapeData shapeData)
    {
        currentshapedata = shapeData;
        TotalSquareNumber = GetNumberofSquares(shapeData);

        while (currentshape.Count <= TotalSquareNumber)
        {
            currentshape.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }

        foreach (var square in currentshape)
        {
            square.gameObject.transform.position = Vector3.zero;
            square.gameObject.SetActive(false);
        }

        var squareReact = squareShapeImage.GetComponent<RectTransform>();
        var moveDistance = new Vector2(squareReact.rect.width * squareReact.localScale.x, squareReact.rect.height * squareReact.localScale.y);

        int currentIndexInList = 0;

        // set position
        for (var row = 0; row < shapeData.rows; row++)
        {
            for (var column = 0; column < shapeData.columns; column++)
            {
                if (shapeData.board[row].column[column])
                {
                    currentshape[currentIndexInList].SetActive(true);
                    currentshape[currentIndexInList].GetComponent<RectTransform>().localPosition = new Vector2(
                        GetXPositionForShapeSquare(shapeData, column, moveDistance),
                        GetYPositionForShapeSquare(shapeData, row, moveDistance));
                    currentIndexInList++;
                }
            }
        }
    }

    private float GetYPositionForShapeSquare(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        float shiftOny = 0f;

        if (shapeData.rows > 1)
        {
            if (shapeData.rows % 2 != 0)
            {
                var middleSquareIndex = (shapeData.rows - 1) / 2;
                var multiplier = (shapeData.rows - 1) / 2;

                if (row < middleSquareIndex)
                { // move negative
                    shiftOny = moveDistance.y * 1;
                    shiftOny *= multiplier;
                }
                else if (row > middleSquareIndex)
                { // move plus
                    shiftOny = moveDistance.y * -1;
                    shiftOny *= multiplier;
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
                        shiftOny = (moveDistance.y / 2) * -1;
                    }
                    if (row == middleSquareIndex1)
                    {
                        shiftOny = (moveDistance.y / 2);
                    }

                    if (row < middleSquareIndex1 && row < middleSquareIndex2)
                    { // negative
                        shiftOny = moveDistance.y * 1;
                        shiftOny *= multiplier;
                    }
                    else if (row > middleSquareIndex1 && row > middleSquareIndex2)
                    { // positive
                        shiftOny = moveDistance.y * -1;
                        shiftOny *= multiplier;
                    }
                }
            }
        }
        return shiftOny;
    }

    private float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        float shiftOnx = 0f;

        if (shapeData.columns > 1)
        { // vertical position
            if (shapeData.columns % 2 != 0)
            {
                var middleSquareIndex = (shapeData.columns - 1) / 2;
                var multiplier = (shapeData.columns - 1) / 2;
                if (column < middleSquareIndex)
                {
                    shiftOnx = moveDistance.x * -1;
                    shiftOnx *= multiplier;
                }
                else if (column > middleSquareIndex)
                {
                    shiftOnx = moveDistance.x * 1;
                    shiftOnx *= multiplier;
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
                        shiftOnx = moveDistance.x / 2;
                    }
                    if (column == middleSquareIndex1)
                    {
                        shiftOnx = (moveDistance.x / 2) * -1;
                    }

                    if (column < middleSquareIndex1 && column < middleSquareIndex2)
                    { // negative
                        shiftOnx = moveDistance.x * -1;
                        shiftOnx *= multiplier;
                    }
                    else if (column > middleSquareIndex1 && column > middleSquareIndex2)
                    { // plus
                        shiftOnx = moveDistance.x * 1;
                        shiftOnx *= multiplier;
                    }
                }
            }
        }
        return shiftOnx;
    }

    private int GetNumberofSquares(ShapeData shapedata)
    {
        int number = 0;

        foreach (var rowData in shapedata.board)
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

    public void OnPointerClick(PointerEventData eventData){

    }

    public void OnPointerUp(PointerEventData eventData){

    }

    public void OnBeginDrag(PointerEventData eventData){
        this.GetComponent<RectTransform>().localScale=shapeSelectedScale;
    }

    public void OnDrag(PointerEventData eventData){
        _transform.anchorMin=new Vector2(0,0);
        _transform.anchorMax=new Vector2(0,0);
        _transform.pivot=new Vector2(0,0);

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,eventData.position,Camera.main,out pos);
        _transform.localPosition=pos+offset;
    }

    public void OnEndDrag(PointerEventData eventData){
        this.GetComponent<RectTransform>().localScale=shapeSelectedScale;
        GameEvents.CheckIfShapeCanBePlaced();
    }

    public void OnPointerDown(PointerEventData eventData){

    }

    private void MoveShapeToStartPosition(){
        _transform.transform.localPosition=startposition;
    }
}
