using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CubeController
{
    #region Variables
    [Header("Movement")]
    private float _rawHorizontalInput;
    private float _convertedInput;
    [SerializeField] private bool _movingLeft;
    [SerializeField] private bool _movingRight;

    [Header("Jump")]
    private float _retainJumpInputDuration = 0.15f;
    private Coroutine _retainJumpInputRoutine;

    [Header("QDC")]
    private bool _qDCButtonDown;

    [Header("Spawnning")]
    private float _delayBeforeSpawnParticlesBurst = 1.05f;
    private float _delayBeforeScalingPlayer = 0.5f;
    private float _halfScaleDuration = 0.4f;
    private float _scaleStopThreshold = 0.001f;
    private float _scaleEnergyLoss = 6f;
    private float _resistingScaleRate = 150f;
    private float _initialScaleRateForBounce = 2f;
    private Vector3 _targetScaleForSpawn = new Vector3(0.5f, 0.5f, 1f);
    private Vector3 _targetRotationForSpawn = new Vector3(0, 0, 180);
    private Coroutine _animateToOriginalSizeRoutine;
    private Coroutine _spawnCompleteRoutine;

    [Header("Particle Systems")]
    [SerializeField] private GameObject _moveParticlesFolder;
    [SerializeField] private ParticleSystem _spawnParticlesVortex;
    [SerializeField] private ParticleSystem _spawnParticlesBurst;

    [Header("Phone Controls")]
    [SerializeField] private Animator _leftButton;
    [SerializeField] private Animator _rightButton;
    [SerializeField] private Animator _jumpButton;
    [SerializeField] private Animator _qDCButton;
    [SerializeField] private Animator _pauseButton;

    [Header("Components")]
    private StaticColor _staticColorComponent;
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        base.Awake();

        _staticColorComponent = GetComponent<StaticColor>();
    }

    private void OnEnable()
    {
        Respawn();

        if (GameManager.Instance.onAndroid)
            SetButtonImagesToDefault();
        else
            KBMInputOnEnable();

        _staticColorComponent.DependanciesColorChange += SetColors;
        AdManager.Instance.OnInterstitialOpened += GamePaused;
        AdManager.Instance.OnInterstitialClosed += GameResumed;
        GameManager.Instance.OnGameResumed += GameResumed;

        SetColors();
    }

    private void OnDisable()
    {
        _staticColorComponent.DependanciesColorChange -= SetColors;
        AdManager.Instance.OnInterstitialOpened -= GamePaused;
        AdManager.Instance.OnInterstitialClosed -= GameResumed;
        GameManager.Instance.OnGameResumed -= GameResumed;
    }

    private void Update()
    {
        if (!GameManager.Instance.onAndroid)
            KBMInputHandling();

        if (GameManager.Instance.spawned)
        {   
            _horizontalInput = ConvertToSmoothedInput(_rawHorizontalInput);
            IsOnGround();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Exit"))
        {
            LevelManager.Instance.OnLevelClear();

            if (gameObject.activeSelf == true)
                Respawn();
        }
        else if (collision.gameObject.CompareTag("Death Collider"))
        {
            Die();
        }
    }
    #endregion

    #region Mobile Input
    #region Right Button
    public void RightButtonDown()
    {
        _rightButton.SetBool("Button Down", true);

        _movingRight = true;

        if (!_movingLeft)
            _rawHorizontalInput = 1;

        if (_qDCButtonDown)
        {
            QuickDirectionChange(1f);
        }
    }

    public void RightButtonUp()
    {
        _rightButton.SetBool("Button Down", false);

        _movingRight = false;

        if (_movingLeft)
            _rawHorizontalInput = -1;
        else
            _rawHorizontalInput = 0;
    }
    #endregion

    #region Left Button
    public void LeftButtonDown()
    {
        _leftButton.SetBool("Button Down", true);

        _movingLeft = true;

        if (!_movingRight)
            _rawHorizontalInput = -1;

        if (_qDCButtonDown)
        {
            QuickDirectionChange(-1f);
        }
    }

    public void LeftButtonUp()
    {
        _leftButton.SetBool("Button Down", false);

        _movingLeft = false;

        if (_movingRight)
            _rawHorizontalInput = 1;
        else
            _rawHorizontalInput = 0;
    }
    #endregion

    #region Jump Button
    public void JumpButtonDown()
    {
        _jumpButton.SetBool("Button Down", true);

        _jumpInput = true;
    }
    #endregion

    #region QDC Button
    public void QDCButtonDown()
    {
        _qDCButton.SetBool("Button Down", true);

        _qDCButtonDown = true;
    }

    public void QDCButtonUp()
    {
        _qDCButton.SetBool("Button Down", false);

        _qDCButtonDown = false;
    }
    #endregion

    #region Pause Button
    public void PauseButtonDown()
    {
        _pauseButton.SetBool("Button Down", true);
    }

    public void PauseButtonUp()
    {
        _pauseButton.SetBool("Button Down", false);
    }
    #endregion

    private float ConvertToSmoothedInput(float rawInput)
    {
        float sensitivity = 3f;
        float dead = 0.001f;

        _convertedInput = Mathf.MoveTowards(_convertedInput,
                      rawInput, sensitivity * Time.unscaledDeltaTime);

        if ((rawInput > 0 && _convertedInput < 0) || (rawInput < 0 && _convertedInput > 0))
            _convertedInput = 0;

        return (Mathf.Abs(_convertedInput) < dead) ? 0f : _convertedInput;
    }
    #endregion

    #region Keyboard Input
    private void KBMInputOnEnable()
    {
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            LeftButtonDown();
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            RightButtonDown();

        if (Input.GetKey(KeyCode.LeftShift))
            QDCButtonDown();
    }

    private void KBMInputHandling()
    {
        // Movement
        if (Input.GetKeyDown(KeyCode.A))
            LeftButtonDown();
        else if (Input.GetKeyUp(KeyCode.A))
            LeftButtonUp();

        if (Input.GetKeyDown(KeyCode.D))
            RightButtonDown();
        else if (Input.GetKeyUp(KeyCode.D))
            RightButtonUp();

        // Jump
        if (Input.GetButtonDown("Jump"))
            JumpButtonDown();

        // QDC
        if (Input.GetKeyDown(KeyCode.LeftShift))
            QDCButtonDown();
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            QDCButtonUp();

        // Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseButtonDown();
            GameManager.Instance.ChangePauseState();
        }
        else if (Input.GetKeyUp(KeyCode.Escape))
        {
            PauseButtonUp();
        }

    }
    #endregion

    #region Jump
    protected override void ResetJumpInput()
    {
        if (_jumpReady)
        {
            _jumpInput = false;
            AudioManager.Instance.PlaySound("Player - Jump");

            if (_retainJumpInputRoutine != null)
            {
                StopCoroutine(_retainJumpInputRoutine);

                _retainJumpInputRoutine = null;
            }
        }
        else
        {
            if (_retainJumpInputRoutine == null)
                _retainJumpInputRoutine = StartCoroutine(RetainJumpInput());
        }
    }

    IEnumerator RetainJumpInput()
    {
        yield return new WaitForSeconds(_retainJumpInputDuration);

        _jumpInput = false;
        _retainJumpInputRoutine = null;
    }

    protected override void LandedOnGround(RaycastHit2D hit)
    {
        base.LandedOnGround(hit);

        AudioManager.Instance.PlaySound("Player - Landing");
    }

    protected override void GotJump()
    {
        base.GotJump();

        if (!_jumpInput)
            _jumpButton.SetBool("Button Down", false);
    }
    #endregion

    #region QDC
    private void QuickDirectionChange(float direction)
    {
        _gameObjectRb.velocity = new Vector2(direction * HorizontalVelocityClamp, _gameObjectRb.velocity.y);
        _gameObjectRb.AddTorque(-_moveTorque * direction);
    }
    #endregion

    #region Spawning and Death
    private void Die()
    {
        if (AdManager.Instance.DisplayAdConditions())
        {
            AdManager.Instance.deathCounter++;

            if (AdManager.Instance.deathCounter >= AdManager.Instance.deathThreshold)
            {
                AdManager.Instance.deathCounter = 0;
                AdManager.Instance.DisplayVideoInterstitial();
                AdManager.Instance.RequestVideoInterstitial();
            }
        }

        AudioManager.Instance.PlaySound("Player - Death");
        Respawn();
    }

    private void Respawn()
    {
        GameManager.Instance.spawned = false;

        if (GameManager.Instance.OnRespawnStart != null)
            GameManager.Instance.OnRespawnStart();

        ColorManager.Instance.StartShiftingColors();
        ResetInput();
        StopMovement();
        DisableMoveParticles();
        SetSpawnPosition();
        PlaySpawnAnimation();
        ClearTrails();
        ClearParticles();
    }

    private void ResetInput()
    {
        _horizontalInput = 0;
        _convertedInput = 0;
    }

    private void StopMovement()
    {
        _gameObjectRb.velocity = Vector2.zero;
        _gameObjectRb.angularVelocity = 0;
        _gameObjectRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        transform.eulerAngles = Vector3.zero;
    }

    private void DisableMoveParticles()
    {
        _moveParticlesFolder.SetActive(false);
    }

    private void SetSpawnPosition()
    {
        var currentSpawnPoint = GameObject.Find("Spawn Point").transform.position; // Find through tag in the levels gameobject

        transform.position = currentSpawnPoint;
        _spawnParticlesVortex.transform.position = currentSpawnPoint;
        _spawnParticlesBurst.transform.position = currentSpawnPoint;
    }

    private void PlaySpawnAnimation()
    {
        _spawnParticlesVortex.Play();
        AudioManager.Instance.PlaySound("Player - Spawn Particles Vortex");

        if (_animateToOriginalSizeRoutine != null)
            StopCoroutine(_animateToOriginalSizeRoutine);

        if (_spawnCompleteRoutine != null)
            StopCoroutine(_spawnCompleteRoutine);

        _animateToOriginalSizeRoutine = StartCoroutine(AnimateToOriginalSize());
        _spawnCompleteRoutine = StartCoroutine(SpawnComplete());
    }

    private void ClearTrails()
    {
        foreach (var trail in _trails)
        {
            trail.Clear();
        }
    }

    private void ClearParticles()
    {
        foreach (var particleSystem in _particleSystems)
        {
            particleSystem.Clear();
        }
    }

    IEnumerator AnimateToOriginalSize()
    {
        transform.localScale = new Vector3(0, 0, 1);

        yield return new WaitForSeconds(_delayBeforeScalingPlayer);

        float lerp = 0f;

        while (transform.localScale != _targetScaleForSpawn / 2)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, _targetScaleForSpawn / 2, lerp);
            transform.eulerAngles = Vector3.Lerp(Vector3.zero, _targetRotationForSpawn, lerp);

            lerp += Time.deltaTime / _halfScaleDuration;

            yield return null;
        }

        yield return new WaitUntil(() => GameManager.Instance.spawned == true);

        float currentScaleRate = _initialScaleRateForBounce;

        while (Mathf.Abs(transform.localScale.x - _targetScaleForSpawn.x) > _scaleStopThreshold || Mathf.Abs(currentScaleRate) > 0)
        {
            transform.localScale += new Vector3(currentScaleRate * Time.deltaTime, currentScaleRate * Time.deltaTime, 0);

            currentScaleRate += (_targetScaleForSpawn.x - transform.localScale.x) * _resistingScaleRate * Time.deltaTime;

            currentScaleRate -= currentScaleRate * _scaleEnergyLoss * Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator SpawnComplete()
    {
        yield return new WaitForSeconds(_delayBeforeSpawnParticlesBurst);

        _spawnParticlesBurst.Play();
        AudioManager.Instance.PlaySound("Player - Spawn Particles Burst");

        _gameObjectRb.constraints = RigidbodyConstraints2D.None;

        EnableMoveParticles();

        GameManager.Instance.spawned = true;

        if (GameManager.Instance.OnRespawnComplete != null)
            GameManager.Instance.OnRespawnComplete();
    }

    private void EnableMoveParticles()
    {
        _moveParticlesFolder.SetActive(true);
    }
    #endregion

    #region Colors
    private void SetColors()
    {
        _cubeColor = _staticColorComponent.color;

        ChangeTrailColor();
        ChangeMoveParticlesColor();
    }
    #endregion

    #region Initializing
    private void SetButtonImagesToDefault()
    {
        _leftButton.SetBool("Button Down", false);
        _rightButton.SetBool("Button Down", false);
        _jumpButton.SetBool("Button Down", false);
        _qDCButton.SetBool("Button Down", false);
    }
    #endregion

    #region Game Pause and Resume (Needs Cleaning)
    private void GamePaused()
    {
        // Code to pause here
    }

    private void GameResumed()
    {
        if (GameManager.Instance.onAndroid)
        {
            RightButtonUp();
            LeftButtonUp();
            QDCButtonUp();
            _jumpInput = false;

            SetButtonImagesToDefault();
        }
    }
    #endregion
}
