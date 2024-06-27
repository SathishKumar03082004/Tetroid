using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public ShapeStorage shapestorage;
    public int columns = 9; // Updated columns and rows to match your grid size
    public int rows = 9;
    public float squaregap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startposition = new Vector2(0.0f, 0.0f);
    public float squarescale = 0.5f;
    public float everysquareoffset = 0.0f;
    public SquareTextureData squareTextureData;

    private Vector2 offset = new Vector2(0.0f, 0.0f);

    private List<GameObject> gridsquare = new List<GameObject>();

    private LineIndicator line_indicator;

    private Config.SquareColor currentActiveSquareColor = Config.SquareColor.NotSet;

    [Header("Audio Clips")]
    public AudioClip rowColumnCancelClip;
    public AudioClip grid3x3CancelClip;

    private AudioSource audioSource;

    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
        GameEvents.UpdateSquareColor += OnUpdateSquareColor;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
        GameEvents.UpdateSquareColor -= OnUpdateSquareColor;
    }

    void Start()
    {
        line_indicator = GetComponent<LineIndicator>();
        audioSource = GetComponent<AudioSource>();
        CreateGrid();
        currentActiveSquareColor = squareTextureData.activeSquareTextures[0].squareColor; // Initialize with first texture color
    }

    private void OnUpdateSquareColor(Config.SquareColor color)
    {
        currentActiveSquareColor = color;
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

    private void CheckIfShapeCanBePlaced()
    {
        var squareIndexs = new List<int>();
        foreach (var square in gridsquare)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if (gridSquare.Selected && !gridSquare.squareOccupied)
            {
                squareIndexs.Add(gridSquare.squareIndex);
                gridSquare.Selected = false;
            }
        }

        var currentSelectedShape = shapestorage.GetCurrentSelectedShape();
        if (currentSelectedShape == null) return;

        if (currentSelectedShape.TotalSquareNumber == squareIndexs.Count)
        {
            foreach (var squareIndex in squareIndexs)
            {
                gridsquare[squareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard(currentActiveSquareColor);
            }

            var shapeLeft = 0;
            foreach (var shape in shapestorage.shapeList)
            {
                if (shape.IsOnStartPosition() && shape.IsAnyOfShapeSquareActive())
                {
                    shapeLeft++;
                }
            }

            if (shapeLeft == 0)
            {
                GameEvents.RequestNewShape();
            }
            else
            {
                GameEvents.SetShapeInactive();
            }

            CheckIfAnyLineIsCompleted();
        }
        else
        {
            GameEvents.MoveShapeToStartPosition();
        }
    }




    





    private void CheckIfAnyLineIsCompleted()
    {
        List<int[]> lines = new List<int[]>();

        //column
        foreach (var column in line_indicator.columnIndexes)
        {
            lines.Add(line_indicator.GetVerticalLine(column));
        }

        //row
        for (var row = 0; row < 9; row++)
        {
            List<int> data = new List<int>(9);
            for (var index = 0; index < 9; index++)
            {
                data.Add(line_indicator.line_data[row, index]);
            }
            lines.Add(data.ToArray());
        }

        var completedLines = CheckIfSquaresAreCompleted(lines);

        // Handle 3x3 matrices separately
        var completed3x3Matrices = CheckIf3x3MatricesAreCompleted();

        if (completedLines >= 2)
        {
            // play bonus
            GameEvents.ShowCongratulationsWritings();
        }

        var totalScore = 10 * completedLines + 15 * completed3x3Matrices;
        if (totalScore >= 15)
        {
            GameEvents.ShowCongratulationsWritings();
        }

        GameEvents.AddScores(totalScore);
        CheckIfPlayerLost();
    }

    private int CheckIfSquaresAreCompleted(List<int[]> data)
    {
        List<int[]> completedLines = new List<int[]>();

        var linesCompleted = 0;

        foreach (var line in data)
        {
            var lineCompleted = true;
            foreach (var squareIndex in line)
            {
                var comp = gridsquare[squareIndex].GetComponent<GridSquare>();
                if (!comp.squareOccupied)
                {
                    lineCompleted = false;
                }
            }

            if (lineCompleted)
            {
                completedLines.Add(line);
            }
        }

        foreach (var line in completedLines)
        {
            var completed = false;
            foreach (var squareIndex in line)
            {
                var comp = gridsquare[squareIndex].GetComponent<GridSquare>();
                comp.Deactivate();
                completed = true;
            }

            foreach (var squareIndex in line)
            {
                var comp = gridsquare[squareIndex].GetComponent<GridSquare>();
                comp.ClearOccupied();
            }

            if (completed)
            {
                linesCompleted++;
            }
        }

        // Play audio if any line is completed
        if (linesCompleted > 0 && audioSource != null && rowColumnCancelClip != null)
        {
            audioSource.PlayOneShot(rowColumnCancelClip);
        }
        return linesCompleted;
    }

    private int CheckIf3x3MatricesAreCompleted()
    {
        int completedMatrices = 0;
        for (var square = 0; square < 9; square++)
        {
            bool matrixCompleted = true;
            for (var index = 0; index < 9; index++)
            {
                var comp = gridsquare[line_indicator.square_data[square, index]].GetComponent<GridSquare>();
                if (!comp.squareOccupied)
                {
                    matrixCompleted = false;
                    break;
                }
            }

            if (matrixCompleted)
            {
                completedMatrices++;
                // Deactivate and clear the 3x3 matrix
                for (var index = 0; index < 9; index++)
                {
                    var comp = gridsquare[line_indicator.square_data[square, index]].GetComponent<GridSquare>();
                    comp.Deactivate();
                    comp.ClearOccupied();
                }
            }
        }

        if (completedMatrices > 0 && audioSource != null && grid3x3CancelClip != null)
        {
            audioSource.PlayOneShot(grid3x3CancelClip);
        }
        return completedMatrices;
    }

    private void CheckIfPlayerLost()
    {
        var validShapes = 0;

        for (var index = 0; index < shapestorage.shapeList.Count; index++)
        {
            var isShapeActive = shapestorage.shapeList[index].IsAnyOfShapeSquareActive();
            if (CheckIfShapeCanBePlacedOnGrid(shapestorage.shapeList[index]) && isShapeActive)
            {
                shapestorage.shapeList[index]?.ActivateShape();
                validShapes++;
            }
        }

        if (validShapes == 0)
        {
            // Game over
            GameEvents.GameOver(false);
            Debug.Log("Game Over");
        }
    }

    private bool CheckIfShapeCanBePlacedOnGrid(Shape currentShape)
    {
        var currentShapeData = currentShape.currentShapeData;
        var shapeColumns = currentShapeData.columns;
        var shapeRows = currentShapeData.rows;

        // All index
        List<int> originalShapeFilledUpSquares = new List<int>();
        var squareIndex = 0;

        for (var rowIndex = 0; rowIndex < shapeRows; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < shapeColumns; columnIndex++)
            {
                if (currentShapeData.board[rowIndex].column[columnIndex])
                {
                    originalShapeFilledUpSquares.Add(squareIndex);
                }
                squareIndex++;
            }
        }

        if (currentShape.TotalSquareNumber != originalShapeFilledUpSquares.Count)
        {
            Debug.LogError("No Space Is Available");
        }

        var squareList = GetAllSquaresCombination(shapeColumns, shapeRows);

        bool canBePlaced = false;

        foreach (var number in squareList)
        {
            bool shapeCanBePlacedOnTheBoard = true;
            foreach (var squareIndexToCheck in originalShapeFilledUpSquares)
            {
                if (squareIndexToCheck >= number.Length) continue;
                var comp = gridsquare[number[squareIndexToCheck]].GetComponent<GridSquare>();
                if (comp.squareOccupied)
                {
                    shapeCanBePlacedOnTheBoard = false;
                }
            }

            if (shapeCanBePlacedOnTheBoard)
            {
                canBePlaced = true;
                break;
            }
        }
        return canBePlaced;
    }

    private List<int[]> GetAllSquaresCombination(int columns, int rows)
    {
        var squareList = new List<int[]>();
        var lastColumnIndex = 0;
        var lastRowIndex = 0;

        int safeIndex = 0;

        while (lastRowIndex + (rows - 1) < 9)
        {
            var rowData = new List<int>();

            for (var row = lastRowIndex; row < lastRowIndex + rows; row++)
            {
                for (var column = lastColumnIndex; column < lastColumnIndex + columns; column++)
                {
                    rowData.Add(line_indicator.line_data[row, column]);
                }
            }

            squareList.Add(rowData.ToArray());
            lastColumnIndex++;

            if (lastColumnIndex + (columns - 1) >= 9)
            {
                lastRowIndex++;
                lastColumnIndex = 0;
            }

            safeIndex++;
            if (safeIndex > 100)
            {
                break;
            }
        }

        return squareList;
    }
}