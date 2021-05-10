using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoSingleton<UIManager>
{
    #region Variables
    [Header("Text Box")]
    [SerializeField] private RectTransform _textBoxRectTransform;
    private float _textboxShowStopThreshold = 2f; // Converted to x position on canvas
    private float _textboxHideStopThreshold = 0.1f; // Converted to x position on canvas
    private float _easeOutMultiplier = 3f;
    private float _easeInMultiplier = 6f;
    private Vector3 _textboxShowPosition = new Vector3(-147f, -93f, 0f);
    private Vector3 _textboxHidePosition = new Vector3(150f, -93f, 0f);
    private Coroutine _showTextBoxRoutine;
    private Coroutine _hideTextBoxRoutine;

    [Header("Bubbles")]
    [SerializeField] private Image _smallBubbleImage;
    [SerializeField] private Image _mediumBubbleImage;
    [SerializeField] private Image _largeBubbleImage;
    private float _bubblesFadeInDistance = 20f;
    private float _alphaFadeInMultiplier = 90f;
    private float _alphaFadeOutMultiplier = 180f;
    private float _delayBeforHidingTextBox = 0f;
    private int _bubblesListIndex = 0;
    private List<Image> _bubblesList = new List<Image>();
    private Stack<Image> _bubblesStack = new Stack<Image>();
    private Coroutine _bubblesFadeInRoutine;
    private Coroutine _bubblesFadeOutRoutine;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        InitializeStopThresholds();
        FillBubblesListInOrder();
        InitializeTextBoxPos();
        InitializeBubblesAlpha();
    }
    #endregion

    #region Textbox
    private void InitializeStopThresholds()
    {
        _textboxShowStopThreshold = _textboxShowPosition.x + _textboxShowStopThreshold;
        _textboxHideStopThreshold = _textboxHidePosition.x - _textboxHideStopThreshold;
    }

    private void InitializeTextBoxPos()
    {
        _textBoxRectTransform.anchoredPosition = new Vector3(_textboxHideStopThreshold, _textboxHidePosition.y, _textboxHidePosition.z);
    }

    public void ShowTextBox()
    {
        if (_hideTextBoxRoutine != null)
            StopCoroutine(_hideTextBoxRoutine);

        if (_showTextBoxRoutine != null)
            StopCoroutine(_showTextBoxRoutine);

        _showTextBoxRoutine = StartCoroutine(ShowTextBoxAnimation());
    }

    IEnumerator ShowTextBoxAnimation()
    {
        var fadeRoutineCalled = false;

        while (_textBoxRectTransform.anchoredPosition.x > _textboxShowStopThreshold)
        {
            var distance = (_textboxShowPosition.x - _textBoxRectTransform.anchoredPosition.x);
            _textBoxRectTransform.anchoredPosition += new Vector2(distance * _easeOutMultiplier * Time.deltaTime, 0);

            if (!fadeRoutineCalled && Mathf.Abs(distance) < _bubblesFadeInDistance)
            {
                if (_bubblesFadeOutRoutine != null)
                    StopCoroutine(_bubblesFadeOutRoutine);

                _bubblesFadeInRoutine = StartCoroutine(FadeInBubbles());
                fadeRoutineCalled = true;
            }

            yield return null;
        }
    }

    public void HideTextBox()
    {
        if (_showTextBoxRoutine != null)
            StopCoroutine(_showTextBoxRoutine);

        if (_hideTextBoxRoutine != null)
            StopCoroutine(_hideTextBoxRoutine);

        _hideTextBoxRoutine = StartCoroutine(HideTextBoxAnimation());
    }

    IEnumerator HideTextBoxAnimation()
    {
        if (_bubblesFadeInRoutine != null)
            StopCoroutine(_bubblesFadeInRoutine);

        _bubblesFadeOutRoutine = StartCoroutine(FadeOutBubbles());

        yield return new WaitForSeconds(_delayBeforHidingTextBox);

        while ( _textBoxRectTransform.anchoredPosition.x < _textboxHideStopThreshold)
        {
            var distance =  (_textboxHidePosition.x - _textBoxRectTransform.anchoredPosition.x);
            _textBoxRectTransform.anchoredPosition += new Vector2(distance * _easeInMultiplier * Time.deltaTime, 0);

            yield return null;
        }
    }
    #endregion

    #region Bubbles
    private void FillBubblesListInOrder() // First image that needs to be dealt with should be added first
    {
        _bubblesList.Add(_largeBubbleImage);
        _bubblesList.Add(_mediumBubbleImage);
        _bubblesList.Add(_smallBubbleImage);
    }

    private void InitializeBubblesAlpha()
    {
        foreach (var bubble in _bubblesList)
        {
            ChangeAlphaValueOfBubble(0f, bubble);
        }
    }

    IEnumerator FadeInBubbles()
    {
        while (_bubblesListIndex < _bubblesList.Count)
        {
            Image _currentBubblesImage = _bubblesList[_bubblesListIndex];
            _bubblesStack.Push(_currentBubblesImage);
            _bubblesListIndex++;

            float alphaValue = _currentBubblesImage.color.a;

            while (alphaValue < 1)
            {
                alphaValue += 0.1f * _alphaFadeInMultiplier * Time.deltaTime;
                ChangeAlphaValueOfBubble(alphaValue, _currentBubblesImage);

                yield return null;
            }
        }
    }

    IEnumerator FadeOutBubbles()
    {
        while (_bubblesStack.Count > 0)
        {
            Image _currentBubblesImage = _bubblesStack.Peek();
            float alphaValue = _currentBubblesImage.color.a;

            while (alphaValue > 0)
            {
                alphaValue -= 0.1f * _alphaFadeOutMultiplier * Time.deltaTime;

                ChangeAlphaValueOfBubble(alphaValue, _currentBubblesImage);

                yield return null;
            }

            if (_bubblesStack.Count > 0) // Sometimes, bubblesStack.Pop is called when stack is empty so this had to be done
            {
                _bubblesStack.Pop();
                _bubblesListIndex--;
            }
        }
    }

    private void ChangeAlphaValueOfBubble(float alphaValue, Image bubblesImage)
    {
        var tempColor = bubblesImage.color;
        tempColor.a = alphaValue;
        bubblesImage.color = tempColor;
    }
    #endregion
}
