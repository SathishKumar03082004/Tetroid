using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeStorage : MonoBehaviour
{
    public List<ShapeData> shapeData;
    public List<Shape> shapeList;

    private void OnEnable()
    {
        GameEvents.RequestNewShape += RequestNewShape;
    }

    private void OnDisable()
    {
        GameEvents.RequestNewShape -= RequestNewShape;
    }

    void Start()
    {
        foreach (var shape in shapeList)
        {
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            Color defaultSquareColor = GetRandomColor(); // Or replace with a specific color
            shape.CreateShape(shapeData[shapeIndex], defaultSquareColor);
        }
    }

    public Shape GetCurrentSelectedShape()
    {
        foreach (var shape in shapeList)
        {
            if (!shape.IsOnStartPosition() && shape.IsAnyOfShapeSquareActive())
            {
                return shape;
            }
        }

        Debug.LogError("There is no shape");
        return null;
    }

    private void RequestNewShape()
    {
        foreach (var shape in shapeList)
        {
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            Color defaultSquareColor = GetRandomColor();
            shape.RequestNewShape(shapeData[shapeIndex], defaultSquareColor);
        }
    }

    private Color GetRandomColor()
    {
        // Get all values of the SquareColor enum
        Config.SquareColor[] values = (Config.SquareColor[])System.Enum.GetValues(typeof(Config.SquareColor));
        // Randomly select one, excluding NotSet
        Config.SquareColor selectedColor = values[UnityEngine.Random.Range(1, values.Length)];
        return MapSquareColorToColor(selectedColor);
    }

    private Color MapSquareColorToColor(Config.SquareColor squareColor)
    {
        switch (squareColor)
        {
            case Config.SquareColor.Red:
                return Color.red;
            case Config.SquareColor.Blue:
                return Color.blue;
            case Config.SquareColor.Orange:
                return new Color(1.0f, 0.5f, 0.0f); // Orange
            case Config.SquareColor.Mint:
                return new Color(0.0f, 1.0f, 0.5f); // Mint
            case Config.SquareColor.Yellow:
                return Color.yellow;
            case Config.SquareColor.Green:
                return Color.green;
            case Config.SquareColor.Pink:
                return new Color(1.0f, 0.4f, 0.7f); // Pink
            case Config.SquareColor.Purple:
                return new Color(0.5f, 0.0f, 0.5f); // Purple
            default:
                return Color.white; // Default color if not set
        }
    }
}
