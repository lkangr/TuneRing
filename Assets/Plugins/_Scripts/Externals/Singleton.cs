using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _ins;

    public static T Ins
    {
        get
        {
            if (_ins == null)
            {
                _ins = FindObjectOfType(typeof(T)) as T;

                if (_ins == null)
                {
                    _ins = new GameObject().AddComponent<T>();
                    _ins.gameObject.name = "[Singleton] " + _ins.GetType().Name;
                }
            }
            return _ins;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Reset()
    {
        _ins = null;
    }

    public static bool Exists()
    {
        return (_ins != null);
    }
}