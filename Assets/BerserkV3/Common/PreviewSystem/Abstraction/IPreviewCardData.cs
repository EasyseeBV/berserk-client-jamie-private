using Berserk.Shared.Data.Abstraction;

namespace BerserkV3.Common.PreviewSystem
{

	public interface IPreviewCardData : IPreviewData, ICardData
	{
		public bool IsToken { get; }
	}

}