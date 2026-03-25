using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Controllers;
using UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Cards
{
	public class TableCardsPositioning : ITableCardsPositioning
	{
		private float width = 20;
		private float height = 18;
		private Owner owner = Owner.None;

		public IEnumerable<TargetTransform> CalculatePositions(int objectsCount)
		{
			var yOffset = 170f;
			var spacing = GetHorizontalSpacing(objectsCount);
			var leftShift = -spacing * Mathf.Max(0, objectsCount - 1) * 0.5f;
			var ownerFactor = GetOwnerFactor();
			var positionY = (yOffset + GetVerticalOffset()) * ownerFactor;
			var rotation = Quaternion.identity;
			for (var i = 0; i < objectsCount; ++i)
			{
				yield return new TargetTransform(new Vector2(leftShift + i * spacing, positionY), rotation);
			}
		}

		public IEnumerable<TargetTransform> CalculatePositions(int objectsCount, float width, float height, Owner owner)
		{
			this.width = width;
			this.height = height;
			this.owner = owner;
			return CalculatePositions(objectsCount);
		}

		private float GetVerticalOffset()
		{
			return owner == Owner.Self ? 0 : 10f;
		}

		private int GetOwnerFactor()
		{
			return owner == Owner.Self ? -1 : 1;
		}

		private float GetHorizontalSpacing(int objectsCount)
		{
			if (objectsCount <= 1)
				return 0f;

			var cardWidth = Mathf.Max(1f, width);
			var minimal = BoardLayoutSettings.IsMinimal();
			var spacingMultiplier = minimal ? 1.35f : 1.08f;
			var desiredSpacing = cardWidth * spacingMultiplier;
			if (minimal)
			{
				// Ensure stat orbs (HP bubbles) don't overlap neighboring cards
				desiredSpacing = Mathf.Max(desiredSpacing, cardWidth + 60f);
			}

			// Cap total spread so cards stay centered, not edge-to-edge
			var maxSpread = minimal ? 1200f : 980f;
			return Mathf.Min(desiredSpacing, maxSpread / (objectsCount - 1));
		}
	}
}
