using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public abstract class MaterialAnimator<T> : MeshRenderChanger
	{
		[SerializeField] private List<string> propertyNames1 = new();
		[SerializeField] private T value1;

		[SerializeField] private List<string> propertyNames2 = new();
		[SerializeField] private T value2;

		[SerializeField] private List<string> propertyNames3 = new();
		[SerializeField] private T value3;

		[SerializeField] private List<string> propertyNames4 = new();
		[SerializeField] private T value4;

		[SerializeField] private List<string> propertyNames5 = new();
		[SerializeField] private T value5;

		private T prevValue1;
		private T prevValue2;
		private T prevValue3;
		private T prevValue4;
		private T prevValue5;

		private void Update()
		{
			if (HasChanged(prevValue1, value1))
			{
				prevValue1 = value1;
				propertyNames1.ForEach(propertyName => ChangeValue(propertyName, value1));
			}

			if (HasChanged(prevValue2, value2))
			{
				prevValue2 = value2;
				propertyNames2.ForEach(propertyName => ChangeValue(propertyName, value2));
			}

			if (HasChanged(prevValue3, value3))
			{
				prevValue3 = value3;
				propertyNames3.ForEach(propertyName => ChangeValue(propertyName, value3));
			}

			if (HasChanged(prevValue4, value4))
			{
				prevValue4 = value4;
				propertyNames4.ForEach(propertyName => ChangeValue(propertyName, value4));
			}

			if (HasChanged(prevValue5, value5))
			{
				prevValue5 = value5;
				propertyNames5.ForEach(propertyName => ChangeValue(propertyName, value5));
			}
		}

		private void OnEnable()
		{
			if (!MeshRenderers.Any())
			{
				RRLogger.Error($"{nameof(MeshRenderers)} not found");
				return;
			}

			ResetValue();
		}

		[Button]
		private void ResetValue()
		{
			if (propertyNames1.Any())
				prevValue1 = value1 = GetStartingValue(MeshRenderers.First(), propertyNames1.First());
			if (propertyNames2.Any())
				prevValue2 = value2 = GetStartingValue(MeshRenderers.First(), propertyNames2.First());
			if (propertyNames3.Any())
				prevValue3 = value3 = GetStartingValue(MeshRenderers.First(), propertyNames3.First());
			if (propertyNames4.Any())
				prevValue4 = value4 = GetStartingValue(MeshRenderers.First(), propertyNames4.First());
			if (propertyNames5.Any())
				prevValue5 = value5 = GetStartingValue(MeshRenderers.First(), propertyNames5.First());
		}

		protected abstract T GetStartingValue(MeshRenderer mehRenderer, string propertyName);
		protected abstract bool HasChanged(T prevValue, T currentValue);
		protected abstract void ChangeValue(string propertyName, T value);
	}
}