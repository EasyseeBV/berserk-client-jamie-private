using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Controllers;
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
			var halfWidth = width * .5f;
			var yOffset = 100;
			var leftShift = -objectsCount * halfWidth + halfWidth;
			var ownerFactor = GetOwnerFactor();
			var positionY = (yOffset + GetVerticalOffset()) * ownerFactor;
			var rotation = Quaternion.identity;
			for (var i = 0; i < objectsCount; ++i)
			{
				yield return new TargetTransform(new Vector2(leftShift + i * width, positionY), rotation);
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
			return owner == Owner.Self ? 0 : 5f;
		}

		private int GetOwnerFactor()
		{
			return owner == Owner.Self ? -1 : 1;
		}
	}
}