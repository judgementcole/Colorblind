using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors
{
    #region Variables
    [System.Serializable]
    public enum colorName
    {
        white, // Starts from index zero

        player = 2,
        enemy,
        backgroundColorOne,
        backgroundColorTwo,
        backgroundColorThree = 1,
    }
    
    [Header("Common For All")]
    public static Color _whiteColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);

    [Header("Colorblind Original")]
    public static Color playerColor = new Color(255f / 255f, 242f / 255f, 117f / 255f);
    public static Color enemyColor = new Color(100f / 255f, 58f / 255f, 133f / 255f);
    public static Color backgroundColorOne = new Color(38f / 255f, 64f / 255f, 39f / 255f);
    public static Color backgroundColorTwo = new Color(222f / 255f, 84f / 255f, 30f / 255f);
    public static Color backgroundColorThree = new Color(131f / 255f, 151f / 255f, 136f / 255f);
    #endregion

    #region Color Conversion
    public static Color ConvertEnumToColor(colorName color)
    {
        Color colorToReturn = new Color();

        switch (color)
        {
            case Colors.colorName.white:
                colorToReturn = _whiteColor;
                break;

            case Colors.colorName.player:
                colorToReturn = playerColor;
                break;

            case Colors.colorName.enemy:
                colorToReturn = enemyColor;
                break;

            case Colors.colorName.backgroundColorOne:
                colorToReturn = backgroundColorOne;
                break;

            case Colors.colorName.backgroundColorTwo:
                colorToReturn = backgroundColorTwo;
                break;

            case Colors.colorName.backgroundColorThree:
                colorToReturn = backgroundColorThree;
                break;
        }

        return colorToReturn;
    }
    #endregion
}
