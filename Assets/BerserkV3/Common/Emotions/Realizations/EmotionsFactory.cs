using System;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Generic.Customisation;
using Vulcan.Data;

namespace BerserkV3.Generic.Emotions
{
	public class EmotionsFactory : IEmotionsFactory
	{
		private readonly IEmotionsRepository emotionsRepository;
		private readonly IGameDatabase gameDatabase;

		public EmotionsFactory(IEmotionsRepository emotionsRepository, IGameDatabase gameDatabase)
		{
			this.emotionsRepository = emotionsRepository;
			this.gameDatabase = gameDatabase;
		}
		
		public Emotion Create(string id)
		{
			if (emotionsRepository.Contains(id))
				return emotionsRepository.Get(id);
			
			var data = gameDatabase.GetCustomisationData(id);
			var assetData = data.GetCustomisationAsset<EmotionAssetData>();
			if (assetData == null)
				throw new NullReferenceException();

			
			var reaction = new Emotion(id,
				assetData.URL,
				assetData.Height,
				assetData.Width,
				assetData.TimeScale,
				assetData.LifeTime);

			emotionsRepository.Add(reaction);
			return reaction;
		}
	}
}