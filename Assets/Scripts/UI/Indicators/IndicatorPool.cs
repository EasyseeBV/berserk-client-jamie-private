using Events;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using Mathf = UnityEngine.Mathf;

namespace UI
{
	public enum IndicatorType
    {
        Damage,
    }

	public class IndicatorPool : SerializedMonoBehaviour
	{
        [SerializeField] private Indicator damageIndicatorPrefab = default;

        private void Awake() => Init();

        private void Init()
        {
            GameBus.OnDamageDealt.Subscribe(this, ShowDamageIndicator);
        }

        private void ShowDamageIndicator(Vector3 targetPos, int damage)
        {
            var indicator = GetIndicator(IndicatorType.Damage);
            indicator.Show(targetPos, Mathf.Abs(damage));
        }

        private Indicator GetIndicator(IndicatorType type)
        {
            var indicator = LeanPool.Spawn(damageIndicatorPrefab, transform);
            indicator.Init(i => LeanPool.Despawn(i));
            return indicator;
        }
    }
}
