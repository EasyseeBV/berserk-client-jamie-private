using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using Lean.Pool;

namespace UI
{
	public class Indicator : MonoBehaviour, IPoolable
	{
        [SerializeField] private TextMeshProUGUI valueText = default;
        [SerializeField] private float animationOffsetY = 100;

        private Action<GameObject> onRelease;

        public void Init(Action<GameObject> releaseCallback)
        {
            onRelease = releaseCallback;
        }

        public void Show(Vector3 position, int value)
        {
            valueText.SetText($"{value}");

            transform.position = position;
            var y = transform.localPosition.y;
            transform.localScale = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
            transform.DOLocalMoveY(y + animationOffsetY, 1f).SetDelay(0.5f).SetEase(Ease.InQuad);
            transform.DOScale(Vector3.zero, 0.5f).SetDelay(1f).SetEase(Ease.InQuad)
                .OnComplete(() => onRelease.Invoke(gameObject));
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawn()
        {
            gameObject.SetActive(false);
        }
    }
}
