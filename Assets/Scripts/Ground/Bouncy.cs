using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncy : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    private float _downwardsVelocity = 1f;
    private float _resistingForce = 1f;
    private float _stopThreshold = 0.001f;
    private float _energyLoss = 0.05f;

    [Header("Bouncy")]
    private Rigidbody2D _platformRb;
    private Vector2 _initialPos;
    private Coroutine _bounceCoroutine;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _platformRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _initialPos = transform.position;
    }
    #endregion

    #region Bouncy
    public void StartBounce()
    {
        if (_bounceCoroutine != null)
            StopCoroutine(_bounceCoroutine);

        _bounceCoroutine = StartCoroutine(Bounce());

    }

    IEnumerator Bounce()
    {
        _platformRb.velocity = new Vector2(0, -_downwardsVelocity);

        while (Mathf.Abs(transform.position.y - _initialPos.y) > _stopThreshold || Mathf.Abs(_platformRb.velocity.y) > 0)
        {
            _platformRb.velocity += new Vector2 (0, (_initialPos.y - transform.position.y) * _resistingForce);
            _platformRb.velocity -= _platformRb.velocity * _energyLoss;

            yield return new WaitForFixedUpdate();

            if (Mathf.Abs(_platformRb.velocity.y) < _stopThreshold)
                _platformRb.velocity = new Vector2(_platformRb.velocity.x, 0);
        }
    }
    #endregion
}
