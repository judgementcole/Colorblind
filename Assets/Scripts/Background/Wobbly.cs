using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobbly : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    public bool invertWobble;
    private float _upscaleRate = 2.5f;
    private float _resistingScaleRate = 100f;
    private float _stopThreshold = 0.001f;
    private float _energyLoss = 5f;

    [Header("Wobble")]
    private float _currentScaleRate;
    private bool _invert;
    private Vector3 _initialScale;
    private Coroutine _wobbleCoroutine;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        _initialScale = transform.localScale;
    }

    private void OnDisable()
    {
        Reset();
    }

    private void Update()
    {
        transform.localScale += new Vector3(_currentScaleRate * Time.deltaTime, 0, 0);
    }
    #endregion

    #region Wobble
    public void StartWobble()
    {
        if (_wobbleCoroutine != null)
            StopCoroutine(_wobbleCoroutine);

        _wobbleCoroutine = StartCoroutine(Wobble());
    }

    IEnumerator Wobble()
    {
        SetWobbleScaleRate();

        while (Mathf.Abs(transform.localScale.x - _initialScale.x) > _stopThreshold || Mathf.Abs(_currentScaleRate) > 0)
        {
            _currentScaleRate += (_initialScale.x - transform.localScale.x) * _resistingScaleRate * Time.deltaTime;
            _currentScaleRate -= _currentScaleRate * _energyLoss * Time.deltaTime;

            yield return null;
        }
    }

    private void SetWobbleScaleRate()
    {
        if (!_invert)
        {
            if (!invertWobble)
                _currentScaleRate = _upscaleRate;
            else
                _currentScaleRate = -_upscaleRate;

            _invert = true;
        }
        else if (_invert)
        {
            if (!invertWobble)
                _currentScaleRate = -_upscaleRate;
            else
                _currentScaleRate = _upscaleRate;

            _invert = false;
        }
    }

    private void Reset()
    {
        _currentScaleRate = 0;
        transform.localScale = _initialScale;
    }
    #endregion
}
