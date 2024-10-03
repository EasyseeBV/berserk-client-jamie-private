using System;
using System.Runtime.CompilerServices;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.RuntimeObjects;
using BerserkV3.GameCore.UI;
using RR.Core.DebugSystem;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public class CardViewMock : ICardView
	{
		private readonly IRuntimeCardData mockRuntimeData;
		private readonly IRuntimeGameCard mockRuntimeObject;
		
		IRuntimeGameObject IRuntimeObjectView.RuntimeGameObject => RuntimeGameObject;

		IRuntimeData IRuntimeObjectView.RuntimeData => RuntimeData;

		public RectTransform SelfContainer
		{
			get
			{
				NotifyUsedMock();
				return default;
			}
		}

		public TargetTransform TargetTransform
		{
			get
			{
				NotifyUsedMock();
				return default;
			}
			set => NotifyUsedMock();
		}

		public IRuntimeGameCard RuntimeGameObject
		{
			get
			{
				NotifyUsedMock();
				return mockRuntimeObject;
			}
		}
		
		public IRuntimeCardData RuntimeData
		{
			get
			{
				NotifyUsedMock();
				return mockRuntimeData;
			}
		}

		public ICardStrategy Strategy
		{
			get
			{
				NotifyUsedMock();
				return default;
			}
		}

		public IRuntimeLayout Layout
		{
			get
			{
				NotifyUsedMock();
				return default;
			}
		}

		private IGlowView glowViewMock;
		public IGlowView GlowView
		{
			get
			{
				NotifyUsedMock();
				return glowViewMock ??= new GlowViewMock();
			}
		}

		public bool IsSelf
		{
			get
			{
				NotifyUsedMock();
				return false;
			}
		}

		public bool IsLocked		
		{
			get
			{
				NotifyUsedMock();
				return false;
			}
		}

		public bool MarkedAsSelected
		{
			get
			{
				NotifyUsedMock();
				return false;
			}
		}

		~CardViewMock()
		{
			mockRuntimeObject?.Dispose();
		}
		
		public void Enable()
		{

		}

		public void Disable()
		{
			
		}
		
		public void MarkAsSelected(bool value = true)
		{
			NotifyUsedMock();
		}

		public void SetSize(float value)
		{
			NotifyUsedMock();
		}
		
		public void SetLock(bool value)
		{
			NotifyUsedMock();
		}

		public void SetLocalState(RuntimeState value)
		{
			NotifyUsedMock();
		}

		public void Refresh()
		{
			NotifyUsedMock();
		}

		public CardViewMock()
		{
			var mockData = new CardData();
			var runtimeCard = new RuntimeGameCard();
			mockRuntimeData = new RuntimeCardData(mockData);
			mockRuntimeObject = runtimeCard;
			runtimeCard.Init(mockRuntimeData, mockData);
		}

		public void Setup(IRuntimeGameObject runtimeGameObject)
		{
			SetLocalState(RuntimeData.State);
		}

		private void NotifyUsedMock([CallerMemberName] string callername = "", [CallerLineNumber] int callerLine = 0)
		{
			RRLogger.Log($"You trying to use {nameof(CardViewMock)}, [From {callername}, line {callerLine}]");
		}
	}
}