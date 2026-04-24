using UnityEngine;

namespace _Scripts.Utility
{
    public class SpriteSwapper : MonoBehaviour
    {
        public Sprite spriteA;
        public Sprite spriteB;
        public float interval = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private float _timer;
        private bool _showingA = true;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < interval)
                return;

            _timer -= interval;
            _showingA = !_showingA;
            _spriteRenderer.sprite = _showingA ? spriteA : spriteB;
        }
    }
}
