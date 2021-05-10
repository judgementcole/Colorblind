using System.Collections;
using UnityEngine;

public class GroundColorShift : MonoBehaviour
{
    #region Variables
    [Header("Colors")]
    [SerializeField] private Colors.colorName _colorName;
    private bool _initialized;
    private Coroutine _changeColorRoutine;
    private Coroutine _changeSortingLayerToDeactivatedRoutine;

    [Header("Layers")]
    [Tooltip("For sprite renderers")]
    private string _activatedStateSortingLayerName = "Ground";
    [Tooltip("For sprite renderers")]
    private string _deactivatedStateSortingLayerName = "Deactivated Object";

    [Header("Components")]
    private BackgroundColorShift _backgroundColorShiftComponent;
    private BoxCollider2D[] _boxColliders;
    private SpriteRenderer[] _spriteRenderers;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // 'GetComponentsInChildren' gets parent components as well
        _spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        _boxColliders = gameObject.GetComponentsInChildren<BoxCollider2D>();
    }

    private void OnEnable()
    {
        ColorManager.Instance.InitializeStaticColor += InitGroundColor;
        ColorManager.Instance.ChangeToStaticColor += SetColorToColorName;
        ColorManager.Instance.ChangeGroundState += DisableWhenNotVisable;
    }

    private void OnDisable()
    {
        ColorManager.Instance.InitializeStaticColor -= InitGroundColor;
        ColorManager.Instance.ChangeToStaticColor -= SetColorToColorName;
        ColorManager.Instance.ChangeGroundState -= DisableWhenNotVisable;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Background"))
        {
            if (!_initialized)
            {
                _backgroundColorShiftComponent = collision.gameObject.GetComponentInChildren<BackgroundColorShift>();
                InitGroundColor();
            }
        }
    }
    #endregion

    #region Colors
    private void InitGroundColor()
    {
        if (_backgroundColorShiftComponent == null)
            return;

        if (_changeColorRoutine != null)
            ColorManager.Instance.StopCoroutine(_changeColorRoutine);

        UtilityHelper.ChangeStateOfBoxColliders(_boxColliders, true);

        // Needs to be done before change sorting layer to activated sorting layer
        if (_changeSortingLayerToDeactivatedRoutine != null)
            StopCoroutine(_changeSortingLayerToDeactivatedRoutine);

        foreach (var spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingLayerName = _activatedStateSortingLayerName;
        }

        _initialized = true;
    }

    void SetColorToColorName()
    {
        Color convertedColor = Colors.ConvertEnumToColor(_colorName);
        ColorManager.Instance.CallChangeColor(_spriteRenderers, convertedColor, ref _changeColorRoutine);
    }
    #endregion

    #region States
    private void DisableWhenNotVisable()
    {
        if (_colorName == _backgroundColorShiftComponent.nextColorName)
        {
            UtilityHelper.ChangeStateOfBoxColliders(_boxColliders, false);

            _changeSortingLayerToDeactivatedRoutine = StartCoroutine(ChangeSortingLayerToDeactivatedAfterColorChange());
        }
        else
        {
            UtilityHelper.ChangeStateOfBoxColliders(_boxColliders, true);

            foreach (var spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.sortingLayerName = _activatedStateSortingLayerName;
            }
        }
    }

    IEnumerator ChangeSortingLayerToDeactivatedAfterColorChange()
    {
        yield return new WaitForSeconds(ColorManager.Instance.fadeDuration);

        foreach (var spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.sortingLayerName = _deactivatedStateSortingLayerName;
        }
    }
    #endregion
}