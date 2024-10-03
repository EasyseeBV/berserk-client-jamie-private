using UnityEngine;

namespace RR.Game.ParallaxSystem
{
    public class ParallaxElement : MonoBehaviour
    {
        [Range(-2, 2)]
        public float XFactor;
        [Range(-2, 2)]
        public float YFactor;

        private float randomFactor;

        private void Awake()
        {
            randomFactor = Random.Range(0.8f, 1.5f);
        }

        public void Move(float deltaX, float deltaY)
        {
            var newPos = transform.position;

            newPos.x -= deltaX * XFactor * randomFactor;
            newPos.y -= deltaY * YFactor * randomFactor;

            transform.position = newPos;
        }
    }
}