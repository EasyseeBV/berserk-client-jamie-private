using UnityEngine;

namespace BerserkV3.Generic.Emotions
{
	public interface IEmotionsViewFactory
	{
		IEmotionView Create(string name, Transform parent = null);
	}
}