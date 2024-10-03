using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Generic.Emotions
{
	public interface IEmotionView
	{
		string Id { get; }
		
		bool AutoDispose { get; set; }
		
		UniTask InitAsync(Emotion emotion, CancellationToken token = default);
	}
}