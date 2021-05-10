using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BackgroundColorShift : MonoBehaviour
{
    #region Variables
    [Header("Color Patterns")]
    public List<Colors.colorName> _colorPattern = new List<Colors.colorName>();
    [HideInInspector] public Colors.colorName nextColorName;
    private int _patternIndex = 0;
    private Coroutine _changeColorRoutine;

    [Header("Components")]
    private Wobbly _wobbly;
    private List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer>();
    private List<SpriteMask> _spriteMasks = new List<SpriteMask>();
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _spriteMasks = gameObject.transform.parent.GetComponentsInChildren<SpriteMask>().ToList(); // Gets extension sprites as well
        _spriteRenderers = gameObject.transform.parent.GetComponentsInChildren<SpriteRenderer>().ToList(); // Gets extension sprites as well

        _wobbly = this.gameObject.GetComponent<Wobbly>();
    }

    private void OnEnable()
    {
        foreach (var spriteMask in _spriteMasks)
        {
            spriteMask.enabled = false;
        }

        ColorManager.Instance.OnFCPColorChange += UpdateStaticColor;
        GameManager.Instance.OnRespawnStart += ResetPattern;
        ColorManager.Instance.InitializeBackgroundColor += InitializeColor;
        ColorManager.Instance.ChangeBackgroundColor += SetColor;
    }

    private void OnDisable()
    {
        ColorManager.Instance.OnFCPColorChange += UpdateStaticColor;
        GameManager.Instance.OnRespawnStart -= ResetPattern;
        ColorManager.Instance.InitializeBackgroundColor -= InitializeColor;
        ColorManager.Instance.ChangeBackgroundColor -= SetColor;
    }
    #endregion

    #region Color Shift
    public void InitializeColor()
    {
        if (_changeColorRoutine != null)
            ColorManager.Instance.StopCoroutine(_changeColorRoutine);

        foreach (var spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.color = Color.black;
        }
    }

    private void ResetPattern()
    {
        foreach (var spriteMask in _spriteMasks)
        {
            spriteMask.enabled = false;
        }

        _patternIndex = 0;
    }

    public void SetColor()
    {
        nextColorName = AccordingToColorPattern();
        
        if (nextColorName == Colors.colorName.player)
        {
            foreach (var spriteMask in _spriteMasks)
            {
                spriteMask.enabled = true;
            }
        }
        else
        {
            foreach (var spriteMask in _spriteMasks)
            {
                spriteMask.enabled = false;
            }
        }

        Color nextColor = Colors.ConvertEnumToColor(nextColorName);

        ColorManager.Instance.CallChangeColor(_spriteRenderers.ToArray(), nextColor, ref _changeColorRoutine);
        _wobbly.StartWobble();
    }

    private Colors.colorName AccordingToColorPattern()
    {
        Colors.colorName colorToReturn = _colorPattern[_patternIndex];

        if (_patternIndex >= _colorPattern.Count - 1)
            _patternIndex = 0;
        else
            _patternIndex++;

        return colorToReturn;
    }

    private void UpdateStaticColor()
    {
        if (GameManager.Instance._inCustomizationScreen && GameManager.Instance.spawned)
        {
            foreach (var spriteRenderer in _spriteRenderers)
            {
                if (_patternIndex - 1 >= 0)
                    spriteRenderer.color = Colors.ConvertEnumToColor(_colorPattern[_patternIndex - 1]);
                else
                    spriteRenderer.color = Colors.ConvertEnumToColor(_colorPattern[_colorPattern.Count - 1]);
            }
        }
    }
    #endregion
}
