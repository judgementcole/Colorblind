using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class ColorManager : MonoSingleton<ColorManager>
{
    #region Variables
    [Header("Color Shift")]
    private float _colorShiftInterval = 2f;
    public Coroutine colorShiftRoutine;
    public Action InitializeBackgroundColor;
    public Action InitializeStaticColor;
    public Action ChangeToStaticColor;
    public Action ChangeBackgroundColor;
    public Action ChangeGroundState;
    public Action WobblyExtentionAnimate;
    public Action EnemyCubeActiveState;
    [HideInInspector] public List<BackgroundColorShift> backgroundColorShiftComponents = new List<BackgroundColorShift>();

    [Header("Color Fade Animation")]
    [HideInInspector] public float fadeDuration = 0.1f;

    [Header("Color Customization")]
    [SerializeField] private RectTransform _colorSelectionPointer;
    [SerializeField] private Image _playerColorButton;
    [SerializeField] private Image _backgroundColorOneButton;
    [SerializeField] private Image _backgroundColorTwoButton;
    [SerializeField] private Image _backgroundColorThreeButton;
    [SerializeField] private Image _enemyColorButton;
    [SerializeField] private FlexibleColorPicker _fcp;
    private Color _fcpPreviousColor;
    private Colors.colorName _selectedColor = Colors.colorName.player;
    public Action OnFCPColorChange;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        AssignPaletteColorsToButtons();

        // To initialize selection cursor
        SelectPlayerColor();
    }

    private void OnEnable()
    {
        OnFCPColorChange += AssignPaletteColorsToButtons;
    }

    private void OnDisable()
    {
        OnFCPColorChange -= AssignPaletteColorsToButtons;
    }

    private void Update()
    {
        if (GameManager.Instance._inCustomizationScreen)
        {
            if (_fcp.color != _fcpPreviousColor)
            {
                // Needs to be done before calling OnFCPColorChange
                ChangeColorPaletteColors();

                if (OnFCPColorChange != null)
                    OnFCPColorChange();

                _fcpPreviousColor = _fcp.color;
            }
        }
    }
    #endregion

    #region Getting Components
    public void GetBackgroundColorShiftComponents()
    {
        backgroundColorShiftComponents.Clear();

        GameObject gameObjectToSearchIn = LevelManager.Instance.levels[LevelManager.Instance.currentLevel].level;
        var backgroundColorShiftArray = gameObjectToSearchIn.GetComponentsInChildren<BackgroundColorShift>();
        backgroundColorShiftComponents = backgroundColorShiftArray.ToList();
    }
    #endregion

    #region Color Shifts
    public void StartShiftingColors()
    {
        if (colorShiftRoutine != null)
            StopCoroutine(colorShiftRoutine);

        colorShiftRoutine = StartCoroutine(ShiftColors());
    }

    IEnumerator ShiftColors()
    {
        // Set Everything to Black and White
        if (InitializeBackgroundColor != null)
            InitializeBackgroundColor();

        if (InitializeStaticColor != null)
            InitializeStaticColor();

        yield return new WaitUntil(() => GameManager.Instance.spawned == true);

        // Set Everything to Colored
        if (ChangeToStaticColor != null)
            ChangeToStaticColor();

        while (true)
        {
            if (ChangeBackgroundColor != null)
                ChangeBackgroundColor();

            if (ChangeGroundState != null)
                ChangeGroundState();

            if (EnemyCubeActiveState != null)
                EnemyCubeActiveState();

            if (WobblyExtentionAnimate != null)
                WobblyExtentionAnimate();

            yield return new WaitForSeconds(_colorShiftInterval);
        }
    }
    #endregion

    #region Color Fade Animation
    public void CallChangeColor(SpriteRenderer[] spriteRenderers, Color nextColor, ref Coroutine changeColorRoutine)
    {
        changeColorRoutine = StartCoroutine(ChangeColor(spriteRenderers, nextColor));
    }

    public void CallChangeColor(SpriteRenderer[] spriteRenderers, Color nextColor)
    {
        SpriteRenderer[] spriteRenderersArray = spriteRenderers.ToArray();

        StartCoroutine(ChangeColor(spriteRenderersArray, nextColor));
    }

    IEnumerator ChangeColor(SpriteRenderer[] spriteRenderers, Color nextColor)
    {
        var initialColor = spriteRenderers[0].color;
        var currentColor = spriteRenderers[0].color;
        float lerp = 0f;

        while (currentColor != nextColor)
        {
            foreach (var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = Color.Lerp(initialColor, nextColor, lerp);
            }

            currentColor = spriteRenderers[0].color;

            lerp += Time.deltaTime / ColorManager.Instance.fadeDuration;

            yield return null;
        }
    }
    #endregion

    #region Customization Level
    public void AssignPaletteColorsToButtons()
    {
        _playerColorButton.color = Colors.ConvertEnumToColor(Colors.colorName.player);
        _backgroundColorOneButton.color = Colors.ConvertEnumToColor(Colors.colorName.backgroundColorOne);
        _backgroundColorTwoButton.color = Colors.ConvertEnumToColor(Colors.colorName.backgroundColorTwo);
        _backgroundColorThreeButton.color = Colors.ConvertEnumToColor(Colors.colorName.backgroundColorThree);
        _enemyColorButton.color = Colors.ConvertEnumToColor(Colors.colorName.enemy);
    }

    public void ChangeColorPaletteColors()
    {
        switch(_selectedColor)
        {
            case Colors.colorName.player:
                Colors.playerColor = _fcp.color;
                break;

            case Colors.colorName.backgroundColorOne:
                Colors.backgroundColorOne = _fcp.color;
                break;

            case Colors.colorName.backgroundColorTwo:
                Colors.backgroundColorTwo = _fcp.color;
                break;

            case Colors.colorName.backgroundColorThree:
                Colors.backgroundColorThree = _fcp.color;
                break;

            case Colors.colorName.enemy:
                Colors.enemyColor = _fcp.color;
                break;
        }
    }

    public void SelectPlayerColor()
    {
        _colorSelectionPointer.anchoredPosition = _playerColorButton.rectTransform.anchoredPosition;
        _selectedColor = Colors.colorName.player;
        _fcp.color = Colors.ConvertEnumToColor(_selectedColor);
    }

    public void SelectBackgroundColorOne()
    {
        _colorSelectionPointer.anchoredPosition = _backgroundColorOneButton.rectTransform.anchoredPosition;
        _selectedColor = Colors.colorName.backgroundColorOne;
        _fcp.color = Colors.ConvertEnumToColor(_selectedColor);
    }

    public void SelectBackgroundColorTwo()
    {
        _colorSelectionPointer.anchoredPosition = _backgroundColorTwoButton.rectTransform.anchoredPosition;
        _selectedColor = Colors.colorName.backgroundColorTwo;
        _fcp.color = Colors.ConvertEnumToColor(_selectedColor);
    }

    public void SelectBackgroundColorThree()
    {
        _colorSelectionPointer.anchoredPosition = _backgroundColorThreeButton.rectTransform.anchoredPosition;
        _selectedColor = Colors.colorName.backgroundColorThree;
        _fcp.color = Colors.ConvertEnumToColor(_selectedColor);
    }

    public void SelectEnemyColor()
    {
        _colorSelectionPointer.anchoredPosition = _enemyColorButton.rectTransform.anchoredPosition;
        _selectedColor = Colors.colorName.enemy;
        _fcp.color = Colors.ConvertEnumToColor(_selectedColor);
    }

    public void SetColorsToDefault()
    {
        Colors.playerColor = new Color(255f / 255f, 242f / 255f, 117f / 255f);
        Colors.enemyColor = new Color(100f / 255f, 58f / 255f, 133f / 255f);
        Colors.backgroundColorOne = new Color(38f / 255f, 64f / 255f, 39f / 255f);
        Colors.backgroundColorTwo = new Color(222f / 255f, 84f / 255f, 30f / 255f);
        Colors.backgroundColorThree = new Color(131f / 255f, 151f / 255f, 136f / 255f);

        _fcp.color = Colors.ConvertEnumToColor(_selectedColor);

        if (OnFCPColorChange != null)
            OnFCPColorChange(); 
    }
    #endregion
}
