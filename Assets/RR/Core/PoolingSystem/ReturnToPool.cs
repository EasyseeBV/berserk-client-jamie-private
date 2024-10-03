using UnityEngine;

namespace RR.Core.PoolingSystem
{
    /// <summary>Auto return this Object to Pool. This script assumes that the Object was created via Pool.</summary>
    public class ReturnToPool : MonoBehaviour
    {
        public bool ReturnByTimer = true;
        public float TimeInterval = 1;
        public bool ReturnIfDisabled = true;

        bool returned = false;

        private void Start()
        {
            returned = false;
        }

        void OnEnable()
        {
            returned = false;
            if (ReturnByTimer)
                Invoke("Return", TimeInterval);
        }

        void OnDisable()
        {
            if (ReturnIfDisabled)
                Return();
        }

        void Return()
        {
            if (!returned)
            {
                Pool.Return(gameObject);
                returned = true;
            }
        }
    }
}