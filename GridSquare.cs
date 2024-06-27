using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    public Image hooverImage;
    public Image activeImage;
    public Image normalImage;
    public List<Sprite> normalImages;

    private Config.SquareColor currentSquareColor = Config.SquareColor.NotSet;

    public bool Selected { get; set; }
    public int squareIndex { get; set; }
    public bool squareOccupied { get; set; }

    void Start()
    {
        Selected = false;
        squareOccupied = false;
    }

    public Config.SquareColor GetCurrentColor()
    {
        return currentSquareColor;
    }

    public void PlaceShapeOnBoard(Config.SquareColor color)
    {
        currentSquareColor = color;
        ActivateSquare();
    }

    public bool CanWeUseThisSquare()
    {
        return hooverImage.gameObject.activeSelf;
    }

    public void ActivateSquare()
    {
        hooverImage.gameObject.SetActive(false);
        activeImage.gameObject.SetActive(true);
        Selected = true;
        squareOccupied = true;
    }

    public void Deactivate()
    {
        currentSquareColor = Config.SquareColor.NotSet;
        activeImage.gameObject.SetActive(false);
    }

    public void ClearOccupied()
    {
        currentSquareColor = Config.SquareColor.NotSet;
        Selected = false;
        squareOccupied = false;
    }

    public void SetImage(bool setFirstImage)
    {
        normalImage.GetComponent<Image>().sprite = setFirstImage ? normalImages[1] : normalImages[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (squareOccupied == false)
        {
            Selected = true;
            hooverImage.gameObject.SetActive(true);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Selected = true;
        if (squareOccupied == false)
        {
            hooverImage.gameObject.SetActive(true);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (squareOccupied == false)
        {
            Selected = false;
            hooverImage.gameObject.SetActive(false);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().UnSetOccupied();
        }
    }

    public void SetSquareColor(Config.SquareColor color)
    {
        currentSquareColor = color;
        Color unityColor;

        switch (color)
        {
            case Config.SquareColor.Red:
                unityColor = Color.red;
                break;
            case Config.SquareColor.Blue:
                unityColor = Color.blue;
                break;
            case Config.SquareColor.Green:
                unityColor = Color.green;
                break;
            case Config.SquareColor.Yellow:
                unityColor = Color.yellow;
                break;
            default:
                unityColor = Color.white;
                break;
        }

        if (activeImage != null)
        {
            activeImage.color = unityColor;
        }
        else if (normalImage != null)
        {
            normalImage.color = unityColor;
        }
        else
        {
            Debug.LogWarning("No Image component found on GridSquare.");
        }
    }
}
