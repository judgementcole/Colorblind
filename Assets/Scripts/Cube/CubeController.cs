using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    #region Variables
    [Header("Movement")]
    protected float _moveForce = 30f;
    protected float _moveTorque = 25f;
    protected float _nearWallDistance = 0.3f;
    protected float _horizontalInput;
    protected virtual float HorizontalVelocityClamp { get; set; } = 10f;

    [Header("Jump")]
    protected float _jumpForce = 10f;
    protected float jumpCheckDelay = 0.05f;
    protected virtual float OnGroundHeight { get; set; } = 0.541f;
    protected float _groundLandYVelocityThreshold = -0.1f;
    protected int _groundExtensionLayer = 9;
    protected bool _checkJump = true;
    protected bool _isOnGround = true;
    protected bool _jumpReady = false;
    protected bool _jumpInput = false;

    [Header("Color")]
    protected Color _cubeColor;

    [Header("Components")]
    [SerializeField] protected LandingParticles[] _landingParticles;
    protected Rigidbody2D _gameObjectRb;
    protected ParticleSystem[] _particleSystems;
    protected TrailRenderer[] _trails;
    #endregion

    #region Unity Callbacks
    protected virtual void Awake()
    {
        _gameObjectRb = gameObject.GetComponent<Rigidbody2D>();
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _trails = GetComponentsInChildren<TrailRenderer>();
    }

    protected virtual void FixedUpdate()
    {
        Move();

        if (_jumpInput)
        {
            ResetJumpInput();
            Jump();
        }
    }

    protected virtual void LateUpdate()
    {
        ClampMovement();
    }
    #endregion

    #region Movement
    protected void Move()
    {
        _gameObjectRb.AddForce(Vector2.right * _moveForce * _horizontalInput);

        if (!CollidingWithWall())
            _gameObjectRb.AddTorque(-_moveTorque * _horizontalInput);
    }

    protected void ClampMovement()
    {
        _gameObjectRb.velocity = new Vector2(Mathf.Clamp(_gameObjectRb.velocity.x, -HorizontalVelocityClamp, HorizontalVelocityClamp), _gameObjectRb.velocity.y);
        _gameObjectRb.angularVelocity = 0;
    }

    protected bool CollidingWithWall()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(this.transform.position, Vector2.left, _nearWallDistance);
        if (hitLeft.collider != null)
        {
            if (_horizontalInput < 0)
                return true;
        }

        RaycastHit2D hitRight = Physics2D.Raycast(this.transform.position, Vector2.right, _nearWallDistance);
        if (hitRight.collider != null)
        {
            if (_horizontalInput > 0)
                return true;
        }

        return false;
    }
    #endregion

    #region Jump
    protected void Jump()
    {
        if (_jumpReady)
        {
            _gameObjectRb.velocity = new Vector2(_gameObjectRb.velocity.x, _jumpForce);

            _jumpReady = false;
            StartCoroutine(JumpCheckDelay());
        }
    }

    protected virtual void ResetJumpInput()
    {
        _jumpInput = false;
    }

    protected void IsOnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, OnGroundHeight);
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            // If lands on ground after being airborne
            if (!_isOnGround && gameObject.layer != LayerMask.NameToLayer("Ignore Raycast") && _gameObjectRb.velocity.y < _groundLandYVelocityThreshold)
            {
                LandedOnGround(hit);
                _isOnGround = true;
            }

            // If jump check delay timer had been finished (if removed, causes a problem where a player gets another jump after jumping while being still)
            if (_checkJump && !_jumpReady)
            {
                GotJump();
            }
        }
        else
        {
            _isOnGround = false;
        }
    }

    protected virtual void LandedOnGround(RaycastHit2D hit)
    {
        Bounce(hit);
        PlayLandingParticles(hit);
    }

    protected virtual void GotJump()
    {
        _jumpReady = true;
    }

    IEnumerator JumpCheckDelay()
    {
        _checkJump = false;

        yield return new WaitForSeconds(jumpCheckDelay);

        _checkJump = true;
    }

    protected void Bounce(RaycastHit2D hit)
    {
        Bouncy bounceComponent;

        if (hit.collider.gameObject.layer == _groundExtensionLayer)
        {
            bounceComponent = hit.collider.transform.parent.transform.parent.gameObject.GetComponent<Bouncy>();
        }
        else
        {
            bounceComponent = hit.collider.gameObject.GetComponent<Bouncy>();
        }

        if (bounceComponent != null)
        {
            bounceComponent.StartBounce();
        }
    }

    protected void PlayLandingParticles(RaycastHit2D hit)
    {
        Vector2 hitPos = hit.point;

        foreach (var particleSystem in _landingParticles)
        {
            particleSystem.PlayLandingParticles(hitPos);
        }
    }
    #endregion

    #region VFX
    protected void ChangeMoveParticlesColor()
    {
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(_cubeColor, 0.0f), new GradientColorKey(_cubeColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0, 0.0f), new GradientAlphaKey(1, 0.05f), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1.0f) });

        foreach (var particleSystem in _particleSystems)
        {
            var colorOverLifetime = particleSystem.colorOverLifetime;

            colorOverLifetime.color = grad;
        }
    }

    protected void ChangeTrailColor()
    {
        foreach (var trail in _trails)
        {
            trail.startColor = _cubeColor;
            trail.endColor = _cubeColor;
        }
    }
    #endregion
}