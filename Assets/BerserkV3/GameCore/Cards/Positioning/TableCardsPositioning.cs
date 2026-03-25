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
		private float width = 10;
		private float height = 15;
		private Owner owner = Owner.None;

		public IEnumerable<TargetTransform> CalculatePositions(int objectsCount)
		{
			var effectiveWidth = width * GetHorizontalSpacingFactor(objectsCount);
			var halfWidth = effectiveWidth * .5f;
			var yOffset = BoardLayoutSettings.IsMinimal() ? 112f : 100f;
			var leftShift = -objectsCount * halfWidth + halfWidth;
			var ownerFactor = GetOwnerFactor();
			var positionY = (yOffset + GetVerticalOffset()) * ownerFactor;
			var rotation = Quaternion.identity;
			for (var i = 0; i < objectsCount; ++i)
			{
				yield return new TargetTransform(new Vector2(leftShift + i * effectiveWidth, positionY), rotation);
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
			if (BoardLayoutSettings.IsMinimal())
				return owner == Owner.Self ? -8f : 12f;

			return owner == Owner.Self ? 0 : 5f;
		}

		private int GetOwnerFactor()
		{
			return owner == Owner.Self ? -1 : 1;
		}

		private static float GetHorizontalSpacingFactor(int objectsCount)
		{
			if (!BoardLayoutSettings.IsMinimal())
				return 1f;

			if (objectsCount >= 7)
				return 1.22f;

			if (objectsCount >= 5)
				return 1.16f;

			return 1.1f;
		}
	}
}
