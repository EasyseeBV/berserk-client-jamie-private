using System;
using UnityEditor;
using UnityEngine;

namespace RR.UI.Editor
{
	public class NameWizard : ScriptableWizard
	{
		string Name;
		Action<string> OnCreated;

		public static void CreateWizard(string name, Action<string> onCreated)
		{
			var wizard = DisplayWizard<NameWizard>("Enter name", "OK");
			wizard.Name = name;
			wizard.OnCreated = onCreated;
			wizard.maxSize = 
			wizard.minSize = new Vector2(260, 100);
			wizard.Focus();
		}

		void OnWizardCreate()
		{
			OnCreated?.Invoke(Name);
		}

		void OnWizardUpdate()
		{
			helpString = "Name";
		}

		protected override bool DrawWizardGUI()
		{
			Name = GUILayout.TextField(Name);
			return false;
		}
	}
}