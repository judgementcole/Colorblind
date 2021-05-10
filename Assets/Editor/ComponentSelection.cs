using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ComponentSelection : ScriptableWizard
{
    // Works only for enabled objects (Needs research to work on disabled objects)

    #region Variables
    private enum Components
    {
        SpriteRenderer,
        Rigidbody2D,
        BoxCollider2D,
        Tilemap,
        RectTransform,
        Button,

        // Only in "Colorblind"
        BackgroundColorShift
    }

    [Header("Components")]
    [SerializeField] private Components _componentToSelect;
    #endregion

    #region Unity Callbacks
    [MenuItem("My Tools/Component Selection Tool")]
    static void CreateWizard()
    {
        DisplayWizard<ComponentSelection>("Select Component in Children", "Select");
    }

    private void OnWizardCreate()
    {
        GameObject[] selectedGameObjects = Selection.gameObjects;
        Selection.activeGameObject = null;
        List<Object> objectsWithSpecifiedComponent = new List<Object>();

        switch (_componentToSelect)
        {
            #region For General Use
            case Components.SpriteRenderer:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<SpriteRenderer>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;

            case Components.Rigidbody2D:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<Rigidbody2D>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;

            case Components.BoxCollider2D:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<BoxCollider2D>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;

            case Components.Tilemap:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<Tilemap>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;

            case Components.RectTransform:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<RectTransform>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;

            case Components.Button:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<Button>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;
            #endregion

            #region For Colorblind Only
            case Components.BackgroundColorShift:

                foreach (var gameObject in selectedGameObjects)
                {
                    var gameObjectsWithComponent = gameObject.GetComponentsInChildren<BackgroundColorShift>();

                    foreach (var gameObjectWithComponent in gameObjectsWithComponent)
                    {
                        objectsWithSpecifiedComponent.Add(gameObjectWithComponent.gameObject);
                    }
                }

                break;
            #endregion
        }

        Selection.objects = objectsWithSpecifiedComponent.ToArray();
    }
    #endregion
}
