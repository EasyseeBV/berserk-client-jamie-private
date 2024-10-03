using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Game.SettingSystem
{
	[CreateAssetMenu(fileName = "GameSettings", menuName = "RR/GameSettings/SettingsFile", order = 1)]
	public class GameSettings : ScriptableObject
	{
		[Space]
		[TabGroup("Settings", Order = 1)]
		[LabelText("Groups")]
		[ListDrawerSettings(Expanded = true)]
		public List<SettingParameterGroup> ParameterGroups = new List<SettingParameterGroup>();

		#region Inspector

#if UNITY_EDITOR

		public Action OnChanged;

		private void OnValidate()
		{
			// todo: 18/01/20 Denis P: seems incorrect to trigger this here
			//OnChanged?.Invoke();
		}

#endif

		#endregion Inspector
	}

	[Serializable]
	public class SettingParameterGroup
	{
		[FoldoutGroup("$Name", false)]
		[DelayedProperty]
		[LabelWidth(100)]
		public string Name = "NewGroup";

		[FoldoutGroup("$Name", false)]
		[LabelWidth(100)]
		public string Description;

		[FoldoutGroup("$Name", false)]
		[TableList(AlwaysExpanded = true)]
		[HideLabel]
		public List<SettingParameter> Parameters = new List<SettingParameter>();

#if UNITY_EDITOR

		[ValidateInput(nameof(ValidateName), SettingsHelper.KeyValidateMessage)]
		private bool ValidateName(string name)
		{
			bool result = SettingsHelper.ValidateKey(name, out var fixedName, true);

			Name = fixedName;

			return result;
		}

#endif
	}

	[Serializable]
	public class SettingParameter
	{
		[HideInInspector] public string Name;

		[HideInInspector] public SerializedValue ParameterValue;

		[HideInInspector] public string Description;

		#region Inspector

#if UNITY_EDITOR

		[ShowInInspector]
		[DelayedProperty]
		[HideLabel]
		[ValidateInput(nameof(ValidateName), SettingsHelper.KeyValidateMessage)]
		private string name
		{
			get => Name;
			set => Name = value;
		}

		[VerticalGroup("Value")]
		[ShowIf(nameof(type), Value = SerializedValueType.Bool)]
		[HideLabel]
		[ShowInInspector]
		private bool boolValue
		{
			get => ParameterValue.BoolValue;
			set => ParameterValue.BoolValue = value;
		}

		[VerticalGroup("Value")]
		[ShowIf(nameof(type), Value = SerializedValueType.Int)]
		[HideLabel]
		[ShowInInspector]
		private int intValue
		{
			get => ParameterValue.IntValue;
			set => ParameterValue.IntValue = value;
		}

		[VerticalGroup("Value")]
		[ShowIf(nameof(type), Value = SerializedValueType.Float)]
		[HideLabel]
		[ShowInInspector]
		private float floatValue
		{
			get => ParameterValue.FloatValue;
			set => ParameterValue.FloatValue = value;
		}

		[VerticalGroup("Value")]
		[ShowIf(nameof(type), Value = SerializedValueType.String)]
		[HideLabel]
		[ShowInInspector]
		private string stringValue
		{
			get => ParameterValue.StringValue;
			set => ParameterValue.StringValue = value;
		}

		[PropertyOrder(9)]
		[HideLabel]
		[ShowInInspector]
		public string description
		{
			get => Description;
			set => Description = value;
		}

		[HideLabel]
		[TableColumnWidth(70, false)]
		[ShowInInspector]
		[PropertyOrder(10)]
		private SerializedValueType type
		{
			get => ParameterValue.ValueType;
			set => ParameterValue.ValueType = value;
		}

		private bool ValidateName(string name)
		{
			bool result = SettingsHelper.ValidateKey(name, out var fixedName, true);

			Name = fixedName;

			return result;
		}

#endif

		#endregion Inspector
	}
}