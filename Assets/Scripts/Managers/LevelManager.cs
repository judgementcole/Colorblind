using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoSingleton<LevelManager>
{
    #region Levels Class
    [System.Serializable]
    public class Levels
    {
        public GameObject level;
        public bool displayAdOnThisLevel = true;
    }
    #endregion

    #region Variables
    [Header("Levels")]
    public Levels[] levels;
    [HideInInspector] public int currentLevel;
    public Action OnLevelSet;

    [Header("Customization Level")]
    public GameObject customizationLevel;
    #endregion

    #region Levels
    public void OnLevelClear()
    {
        AudioManager.Instance.PlaySound("Levels - Goal Reached");

        if (AdManager.Instance.DisplayAdConditions() && levels[currentLevel].displayAdOnThisLevel)
        {
            AdManager.Instance.DisplayImageInterstitial();
            AdManager.Instance.RequestImageInterstitial();
        }

        if (GameManager.Instance.OnLevelClear != null)
            GameManager.Instance.OnLevelClear();

        GameManager.Instance.Save();
        SetActiveLevel();
    }

    public void SetActiveLevel()
    {
        levels[currentLevel].level.SetActive(false);

        if (PlayerPrefs.GetInt("Level") == levels.Length)
        {
            // All Levels Cleared
            PlayerPrefs.SetInt("Level", 0);
            GameManager.Instance.ReturnToMainMenu();
            return;
        }

        currentLevel = PlayerPrefs.GetInt("Level");
        levels[currentLevel].level.SetActive(true);

        EnemyManager.Instance.GetEnemyColliders();
        ColorManager.Instance.GetBackgroundColorShiftComponents();

        if (OnLevelSet != null)
            OnLevelSet();    
    }
    #endregion

    #region Customization Level
    public void SetActiveCustomizationLevel()
    {
        customizationLevel.SetActive(true);

        ColorManager.Instance.GetBackgroundColorShiftComponents();
    }
    #endregion
}
