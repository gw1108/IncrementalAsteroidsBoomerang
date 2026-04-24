using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.Utility
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object lockObject = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }

        protected Singleton()
        {
            if (_instance != null)
            {
                Debug.LogError($"Singleton of type {typeof(T)} already exists. Cannot create another instance.");
            }
        }

        public static bool HasInstance => _instance != null;

        public static void DestroyInstance()
        {
            _instance = null;
        }
    }
    public abstract class SingletonMonoBehaviour<T> : SerializedMonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                            instance = singletonObject.AddComponent<T>();
                        }
                    }

                    return instance;
                }
            }
        }

        public static bool HasInstance => instance != null;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} already exists. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}