using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    #region Variables
    [Header("Main Menu")]
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _mainMenuButtons;
    [SerializeField] private GameObject _confirmSelectionButtons;
    [SerializeField] private CompositeCollider2D _mainMenuConfiner;
    [SerializeField] private RectTransform _customizeAndSettingsButtons;
    [SerializeField] private CanvasGroup _mainMenuCG;
    [SerializeField] private GraphicRaycaster _mainMenuGR;
    private float _customizeButtonYOffset = -1.249f;
    public Action OnReturnToMainMenu;

    [Header("Player")]
    public GameObject player;
    [HideInInspector] public bool spawned;
    public Action OnRespawnStart;
    public Action OnRespawnComplete;
    public Action OnLevelClear;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] private CinemachineConfiner cameraConfiner;

    [Header("Phone Controls")]
    // Needs to be dealt seperately from DependingOnPlatform Script because it needs to 
    // be enabled and disabled again and again on the same platform
    [SerializeField] private GameObject _phoneControls;
    [SerializeField] private CanvasGroup _phoneControlsCG;
    [SerializeField] private GraphicRaycaster _phoneControlsGR;
    [HideInInspector] public bool onAndroid;

    [Header("Pause")]
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Image _pauseBackgroundImage;
    [SerializeField] private CanvasGroup _pauseMenuCG;
    [SerializeField] private GraphicRaycaster _pauseMenuGR;
    [SerializeField] private GameObject _backToMainMenuText;
    [SerializeField] private GameObject _exitWithoutSavingText;
    private float _pauseEaseDuration = 0.4f;
    private float _pauseBackgroundDefaultAlpha;
    private bool _paused;
    public Action OnGameResumed;
    private Coroutine _pauseTransitionRoutine;
    private enum EaseType
    {
        easeOut,
        easeIn
    }

    [Header("Settings")]
    [SerializeField] private GameObject _settingsMenu;
    [SerializeField] private CanvasGroup _settingsDarkThemeMenuCG;
    [SerializeField] private CanvasGroup _settingsLightThemeMenuCG;
    [SerializeField] private GraphicRaycaster _settingsMenuGR;
    private bool _inSettingsMenu;
    private float _menuTransitionsDuration = 0.2f;
    private enum FadeType
    {
        fadeIn,
        fadeOut
    }
    private enum ButtonsTheme
    {
        darkTheme,
        lightTheme
    }

    [Header("Customization Level")]
    [SerializeField] private GameObject _colorPicker;
    private float _playerConfinerThickness = 4f;
    private float _playerConfinerZPos = 0f;
    [HideInInspector] public bool _inCustomizationScreen;
    private bool _customizationLevelPlayerConfinersExist;
    private Vector2 _screenResoluton;
    private Vector2 _screenSize; // Used for cuztomization level confiners (DO NOT TOUCH)
    private GameObject _playerConfinersFolder;
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        base.Awake();

        if (PlayerPrefs.HasKey("Custom Colors Saved"))
            LoadColorPalette();

        CheckDevice();
    }

    private void Start()
    {
        AudioManager.Instance.PlaySound("Music");

        _screenResoluton = new Vector2(Screen.width, Screen.height);

        InitializePauseMenu();
        ReturnToMainMenu();
    }

    private void OnEnable()
    {
        LevelManager.Instance.OnLevelSet += SetCinemachineConfiner;
    }

    private void OnDisable()
    {
        LevelManager.Instance.OnLevelSet -= SetCinemachineConfiner;
    }
    #endregion

    #region Platform
    private void CheckDevice()
    {
        if (Application.platform == RuntimePlatform.Android)
            onAndroid = true;

        _phoneControls.SetActive(false);
    }
    #endregion

    #region Save Game / Load Game
    public void Save()
    {
        if (PlayerPrefs.GetInt("Level") == LevelManager.Instance.levels.Length)
        {
            // Last Level already
            PlayerPrefs.SetInt("Level", 0);
        }
        else
        {
            PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        }
    }
    #endregion

    #region Color Palette
    private void OverwriteColorPalette()
    {
        PlayerPrefs.SetInt("Custom Colors Saved", 0);

        SaveColorRGBValues("Custom Player Color", Colors.colorName.player);
        SaveColorRGBValues("Custom Background Color One", Colors.colorName.backgroundColorOne);
        SaveColorRGBValues("Custom Background Color Two", Colors.colorName.backgroundColorTwo);
        SaveColorRGBValues("Custom Background Color Three", Colors.colorName.backgroundColorThree);
        SaveColorRGBValues("Custom Enemy Color", Colors.colorName.enemy);
    }

    private void SaveColorRGBValues(string saveFileColorName, Colors.colorName colorName)
    {
        Color color = Colors.ConvertEnumToColor(colorName);

        PlayerPrefs.SetFloat(saveFileColorName + " R", color.r);
        PlayerPrefs.SetFloat(saveFileColorName + " G", color.g);
        PlayerPrefs.SetFloat(saveFileColorName + " B", color.b);
    }

    private void LoadColorPalette()
    {
        Colors.playerColor = new Color(PlayerPrefs.GetFloat("Custom Player Color R"), PlayerPrefs.GetFloat("Custom Player Color G"), PlayerPrefs.GetFloat("Custom Player Color B"));
        Colors.enemyColor = new Color(PlayerPrefs.GetFloat("Custom Enemy Color R"), PlayerPrefs.GetFloat("Custom Enemy Color G"), PlayerPrefs.GetFloat("Custom Enemy Color B"));
        Colors.backgroundColorOne = new Color(PlayerPrefs.GetFloat("Custom Background Color One R"), PlayerPrefs.GetFloat("Custom Background Color One G"), PlayerPrefs.GetFloat("Custom Background Color One B"));
        Colors.backgroundColorTwo = new Color(PlayerPrefs.GetFloat("Custom Background Color Two R"), PlayerPrefs.GetFloat("Custom Background Color Two G"), PlayerPrefs.GetFloat("Custom Background Color Two B"));
        Colors.backgroundColorThree = new Color(PlayerPrefs.GetFloat("Custom Background Color Three R"), PlayerPrefs.GetFloat("Custom Background Color Three G"), PlayerPrefs.GetFloat("Custom Background Color Three B"));
    }
    #endregion

    #region Main Menu
    private void StartGame()
    {
        LevelManager.Instance.SetActiveLevel();

        player.SetActive(true);
        _mainMenu.SetActive(false);

        if (onAndroid)
            _phoneControls.SetActive(true);

        _phoneControlsCG.alpha = 1;
        _phoneControlsGR.enabled = true;

        virtualCam.Follow = player.transform;

        if (AdManager.Instance.DisplayAdConditions())
            AdManager.Instance.HideBanner();
    }

    public void ReturnToMainMenu()
    {
        if (_paused)
        {
            // Back to Main Menu Button has been pressed
            if (_pauseTransitionRoutine != null)
                StopCoroutine(_pauseTransitionRoutine);

            Time.timeScale = 1;
            LevelManager.Instance.levels[LevelManager.Instance.currentLevel].level.SetActive(false);

            InitializePauseMenu();

            if (_inCustomizationScreen)
            {
                // Exit Without Saving Button has been pressed
                ExitCustomizationLevelWithoutSaving();
            }

            ColorManager.Instance.OnFCPColorChange();

            AudioManager.Instance.PlaySound("Levels - Goal Reached");
        }

        player.SetActive(false);
        _confirmSelectionButtons.SetActive(false);
        _colorPicker.SetActive(false);
        LevelManager.Instance.customizationLevel.SetActive(false);
        _mainMenu.SetActive(true);
        _mainMenuButtons.SetActive(true);

        virtualCam.Follow = _mainMenu.transform;
        cameraConfiner.m_BoundingShape2D = _mainMenuConfiner;

        // Has to be done before setting customize button position
        DisplayContinueButton();

        SetCustomizeButtonPosition();

        if (ColorManager.Instance.colorShiftRoutine != null)
            ColorManager.Instance.StopCoroutine(ColorManager.Instance.colorShiftRoutine);

        if (onAndroid)
            _phoneControls.SetActive(false);

        if (AdManager.Instance.DisplayAdConditions())
            AdManager.Instance.RequestBanner();

        if (OnReturnToMainMenu != null)
            OnReturnToMainMenu();
    }

    public void SetCinemachineConfiner()
    {
        GameObject gameObjectToSearchIn = LevelManager.Instance.levels[LevelManager.Instance.currentLevel].level;
        cameraConfiner.m_BoundingShape2D = UtilityHelper.FindChildWithTag(gameObjectToSearchIn, "Camera Confiner", true).GetComponent<CompositeCollider2D>();
    }

    private void DisplayContinueButton()
    {
        if (PlayerPrefs.GetInt("Level") != 0)
            _continueButton.SetActive(true);
        else
            _continueButton.SetActive(false);
    }

    public void NewGame()
    {
        if (PlayerPrefs.GetInt("Level") == 0)
        {
            ConfirmNewGame();
        }
        else
        {
            _mainMenuButtons.SetActive(false);
            _confirmSelectionButtons.SetActive(true);
        }
    }

    public void ConfirmNewGame()
    {
        PlayerPrefs.SetInt("Level", 0);
        StartGame();
    }

    public void Continue()
    {
        StartGame();
    }

    public void OnButtonPress()
    {
        AudioManager.Instance.PlaySound("Main Menu - Button Press");
    }

    IEnumerator MainMenuButtonsTransitionFade(float targetCGAlpha, FadeType fadeType, bool openSettingsMenu) // Needs Cleaning
    {
        if (fadeType == FadeType.fadeIn)
        {
            _mainMenuCG.gameObject.SetActive(true);
            _mainMenuGR.enabled = true;
        }
        else if (fadeType == FadeType.fadeOut)
        {
            _mainMenuGR.enabled = false;
        }

        float initialCGAlpha = _mainMenuCG.alpha;

        float calculatedPauseEaseDuration = Mathf.Abs(_mainMenuCG.alpha - targetCGAlpha) * _pauseEaseDuration;

        float lerp = 0f;

        while (_mainMenuCG.alpha != targetCGAlpha)
        {
            _mainMenuCG.alpha = Mathf.Lerp(initialCGAlpha, targetCGAlpha, lerp);

            lerp += Time.unscaledDeltaTime / _menuTransitionsDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }

        if (fadeType == FadeType.fadeOut)
        {
            _mainMenuCG.gameObject.SetActive(false);

            if (openSettingsMenu)
            {
                StartCoroutine(SettingsButtonsMenuTransitionFade(1, FadeType.fadeIn, false, false, ButtonsTheme.lightTheme));
            }
        }
    }
    #endregion

    #region Customization Level
    private void SetCustomizeButtonPosition()
    {
        _customizeAndSettingsButtons.anchoredPosition = _continueButton.GetComponent<RectTransform>().anchoredPosition;

        if (_continueButton.activeSelf == true)
            _customizeAndSettingsButtons.anchoredPosition += new Vector2(0, _customizeButtonYOffset);
    }

    public void EnterCustomizationLevel()
    {
        LevelManager.Instance.SetActiveCustomizationLevel();

        // Needs to be done after customization level is set to active
        if (!_customizationLevelPlayerConfinersExist)
        {
            MakeCollidersBasedOnCameraSize();
            _customizationLevelPlayerConfinersExist = true;
        }
        else if (_screenResoluton != new Vector2(Screen.width, Screen.height))
        {
            UtilityHelper.DestroyAllChildren(_playerConfinersFolder.transform);
            MakeCollidersBasedOnCameraSize();

            _screenResoluton = new Vector2(Screen.width, Screen.height);
        }

        player.SetActive(true);
        _colorPicker.SetActive(true);
        _mainMenu.SetActive(false);

        // Change back to main menu text
        _backToMainMenuText.SetActive(false);
        _exitWithoutSavingText.SetActive(true);

        if (onAndroid)
            _phoneControls.SetActive(true);

        _phoneControlsCG.alpha = 1;
        _phoneControlsGR.enabled = true;

        if (AdManager.Instance.DisplayAdConditions())
            AdManager.Instance.HideBanner();

        _inCustomizationScreen = true;
    }

    public void SaveAndExitCustomizationLevel()
    {
        _inCustomizationScreen = false;

        AudioManager.Instance.PlaySound("Levels - Goal Reached");

        OverwriteColorPalette();
        ColorManager.Instance.OnFCPColorChange();

        // Change back to main menu text
        _backToMainMenuText.SetActive(true);
        _exitWithoutSavingText.SetActive(false);
    }

    public void ExitCustomizationLevelWithoutSaving()
    {
        _inCustomizationScreen = false;

        LoadColorPalette();
        ColorManager.Instance.OnFCPColorChange();

        // Change back to main menu text
        _backToMainMenuText.SetActive(true);
        _exitWithoutSavingText.SetActive(false);
    }

    public void MakeCollidersBasedOnCameraSize()
    {
        // Find the player confiners folder game object
        _playerConfinersFolder = UtilityHelper.FindChildWithTag(LevelManager.Instance.customizationLevel, "Player Confiner", true);

        //Create a Dictionary to contain all our Objects/Transforms
        Dictionary<string, Transform> colliders = new Dictionary<string, Transform>();
        //Create our GameObjects and add their Transform components to the Dictionary we created above
        colliders.Add("Top", new GameObject().transform);
        //colliders.Add("Bottom", new GameObject().transform); // Bottom Collider Removed
        colliders.Add("Right", new GameObject().transform);
        colliders.Add("Left", new GameObject().transform);
        //Generate world space point information for position and scale calculations
        Vector3 cameraPos = Camera.main.transform.position;
        _screenSize.x = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0))) * 0.5f; //Grab the world-space position values of the start and end positions of the screen, then calculate the distance between them and store it as half, since we only need half that value for distance away from the camera to the edge
        _screenSize.y = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height))) * 0.5f;
        //For each Transform/Object in our Dictionary
        foreach (KeyValuePair<string, Transform> valPair in colliders)
        {
            var boxCollider = valPair.Value.gameObject.AddComponent<BoxCollider2D>(); //Add our colliders. Remove the "2D", if you would like 3D colliders.
            boxCollider.usedByComposite = true; // Used by effector after being assogned to composite
            valPair.Value.name = valPair.Key + "Collider"; //Set the object's name to it's "Key" name, and take on "Collider".  i.e: TopCollider
            valPair.Value.parent = _playerConfinersFolder.transform; //Make the object a child of whatever object this script is on (preferably the camera)

            if (valPair.Key == "Left" || valPair.Key == "Right") //Scale the object to the width and height of the screen, using the world-space values calculated earlier
                valPair.Value.localScale = new Vector3(_playerConfinerThickness, _screenSize.y * 2, _playerConfinerThickness);
            else
                valPair.Value.localScale = new Vector3(_screenSize.x * 2, _playerConfinerThickness, _playerConfinerThickness);
        }
        //Change positions to align perfectly with outter-edge of screen, adding the world-space values of the screen we generated earlier, and adding/subtracting them with the current camera position, as well as add/subtracting half out objects size so it's not just half way off-screen
        colliders["Right"].position = new Vector3(cameraPos.x + _screenSize.x + (colliders["Right"].localScale.x * 0.5f), cameraPos.y, _playerConfinerZPos);
        colliders["Left"].position = new Vector3(cameraPos.x - _screenSize.x - (colliders["Left"].localScale.x * 0.5f), cameraPos.y, _playerConfinerZPos);
        colliders["Top"].position = new Vector3(cameraPos.x, cameraPos.y + _screenSize.y + (colliders["Top"].localScale.y * 0.5f), _playerConfinerZPos);
        //colliders["Bottom"].position = new Vector3(cameraPos.x, cameraPos.y - screenSize.y - (colliders["Bottom"].localScale.y * 0.5f), zPosition); // Bottom Collider Removed
    }
    #endregion

    #region Pause Menu (Needs Cleaning)
    private void InitializePauseMenu()
    {
        _pauseBackgroundDefaultAlpha = _pauseBackgroundImage.color.a;

        Color _pauseBackgroundcurrentColor = _pauseBackgroundImage.color;
        _pauseBackgroundcurrentColor.a = 0;
        _pauseBackgroundImage.color = _pauseBackgroundcurrentColor;

        _pauseMenuCG.alpha = 0;

        _pauseMenuGR.enabled = false;
        _paused = false;
        _pauseMenu.SetActive(false);

        // Change back to main menu text
        _backToMainMenuText.SetActive(true);
        _exitWithoutSavingText.SetActive(false);
    }

    public void ChangePauseState()
    {
        if (!_paused)
            Pause();
        else if (_paused)
            Unpause();
    }

    private void Pause()
    {
        _pauseMenu.SetActive(true);
        _pauseMenuGR.enabled = true;
        _phoneControlsGR.enabled = false;
        _paused = true;

        if (_pauseTransitionRoutine != null)
            StopCoroutine(_pauseTransitionRoutine);

        _pauseTransitionRoutine = StartCoroutine(PauseTransition(0, _pauseBackgroundDefaultAlpha, 1, EaseType.easeIn, 2));
    }

    public void Unpause()
    {
        if (!_inSettingsMenu)
        {
            _pauseMenuGR.enabled = false;
            _phoneControlsGR.enabled = true;
            _paused = false;

            if (_pauseTransitionRoutine != null)
                StopCoroutine(_pauseTransitionRoutine);

            _pauseTransitionRoutine = StartCoroutine(PauseTransition(1, 0, 0, EaseType.easeOut, 2));

            if (OnGameResumed != null)
                OnGameResumed();
        }
    }

    IEnumerator PauseTransition(float targetTimeScale, float targetPauseBackgroundAlpha, float targetCGAlpha, EaseType easeType, int easeToThePower)
    {
        float initialTimeScale = Time.timeScale;
        float initialPauseBackgroundAlpha = _pauseBackgroundImage.color.a;
        float initialCGAlpha = _pauseMenuCG.alpha;

        float calculatedPauseEaseDuration = Mathf.Abs(Time.timeScale - targetTimeScale) * _pauseEaseDuration;

        Color _pauseBackgroundcurrentColor = _pauseBackgroundImage.color;

        float lerp = 0f;

        while (Time.timeScale != targetTimeScale)
        {
            if (easeType == EaseType.easeIn)
                Time.timeScale = Mathf.Lerp(initialTimeScale, targetTimeScale, Mathf.Pow(lerp, easeToThePower));
            else if (easeType == EaseType.easeOut)
                Time.timeScale = Mathf.Lerp(initialTimeScale, targetTimeScale, 1 - Mathf.Pow(1 - lerp, easeToThePower));

            _pauseMenuCG.alpha = Mathf.Lerp(initialCGAlpha, targetCGAlpha, lerp);
            _phoneControlsCG.alpha = Mathf.Lerp(targetCGAlpha, initialCGAlpha, lerp * 3);

            _pauseBackgroundcurrentColor.a = Mathf.Lerp(initialPauseBackgroundAlpha, targetPauseBackgroundAlpha, lerp);
            _pauseBackgroundImage.color = _pauseBackgroundcurrentColor;

            lerp += Time.unscaledDeltaTime / calculatedPauseEaseDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }

        // For Resuming
        if (!_paused && _pauseMenu.activeSelf)
            _pauseMenu.SetActive(false);
    }

    IEnumerator PauseButtonsMenuTransitionFade(float targetCGAlpha, FadeType fadeType, bool openSettings)
    {
        if (fadeType == FadeType.fadeIn)
        {
            _pauseMenuCG.gameObject.SetActive(true);
            _pauseMenuGR.enabled = true;
        }
        else if (fadeType == FadeType.fadeOut)
        {
            _pauseMenuGR.enabled = false;
        }

        float initialCGAlpha = _pauseMenuCG.alpha;

        float calculatedPauseEaseDuration = Mathf.Abs(_pauseMenuCG.alpha - targetCGAlpha) * _pauseEaseDuration;

        float lerp = 0f;

        while (_pauseMenuCG.alpha != targetCGAlpha)
        {
            _pauseMenuCG.alpha = Mathf.Lerp(initialCGAlpha, targetCGAlpha, lerp);

            lerp += Time.unscaledDeltaTime / _menuTransitionsDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }

        if (fadeType == FadeType.fadeOut)
        {
            _pauseMenuCG.gameObject.SetActive(false);

            if (openSettings)
            {
                StartCoroutine(SettingsButtonsMenuTransitionFade(1, FadeType.fadeIn, false, false, ButtonsTheme.darkTheme));
            }
        }
    }
    #endregion

    #region Settings Menu (Needs Cleaning)
    private void InitializeSettingMenu()
    {
        _settingsDarkThemeMenuCG.alpha = 0;
        _settingsLightThemeMenuCG.alpha = 0;

        _settingsMenuGR.enabled = false;
        _inSettingsMenu = false;
        _settingsMenu.SetActive(false);
        _settingsDarkThemeMenuCG.gameObject.SetActive(false);
        _settingsLightThemeMenuCG.gameObject.SetActive(false);
    }

    public void TransitionIntoSettingsMenu()
    {
        _settingsMenu.SetActive(true);
        _inSettingsMenu = true;

        if (_paused)
            StartCoroutine(PauseButtonsMenuTransitionFade(0, FadeType.fadeOut, true));
        else
            StartCoroutine(MainMenuButtonsTransitionFade(0, FadeType.fadeOut, true));
    }

    public void TransitionOutOfSettingsMenu()
    {
        if (_paused)
            StartCoroutine(SettingsButtonsMenuTransitionFade(0, FadeType.fadeOut, true, false, ButtonsTheme.darkTheme));
        else
            StartCoroutine(SettingsButtonsMenuTransitionFade(0, FadeType.fadeOut, false, true, ButtonsTheme.lightTheme));
    }

    IEnumerator SettingsButtonsMenuTransitionFade(float targetCGAlpha, FadeType fadeType, bool openPauseMenu, bool openMainMenu, ButtonsTheme buttonsTheme)
    {
        if (buttonsTheme == ButtonsTheme.darkTheme)
        {
            if (fadeType == FadeType.fadeIn)
            {
                _settingsDarkThemeMenuCG.gameObject.SetActive(true);
                _settingsMenuGR.enabled = true;
            }
            else if (fadeType == FadeType.fadeOut)
            {
                _settingsMenuGR.enabled = false;
            }

            float initialCGAlpha = _settingsDarkThemeMenuCG.alpha;

            float calculatedPauseEaseDuration = Mathf.Abs(_settingsDarkThemeMenuCG.alpha - targetCGAlpha) * _pauseEaseDuration;

            float lerp = 0f;

            while (_settingsDarkThemeMenuCG.alpha != targetCGAlpha)
            {
                _settingsDarkThemeMenuCG.alpha = Mathf.Lerp(initialCGAlpha, targetCGAlpha, lerp);

                lerp += Time.unscaledDeltaTime / _menuTransitionsDuration;

                if (lerp > 1)
                    lerp = 1;

                yield return null;
            }

            if (fadeType == FadeType.fadeOut)
            {
                _settingsDarkThemeMenuCG.gameObject.SetActive(false);

                if (openPauseMenu)
                {
                    StartCoroutine(PauseButtonsMenuTransitionFade(1, FadeType.fadeIn, false));
                }

                if (openMainMenu)
                {
                    StartCoroutine(MainMenuButtonsTransitionFade(1, FadeType.fadeIn, false));
                }

                InitializeSettingMenu();
            }
        }
        else if (buttonsTheme == ButtonsTheme.lightTheme)
        {
            if (fadeType == FadeType.fadeIn)
            {
                _settingsLightThemeMenuCG.gameObject.SetActive(true);
                _settingsMenuGR.enabled = true;
            }
            else if (fadeType == FadeType.fadeOut)
            {
                _settingsMenuGR.enabled = false;
            }

            float initialCGAlpha = _settingsLightThemeMenuCG.alpha;

            float calculatedPauseEaseDuration = Mathf.Abs(_settingsLightThemeMenuCG.alpha - targetCGAlpha) * _pauseEaseDuration;

            float lerp = 0f;

            while (_settingsLightThemeMenuCG.alpha != targetCGAlpha)
            {
                _settingsLightThemeMenuCG.alpha = Mathf.Lerp(initialCGAlpha, targetCGAlpha, lerp);

                lerp += Time.unscaledDeltaTime / _menuTransitionsDuration;

                if (lerp > 1)
                    lerp = 1;

                yield return null;
            }

            if (fadeType == FadeType.fadeOut)
            {
                _settingsLightThemeMenuCG.gameObject.SetActive(false);

                if (openPauseMenu)
                {
                    StartCoroutine(PauseButtonsMenuTransitionFade(1, FadeType.fadeIn, false));
                }

                if (openMainMenu)
                {
                    StartCoroutine(MainMenuButtonsTransitionFade(1, FadeType.fadeIn, false));
                }

                InitializeSettingMenu();
            }
        }
    }
    #endregion
}