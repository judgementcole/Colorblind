using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WobblyExtention : MonoBehaviour
{
    #region Variables
    [Header("Wobbly Extension")]
    private bool _initialized;
    private Wobbly _wobbly;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _wobbly = transform.parent.GetComponent<Wobbly>();
    }

    private void OnEnable()
    {
        ColorManager.Instance.WobblyExtentionAnimate += OnColorShift;
    }

    private void OnDisable()
    {
        ColorManager.Instance.WobblyExtentionAnimate -= OnColorShift;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Background"))
        {
            if (!_initialized)
            {
                var backgroundWobblyComponent = collision.gameObject.GetComponentInChildren<Wobbly>();
                _wobbly.invertWobble = backgroundWobblyComponent.invertWobble;

                _initialized = true;
            }
        }
    }
    #endregion

    #region Wobbly
    private void OnColorShift()
    {
        _wobbly.StartWobble();
    }
    #endregion
}
