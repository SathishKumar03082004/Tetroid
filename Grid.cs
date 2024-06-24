using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public ShapeStorage shapestorage;
    public int columns = 0;
    public int rows = 0;
    public float squaregap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startposition = new Vector2(0.0f, 0.0f);
    public float squarescale = 0.5f;
    public float everysquareoffset = 0.0f;

    private Vector2 offset = new Vector2(0.0f, 0.0f);

    private List<GameObject> gridsquare = new List<GameObject>();

    private LineIndicator line_indicator;

    private void OnEnable() {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
    }

    private void OnDisable() {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
    }

    void Start()
    {
        line_indicator = GetComponent<LineIndicator>();
        CreateGrid();
    }

    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
    }

    private void SpawnGridSquares()
    {
        int square_index = 0;
        for (var row = 0; row < rows; ++row)
        {
            for (var column = 0; column < columns; ++column)
            {
                gridsquare.Add(Instantiate(gridSquare) as GameObject);
                gridsquare[gridsquare.Count - 1].GetComponent<GridSquare>().squareIndex = square_index;
                gridsquare[gridsquare.Count - 1].transform.SetParent(this.transform);
                gridsquare[gridsquare.Count - 1].transform.localScale = new Vector3(squarescale, squarescale, squarescale);
                gridsquare[gridsquare.Count - 1].GetComponent<GridSquare>().SetImage(line_indicator.GetGridSquareIndex(square_index) % 2 == 0);
                square_index++;
            }
        }
    }

    private void SetGridSquaresPositions()
    {
        int column_no = 0;
        int row_no = 0;
        Vector2 square_gap_number = new Vector2(0.0f, 0.0f);
        bool row_moved = false;

        var square_rect = gridsquare[0].GetComponent<RectTransform>();
        offset.x = square_rect.rect.width * square_rect.transform.localScale.x + everysquareoffset;
        offset.y = square_rect.rect.height * square_rect.transform.localScale.y + everysquareoffset;

        foreach (GameObject square in gridsquare)
        {
            if (column_no + 1 > columns)
            {
                square_gap_number.x = 0;
                column_no = 0;
                row_no++;
                row_moved = false;
            }
            var pos_x_offset = offset.x * column_no + (square_gap_number.x * squaregap);
            var pos_y_offset = offset.y * row_no + (square_gap_number.y * squaregap);

            if (column_no > 0 && column_no % 3 == 0)
            {
                square_gap_number.x++;
                pos_x_offset += squaregap;
            }

            if (row_no > 0 && row_no % 3 == 0 && row_moved == false)
            {
                row_moved = true;
                square_gap_number.y++;
                pos_y_offset += squaregap;
            }

            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startposition.x + pos_x_offset, startposition.y - pos_y_offset);

            square.GetComponent<RectTransform>().localPosition = new Vector3(startposition.x + pos_x_offset, startposition.y - pos_y_offset, 0.0f);

            column_no++;
        }
    }

    private void CheckIfShapeCanBePlaced() {
        var squareIndexs = new List<int>();
        foreach (var square in gridsquare) {
            var gridSquare = square.GetComponent<GridSquare>();

            if (gridSquare.Selected && !gridSquare.squareOccupied) {
                squareIndexs.Add(gridSquare.squareIndex);
                gridSquare.Selected = false;
            }
        }

        var currentSelectedShape = shapestorage.GetCurrentSelectedShape();
        if (currentSelectedShape == null) return;

        if (currentSelectedShape.TotalSquareNumber == squareIndexs.Count) {
            foreach (var squareIndex in squareIndexs) {
                gridsquare[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard();
            }

            var shapeLeft = 0;
            foreach (var shape in shapestorage.shapeList) {
                if (shape.IsOnStartPosition() && shape.IsAnyOfShapeSquareActive()) {
                    shapeLeft++;
                }
            }

            if (shapeLeft == 0) {
                GameEvents.RequestNewShape();
            }
            else {
                GameEvents.SetShapeInactive();
            }

            CheckIfAnyLineIsCompleted();
        }
        else {
            GameEvents.MoveShapeToStartPosition();
        }
    }

    void CheckIfAnyLineIsCompleted() {
        List<int[]> lines = new List<int[]>();

        //column
        foreach (var column in line_indicator.columnIndexes) {
            lines.Add(line_indicator.GetVerticalLine(column));
        }

        //row
        for (var row = 0; row < 9; row++) {
            List<int> data = new List<int>(9);
            for (var index = 0; index < 9; index++) {
                data.Add(line_indicator.line_data[row,index]);
            }
            lines.Add(data.ToArray());
        }

        //3X3 Matrix Cancalation 
        // for(var square=0;square<9;square++){
        //     List<int> data = new List<int>(9);
        //     for(var index =0;index < 9;index++) {
        //         data.Add(line_indicator.square_data[square,index]);
        //     }
        //     lines.Add(data.ToArray());
        // }

        var completedLines = CheckIfSquaresAreCompleted(lines);

        if (completedLines > 2) {
            //play bonus
        }
        // Add score
    }

    private int CheckIfSquaresAreCompleted(List<int[]> data) {
        List<int[]> completedLines = new List<int[]>();

        var linesCompleted = 0;

        foreach (var line in data) {
            var lineCompleted = true;
            foreach (var squareIndex in line) {
                var comp = gridsquare[squareIndex].GetComponent<GridSquare>();
                if (comp.squareOccupied == false) {
                    lineCompleted = false;
                }
            }

            if (lineCompleted) {
                completedLines.Add(line);
            }
        }

        foreach (var line in completedLines) {
            var completed = false;
            foreach (var squareIndex in line) {
                var comp = gridsquare[squareIndex].GetComponent<GridSquare>();
                comp.Deactivate();
                completed = true;
            }

            foreach (var squareIndex in line) {
                var comp = gridsquare[squareIndex].GetComponent<GridSquare>();
                comp.ClearOccupied();
            }

            if (completed) {
                linesCompleted++;
            }
        }
        return linesCompleted;
    }
}
