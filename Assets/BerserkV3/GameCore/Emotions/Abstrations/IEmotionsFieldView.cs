using Berserk.Shared.Data.Enums;
using UnityEngine;

namespace BerserkV3.GameCore.Emotions
{
	public interface IEmotionsFieldView
	{
		int Count { get; }
		Owner FieldOwner { get; }
		RectTransform Container { get; }
		
		void Rearrange();
	}
}