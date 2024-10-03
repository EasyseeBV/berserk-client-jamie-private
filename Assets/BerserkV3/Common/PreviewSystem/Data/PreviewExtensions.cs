using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.Common.PreviewSystem
{
	public static class PreviewExtensions
	{
		public static IPreviewData ToPreviewData(this ICardData data)
		{
			return new PreviewCardData(data);
		}
		
		public static IPreviewData ToPreviewData(this IPreviewable data)
		{
			return new DeckValueInfoData();
		}

		public static IPreviewCardData ToPreviewData(this IRuntimeCardData runtimeData, ICardData data)
		{
			return new PreviewCardData(runtimeData, data);
		}

		public static IPreviewCardData ToPreviewData(this IRuntimeGameCard runtimeCard)
		{
			return new PreviewCardData(runtimeCard?.RuntimeData, runtimeCard?.Data);
		}

		public static IPreviewCardData ToPreviewData(this IRuntimeGameObject runtimeObject)
		{
			if (runtimeObject is not IRuntimeGameCard gameCard)
				return null;
			
			return new PreviewCardData(gameCard.RuntimeData, gameCard.Data);
		}
	}
}