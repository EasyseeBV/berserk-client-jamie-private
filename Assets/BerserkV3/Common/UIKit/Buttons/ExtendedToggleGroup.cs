using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public interface ISwitcherView
	{
		IEnumerable<ExtendedToggle> Toggels { get; }
		int SelectedIndex { get; }
		event Action<int> OnToggleSelected;
		
		void Init();
		void Release();
		void DisableAll(bool notify = true);
		void SwitchWithoutNotify(int index);
		void AddOrRefresh(List<string> toggleNames, int selectedIndex = 0);
		void AddOrRefresh(ExtendedToggle toggle, string toggleName, int? index = null);
		void SetInteractable(bool value);
	}
	
	public class ExtendedToggleGroup : ToggleGroup, ISwitcherView
	{
		[SerializeField, Tooltip("Enable an extra object in toggles.")] 
		private bool useExtraSelector;
		[SerializeField] private bool ensureStateAtStart = true;
		[SerializeField] private bool ensureStateAtVisible = true;
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField, Tooltip("It is automatically filled in when the group is initialised. It can be filled at runtime.")] 
		private List<ExtendedToggle> toggles;
		
		private bool initialized;
		public IEnumerable<ExtendedToggle> Toggels => toggles;
		public int SelectedIndex { get; private set; }
		
		public event Action<int> OnToggleSelected;

		protected override void Start(){} // handle init flow manually -> Init();

		protected override void OnEnable() // added flexibility, for example - when you want to control a group state outside group class.
		{
			if (ensureStateAtVisible)
				EnsureValidState();
		}
		
		public void Init() // it provide more control than method Start - when you use this class in DI environment.
		{
			if (initialized)
				return;

			initialized = true;
			foreach (var toggle in GetComponentsInChildren<ExtendedToggle>(true))
			{
				if(toggle.group != this)
					toggle.group = this;
				if (!toggles.Contains(toggle))
					toggles.Add(toggle);
				
				UpdateToggleState(toggle);
			}
			
			if (ensureStateAtStart)
				EnsureValidState();
		}

		public void Release()
		{
			OnToggleSelected = null;
			foreach (var berserkToggle in toggles.Where(component => component))
				berserkToggle.onValueChanged.RemoveAllListeners();
		}

		public void DisableAll(bool notify = true)
		{
			SelectedIndex = -1;
			var oldAllowSwitchOff = allowSwitchOff; // disable logic to disable all the toggles.
			allowSwitchOff = true;

			if (notify)
			{
				foreach (var toggle in toggles)
					toggle.isOn = false; // will notify and update the state.
			}
			else
			{
				foreach (var toggle in toggles)
				{
					toggle.SetIsOnWithoutNotify(false);
					UpdateToggleState(toggle); // when notify is disabled you should update the sate manually.
				}
			}

			allowSwitchOff = oldAllowSwitchOff;
		}

		public void SwitchWithoutNotify(int index)
		{
			if (index < 0 || index >= toggles.Count) // when index outside the bounds it disables all.
			{
				DisableAll(false);
				return;
			}
			
			SelectedIndex = index;
			for (var i = 0; i < toggles.Count; i++)
			{
				var toggle = toggles[i];
				toggle.SetIsOnWithoutNotify(SelectedIndex == i);
				UpdateToggleState(toggle); // when notify is disabled you should update the sate manually.
			}
		}
		
		public void AddOrRefresh(List<string> toggleNames, int selectedIndex = 0)
		{
			SelectedIndex = selectedIndex;
			UIHelper.InitWidgets(toggles, toggleNames.Count, (toggle, index) 
				=> AddOrRefresh(toggle, toggleNames[index], index));
		}

		public void AddOrRefresh(ExtendedToggle toggle, string toggleName, int? index = null)
		{
			index = Mathf.Clamp(index ?? toggles.Count, 0, toggles.Count);
			if (!toggles.Contains(toggle))
				toggles.Insert(index.Value, toggle);
			
			if (toggle.group != this)
				toggle.group = this;
			
			toggle.SetText(toggleName);
			toggle.SetIsOnWithoutNotify(index == SelectedIndex);
			toggle.onValueChanged.RemoveListener(ToggleChanged); // it will prevent multiple subscribe
			toggle.onValueChanged.AddListener(ToggleChanged);
			UpdateToggleState(toggle);
				
			return;
			void ToggleChanged(bool isOn)
			{
				UpdateToggleState(toggle);
					
				if (!isOn || SelectedIndex == index.Value) 
					return;
					
				SelectedIndex = index.Value;
				OnToggleSelected?.Invoke(index.Value);
			}
		}

		public void SetInteractable(bool value)
		{
			if (!canvasGroup && !TryGetComponent(out canvasGroup))
				canvasGroup = gameObject.AddComponent<CanvasGroup>();

			canvasGroup.alpha = value ? 1 : 0.5f;
			canvasGroup.interactable = value;
		}

		private void UpdateToggleState(ExtendedToggle toggle)
		{
			if (useExtraSelector && toggle)
				toggle.SetSelectorActive(toggle.isOn);
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			OnToggleSelected = null;
		}
	}
}