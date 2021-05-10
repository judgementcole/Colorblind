using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveFileEditor : ScriptableWizard
{
    #region Variables
    [Header("Save File Info")]
    [SerializeField] private string _saveFileName = "Level";
    [SerializeField] private int _intValue;
    #endregion

    #region Unity Callbacks
    [MenuItem("My Tools/Save File Editor")]
    static void CreateWizard()
    {
        DisplayWizard<SaveFileEditor>("Player Prefs Editor (Only works for int Player Prefs", "Confirm");
    }

    private void OnWizardCreate()
    {
        PlayerPrefs.SetInt(_saveFileName, _intValue);
    }
    #endregion
}
