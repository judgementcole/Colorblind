using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityHelper
{
    #region Finding Parent/Children with Tags
    public static GameObject FindChildWithTag(GameObject parent, string tagName, bool debugIfNotFound)
    {
        var children = parent.GetComponentsInChildren<Transform>();

        foreach(var child in children)
        {
            if (child.CompareTag(tagName))
            {
                return child.gameObject;
            }
        }

        if (debugIfNotFound)
            Debug.Log("Child not found");

        return null;
    }

    public static GameObject[] FindChildrenWithTag(GameObject parent, string tagName, bool debugIfNotFound)
    {
        var children = parent.GetComponentsInChildren<Transform>();
        List<GameObject> childrenToReturn = new List<GameObject>(); 

        foreach (var child in children)
        {
            if (child.CompareTag(tagName))
            {
                childrenToReturn.Add(child.gameObject);
            }
        }

        if (childrenToReturn.Count > 0)
        {
            return childrenToReturn.ToArray();
        }

        if (debugIfNotFound)
            Debug.Log("Children not found");

        return null;
    }

    public static GameObject FindParentWithTag(GameObject child, string tagName, bool debugIfNotFound)
    {
        GameObject gameObjectBeingChecked = child;

        while (gameObjectBeingChecked.transform.parent != null)
        {
            var parent = gameObjectBeingChecked.transform.parent.gameObject;

            if (parent.CompareTag(tagName))
            {
                return parent;
            }
            else
            {
                gameObjectBeingChecked = parent;
            }
        }

        if (debugIfNotFound)
            Debug.Log("Parent not found");

        return null;
    }
    #endregion

    #region Enabling/Disabling all specified components
    public static void ChangeStateOfBoxColliders(BoxCollider2D[] boxColliders, bool state)
    {
        foreach (var collider in boxColliders)
        {
            collider.enabled = state;
        }
    }
    #endregion

    #region Destroy All Specified Game Objects
    public static void DestroyAllChildren(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    #endregion
}
