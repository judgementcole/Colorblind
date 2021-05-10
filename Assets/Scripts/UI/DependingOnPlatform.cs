using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DependingOnPlatform : MonoBehaviour
{
    #region Variables
    private enum Platforms
    {
        PC,
        Android
    }

    [Header("Enable depending on platforms")]
    [Tooltip("Enables on the specified platforms. Disables if platform is different than specified platforms")]
    [SerializeField] private Platforms[] _enableOnFollowingPlatforms = new Platforms[1];
    #endregion

    #region Unity Callbacks
    void OnEnable()
    {
        bool shouldBeDisabled = true;

        foreach (var platform in _enableOnFollowingPlatforms)
        {
            switch (platform)
            {
                case Platforms.PC:

                    if (!GameManager.Instance.onAndroid)
                        shouldBeDisabled = false;

                    break;

                case Platforms.Android:

                    if (GameManager.Instance.onAndroid)
                        shouldBeDisabled = false;

                    break;
            }
        }

        if (shouldBeDisabled)
            gameObject.SetActive(false);
    }
    #endregion
}
