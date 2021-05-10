using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CubeController
{
    #region Variables
    [Header("Movement")]
    private float _horizontalVelocityClamp = 9f;
    protected override float HorizontalVelocityClamp
    {
        get => _horizontalVelocityClamp;
        set => _horizontalVelocityClamp = value;
    }

    [Header("Jump")]
    private float _onGroundHeight = 0.42f;
    protected override float OnGroundHeight
    {
        get => _onGroundHeight;
        set => _onGroundHeight = value;
    }

    [Header("AI")]
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private float _reactionTime = 1f;
    private float _heightDifferenceForJump = 0.5f;
    private bool _playerDetected = false;
    private bool _chasing = false;
    private bool _isDead = false;
    private bool _isActive = true;
    private Coroutine _reactionTimeRoutine;

    [Header("Spawnning")]
    private Vector2 _spawnPoint;

    [Header("Soul Door")]
    public SoulDoor _soulDoor;

    [Header("Detection Stats")]
    private Vector3 _detectionStatsOffset = new Vector3(0, 1, 0);

    [Header("Color")]
    private Colors.colorName _colorName;

    [Header("Layers")]
    private string _activatedStateLayerName = "Default";
    private string _deactivatedStateLayerName = "Ignore Raycast";
    [Tooltip("For sprite renderers")]
    private string _activatedStateSortingLayerName = "Cubes";
    [Tooltip("For sprite renderers")]
    private string _deactivatedStateSortingLayerName = "Deactivated Object";

    [Header("Components")]
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider2D;
    private List<BackgroundColorShift> _backgroundColorShiftComponent = new List<BackgroundColorShift>();
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        base.Awake();

        _colorName = gameObject.GetComponent<StaticColor>()._colorName;
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _boxCollider2D = gameObject.GetComponent<BoxCollider2D>();

        _spawnPoint = transform.position;
    }

    private void Start()
    {
        HorizontalVelocityClamp = _horizontalVelocityClamp;
    }

    private void OnEnable()
    {
        ColorManager.Instance.EnemyCubeActiveState += DieIfNotVisable;
        GameManager.Instance.OnRespawnStart += Respawn;
        GameManager.Instance.OnRespawnComplete += Activate;
    }

    private void OnDisable()
    {
        ColorManager.Instance.EnemyCubeActiveState -= DieIfNotVisable;
        GameManager.Instance.OnRespawnStart -= Respawn;
        GameManager.Instance.OnRespawnComplete -= Activate;
    }

    private void Update()
    {
        if (!_isDead)
        {
            if (_isActive && !_playerDetected)
            {
                PlayerDetectionCheck();
            }

            if (_isActive && _chasing)
            {
                TakeInput();
            }
            else
            {
                ResetInput();
            }

            IsOnGround();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Background"))
        {
            _backgroundColorShiftComponent.Add(collision.gameObject.GetComponentInChildren<BackgroundColorShift>());
        }
        else if (collision.gameObject.CompareTag("Death Collider"))
        {
            Die();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Background"))
        {
            _backgroundColorShiftComponent.Remove(collision.gameObject.GetComponentInChildren<BackgroundColorShift>());

            DieIfNotVisable();
        }
    }
    #endregion

    #region Input
    private void TakeInput()
    {
        _horizontalInput = Mathf.Sign(EnemyManager.Instance.playerTransform.position.x - this.transform.position.x);

        if (EnemyManager.Instance.playerTransform.position.y > this.transform.position.y + _heightDifferenceForJump)
        {
            _jumpInput = true;
        }
    }

    private void ResetInput()
    {
        _horizontalInput = 0;
        _jumpInput = false;
    }
    #endregion

    #region Player Detection
    private void PlayerDetectionCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, EnemyManager.Instance.playerTransform.position - transform.position, _attackRange);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                PlayerDetected();
            }
        }
    }

    private void PlayerDetected()
    {
        _playerDetected = true;
        PoolManager.Instance.RequestDetectionStats(transform.position + _detectionStatsOffset, _reactionTime);
        _reactionTimeRoutine = StartCoroutine(ReactionTime());
    }

    IEnumerator ReactionTime()
    {
        yield return new WaitForSeconds(_reactionTime);

        _chasing = true;
    }
    #endregion

    #region Spawnning and Death
    void DieIfNotVisable()
    {
        if (!_isDead)
        {
            if (_backgroundColorShiftComponent.Count == 1 && _colorName == _backgroundColorShiftComponent[0].nextColorName) // If both are true, enemy shouldn't be visable
            {
                Die();
            }
        }
    }

    private void Die()
    {
        // This needs to be done before setting spawn position
        PoolManager.Instance.RequestSoulOrb(_soulDoor, transform.position);

        ChangeStateOfCollisionsWithEntities(false);

        _isActive = false;
        _isDead = true;

        ResetInput();
        StopMovement();
        SetSpawnPosition();

        _spriteRenderer.sortingLayerName = _deactivatedStateSortingLayerName;
        gameObject.layer = LayerMask.NameToLayer(_deactivatedStateLayerName);
    }

    private void Respawn()
    {
        if (_reactionTimeRoutine != null)
            StopCoroutine(_reactionTimeRoutine);

        ChangeStateOfCollisionsWithEntities(true);

        _playerDetected = false;
        _chasing = false;
        _isDead = false;
        _isActive = false;

        StopMovement(); // Called a second time to avoid not stopping movement
        SetSpawnPosition(); // Called a second time to avoid not spawning

        _spriteRenderer.sortingLayerName = _activatedStateSortingLayerName;
        gameObject.layer = LayerMask.NameToLayer(_activatedStateLayerName);
    }

    private void StopMovement()
    {
        _gameObjectRb.velocity = Vector2.zero;
        _gameObjectRb.angularVelocity = 0;
    }

    private void SetSpawnPosition()
    {
        transform.eulerAngles = Vector3.zero;
        transform.position = _spawnPoint;
    }

    private void ChangeStateOfCollisionsWithEntities(bool state) // State is '!' in code because if state of collisions is false, then ignore collisions should be true
    {
        Physics2D.IgnoreCollision(_boxCollider2D, EnemyManager.Instance.playerCollider, !state);

        foreach (var enemyCollider in EnemyManager.Instance.enemyColliders)
        {
            Physics2D.IgnoreCollision(_boxCollider2D, enemyCollider, !state);
        }
    }

    private void Activate()
    {
        _isActive = true;
    }
    #endregion
}
