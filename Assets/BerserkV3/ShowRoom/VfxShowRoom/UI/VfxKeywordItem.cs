using System;
using Berserk.Shared.Data.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.ShowRoom.VfxShowRoom
{
	public class VfxKeywordItem : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private Button button;
		
		public EffectVisualKeyword Id { get; set; }
		
		public event Action<EffectVisualKeyword> OnClick;

		public void SetText(string value)
		{
			nameText.text = value;
		}

		private void Awake()
		{
			button.onClick.AddListener(() => OnClick?.Invoke(Id));
		}

		private void OnDestroy()
		{
			Id = EffectVisualKeyword.None;
			OnClick = null;
			button.onClick.RemoveAllListeners();
		}
	}
}