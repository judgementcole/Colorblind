using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class StaticColor : MonoBehaviour
{
    #region Variables
    [Header("Color")]
    public Colors.colorName _colorName;
    [HideInInspector] public Color color;
    public Action DependanciesColorChange;

    [Header("Sprite Renderers")]
    private List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer>();

    [Header("TextMesh Pro")] // Only works for changing instantly (without animations)
    [SerializeField] private bool greyScale;
    private TextMeshProUGUI _textMeshCompononent;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        FillSpriteRendererList();
        _textMeshCompononent = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Has to be done before everything else
        UpdateColor();

        UpdateTextMeshProColor();
    }

    private void OnEnable()
    {
        ColorManager.Instance.OnFCPColorChange += UpdateColor;
        ColorManager.Instance.OnFCPColorChange += UpdateSpriteRendererStaticColor;
        ColorManager.Instance.InitializeStaticColor += InintializeStaticColor;
        ColorManager.Instance.ChangeToStaticColor += AnimateSpriteRendererColorToStaticColor;

        if (_textMeshCompononent != null)
        {
            // Has to be done before everything else
            UpdateColor();

            UpdateTextMeshProColor();
        }
    }

    private void OnDisable()
    {
        ColorManager.Instance.OnFCPColorChange -= UpdateColor;
        ColorManager.Instance.OnFCPColorChange -= UpdateSpriteRendererStaticColor;
        ColorManager.Instance.InitializeStaticColor -= InintializeStaticColor;
        ColorManager.Instance.ChangeToStaticColor -= AnimateSpriteRendererColorToStaticColor;
    }
    #endregion

    #region Sprite Renderers
    private void FillSpriteRendererList()
    {
        var childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in childSpriteRenderers)
        {
            if (!spriteRenderer.gameObject.CompareTag("Ignore Sprite"))
            {
                _spriteRenderers.Add(spriteRenderer);
            }
        }
    }
    #endregion

    #region Color
    private void UpdateColor()
    {
        if (color != Colors.ConvertEnumToColor(_colorName))
        {
            color = Colors.ConvertEnumToColor(_colorName);

            if (DependanciesColorChange != null)
                DependanciesColorChange();
        }
    }

    private void AnimateSpriteRendererColorToStaticColor()
    {
        if (_spriteRenderers.Count > 0)
            ColorManager.Instance.CallChangeColor(_spriteRenderers.ToArray(), color);
    }

    private void UpdateSpriteRendererStaticColor()
    {
        if (_spriteRenderers.Count > 0)
        {
            foreach(var spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.color = color;
            }
        }
    }

    private void UpdateTextMeshProColor()
    {
        if (_textMeshCompononent != null)
        {
            if (!greyScale)
                _textMeshCompononent.color = color;
            else
                _textMeshCompononent.color = new Color(color.grayscale, color.grayscale, color.grayscale);
        }
    }

    private void InintializeStaticColor()
    {
        if (_spriteRenderers.Count > 0)
        {
            foreach (var childSpriteRenderer in _spriteRenderers)
            {
                childSpriteRenderer.color = Color.white;
            }
        }
    }
    #endregion
}
