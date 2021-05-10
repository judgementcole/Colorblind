using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log(typeof(T) + " is NULL");

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _instance = this as T;
    }
}