using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    #region Variables
    [Header("FPS")]
    [SerializeField] private float _hudRefreshRate = 0.75f;
    private float _timer;
    private TextMeshProUGUI _fpsText;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _fpsText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateFPS();
    }

    private void Update()
    {
        if (Time.unscaledTime > _timer)
        {
            UpdateFPS();
            _timer = Time.unscaledTime + _hudRefreshRate;
        }
    }
    #endregion

    #region FPS
    private void UpdateFPS()
    {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        _fpsText.text = "FPS: " + fps;
    }
    #endregion
}
