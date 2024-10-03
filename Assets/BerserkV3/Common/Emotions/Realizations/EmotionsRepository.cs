using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Generic.Emotions
{
	public class EmotionsRepository : IEmotionsRepository
	{
		private readonly List<Emotion> emotions = new();

		public Emotion Get(string id)
		{
			return emotions.FirstOrDefault(x => x.Id.Same(id));
		}

		public void Add(Emotion value)
		{
			if (Contains(value.Id))
			{
				RRLogger.Error("You can't add the same reactions");
				return;
			}

			emotions.Add(value);
		}

		public bool Contains(string id)
		{
			return emotions.Any(x => x.Id.Same(id));
		}
	}
}