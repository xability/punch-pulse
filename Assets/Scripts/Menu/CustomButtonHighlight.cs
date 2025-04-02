using UnityEngine;
using UnityEngine.UI;

public class CustomButtonHighlight : MonoBehaviour
{
    public Image highlightImage;
    public Color normalColor = Color.clear;
    public Color selectedColor = Color.green;

    public void SetHighlighted(bool isHighlighted)
    {
        highlightImage.color = isHighlighted ? selectedColor : normalColor;
    }
}