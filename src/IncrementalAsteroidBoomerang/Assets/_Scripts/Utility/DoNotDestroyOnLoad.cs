using UnityEngine;

namespace _Scripts.Utility
{
    public class DoNotDestroyOnLoad : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
