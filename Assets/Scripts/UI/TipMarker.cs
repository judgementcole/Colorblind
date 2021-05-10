using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipMarker : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    private float _fadeSpeedMultiplier = 50f;

    [Header("Text Fade")]
    private Coroutine _fadeRoutine;

    [Header("Components")]
    [SerializeField] private CanvasGroup _canvasGroup;
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        GameManager.Instance.OnRespawnStart += HideTextBox;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRespawnStart -= HideTextBox;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance.spawned == false)
                StartCoroutine(ShowTextBoxRoutine());
            else
                ShowTextBox();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            HideTextBox();
    }
    #endregion

    #region Text Box
    IEnumerator ShowTextBoxRoutine()
    {
        yield return new WaitUntil(() => GameManager.Instance.spawned == true);

        ShowTextBox();
    }

    private void ShowTextBox()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        if (_canvasGroup != null)
            _fadeRoutine = StartCoroutine(FadeRoutine(1f));

        UIManager.Instance.ShowTextBox();
    }

    private void HideTextBox()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        if (_canvasGroup != null)
        {
            if (gameObject.activeInHierarchy == true)
                _fadeRoutine = StartCoroutine(FadeRoutine(0f));
            else
                _canvasGroup.alpha = 0;
        }

        UIManager.Instance.HideTextBox();
    }
    #endregion

    #region Text Fade
    IEnumerator FadeRoutine(float alphaValue)
    {
        while (_canvasGroup.alpha != alphaValue)
        {
            if (_canvasGroup.alpha < alphaValue)
                _canvasGroup.alpha += 0.1f * _fadeSpeedMultiplier * Time.deltaTime;
            else if (_canvasGroup.alpha > alphaValue)
                _canvasGroup.alpha -= 0.1f * _fadeSpeedMultiplier * Time.deltaTime;

            yield return null;
        }
    }
    #endregion
}
