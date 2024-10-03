using Berserk.Shared.Data.Abstraction;
using UnityEngine;

namespace BerserkV3.Common.PreviewSystem
{
	public interface IPreviewView
	{
		void InitAndShowSide(IPreviewData data, float lifeTime);
		void InitAndShowCenter(IPreviewData data, float lifeTime);
		void InitAndShowCorner(IPreviewData data, float lifeTime);
		void ShowAndFitAroundTarget(GameObject target, IPreviewData data, float lifeTime);
		void ShowDeckValueInfo(GameObject target, IPreviewData data, float lifeTime);
		
		void Close(bool noAnimation = true);
	}
}