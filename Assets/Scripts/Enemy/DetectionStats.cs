using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetectionStats : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    private float _loadingTimeReduction = 0.2f; // To make target acquired pop up before it starts chasing
    private float _transitionStatsTime = 0.1f;
    private float _borderAnimationTime = 0.2f;
    private float _delayAfterLoadingTime = 0.1f;
    private float _delayForDissapperance = 1f;
    private float _disappearTime = 0.2f;
    private Colors.colorName _statsColorName = Colors.colorName.white;

    [Header("Detection Stats")]
    private float _loadingTime;
    [HideInInspector] public bool inUse = false;
    private Vector3 _initialPos = new Vector3(0, 24, 0);
    private Vector3 _loadingBarPosition = Vector3.zero;
    private Vector3 _targetAcquiredPosition = new Vector3(0, -24, 0);
    private Vector3 _barBordersDisplacement = new Vector3(6.4f, 0, 0);
    private Vector3 _barLeftBorderInitPos;
    private Vector3 _barRightBorderInitPos;
    private Coroutine animationRoutine;

    [Header("Components")]
    private RectTransform _barLeftBorder;
    private RectTransform _barRightBorder;
    private CanvasGroup _canvasGroup;
    private RectTransform _detectionStats;
    private Slider _detectionBarSlider;
    private RectMask2D _rectMask; // Parent Object (need to be used for setting initial pos)
    private Image[] _allImages;
    private TextMeshProUGUI[] _allText;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _canvasGroup = gameObject.GetComponentInChildren<CanvasGroup>();
        _detectionBarSlider = gameObject.GetComponentInChildren<Slider>();
        _rectMask = gameObject.GetComponent<RectMask2D>();
        _allImages = gameObject.GetComponentsInChildren<Image>();
        _allText = gameObject.GetComponentsInChildren<TextMeshProUGUI>();

        var children = gameObject.GetComponentsInChildren<RectTransform>();
        foreach (var child in children)
        {
            if (child.name == "Position")
                _detectionStats = child;
            if (child.name == "Left Border")
                _barLeftBorder = child;
            if (child.name == "Right Border")
                _barRightBorder = child;
        }
    }

    private void Start()
    {
        ChangeColor();

        _barLeftBorderInitPos = _barLeftBorder.localPosition;
        _barRightBorderInitPos = _barRightBorder.localPosition;
    }

    private void OnEnable()
    {
        GameManager.Instance.OnRespawnStart += ResetStats;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRespawnStart -= ResetStats;
    }
    #endregion

    #region Color
    private void ChangeColor()
    {
        Color statsColor = Colors.ConvertEnumToColor(_statsColorName);
        foreach (var image in _allImages)
        {
            image.color = statsColor;
        }
        foreach (var text in _allText)
        {
            text.color = statsColor;
        }
    }
    #endregion

    #region Detection Stats
    public void PlayDetectionStatsAnimation(Vector3 position, float reactionTime)
    {
        inUse = true;

        _rectMask.transform.position = position;
        _loadingTime = reactionTime - (_transitionStatsTime + _borderAnimationTime + _delayAfterLoadingTime + _loadingTimeReduction);

        if (_loadingTime < 0)
            Debug.LogError("Detection Stats loading time is negative");

        animationRoutine = StartCoroutine(DetectionStatsAnimation());
    }

    IEnumerator DetectionStatsAnimation()
    {
        float lerp = 0f;

        while (_detectionStats.localPosition.y > _loadingBarPosition.y)
        {
            _detectionStats.localPosition = Vector3.Lerp(_initialPos, _loadingBarPosition, lerp);

            lerp += Time.deltaTime / _transitionStatsTime;

            yield return null;
        }

        lerp = 0;

        while (_barLeftBorder.localPosition.x > (_barLeftBorderInitPos.x - _barBordersDisplacement.x))
        {
            _barLeftBorder.localPosition = Vector3.Lerp(_barLeftBorderInitPos, _barLeftBorderInitPos - _barBordersDisplacement, lerp);
            _barRightBorder.localPosition = Vector3.Lerp(_barRightBorderInitPos, _barRightBorderInitPos + _barBordersDisplacement, lerp);

            lerp += Time.deltaTime / _borderAnimationTime;

            yield return null;
        }

        lerp = 0;

        while (_detectionBarSlider.value != _detectionBarSlider.maxValue)
        {
            _detectionBarSlider.value = Mathf.Lerp(_detectionBarSlider.minValue, _detectionBarSlider.maxValue, lerp);

            lerp += Time.deltaTime / _loadingTime;

            yield return null;
        }

        yield return new WaitForSeconds(_delayAfterLoadingTime);

        lerp = 0;

        while (_detectionStats.localPosition.y > _targetAcquiredPosition.y)
        {
            _detectionStats.localPosition = Vector3.Lerp(_loadingBarPosition, _targetAcquiredPosition, lerp);

            lerp += Time.deltaTime / _transitionStatsTime;

            yield return null;
        }

        yield return new WaitForSeconds(_delayForDissapperance);

        lerp = 0;

        while (_canvasGroup.alpha > 0)
        {
            _canvasGroup.alpha = Mathf.Lerp(1, 0, lerp);

            lerp += Time.deltaTime / _disappearTime;

            yield return null;
        }

        ResetStats();
    }

    private void ResetStats()
    {
        inUse = false;
        _detectionStats.localPosition = _initialPos;
        _detectionBarSlider.value = _detectionBarSlider.minValue;
        _canvasGroup.alpha = 1;
        _barLeftBorder.localPosition = _barLeftBorderInitPos;
        _barRightBorder.localPosition = _barRightBorderInitPos;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);
    }
    #endregion
}
