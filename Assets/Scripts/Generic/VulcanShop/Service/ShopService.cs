using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BerserkV3.Common.DataBase;
using BerserkV3.Lobby.Network;
using Sirenix.Utilities;
using Vulcan.Data;
using Vulcan.Shop.Domain;

namespace Vulcan.Shop.Service
{
	public interface IShopService
	{
		IEnumerable<VulcaniteEntity> Vulcanites { get; }
		LavaBalance LavaBalance { get; }
		bool RequiredRefresh { get; }

		Task RefreshPlayerBalance();
		Task RefreshShopVulcanite();
		Task TryByuAsync(VulcaniteEntity vulcaniteEntity);
	}
	public class ShopService : IShopService
	{
		private List<VulcaniteEntity> vulcanites = new List<VulcaniteEntity>();
		public IEnumerable<VulcaniteEntity> Vulcanites => vulcanites;
		public LavaBalance LavaBalance { get; private set; }

		public bool RequiredRefresh => !vulcanites.Any() || LavaBalance == null;

		public async Task RefreshShopVulcanite()
		{
			var response = await ShopAPI.GetShopVulcanites();

			if (!response.IsSuccess || response.Data.IsNullOrEmpty())
				return;
			
			vulcanites = response.Data.Select(shopVulcanite =>
			{
				var gameDataBase = GameDataBaseAdapter.Instance;
				var vulcaniteData = gameDataBase.GetHero(shopVulcanite.VulcaniteId);
				var effectsInfo = gameDataBase
					.GetEffects(vulcaniteData.EffectsIds)
					.Select(effect => new EffectInfo(effect.ToString(), gameDataBase.GetKeyword(effect.KeywordId).GetEffectDescription(effect), effect.Value))
					.ToList();
				
				return new VulcaniteEntity(vulcaniteData.Id,
					vulcaniteData.LevelAtSync,
					vulcaniteData.Quadrant,
					vulcaniteData.Name,
					vulcaniteData.ArtUrl,
					shopVulcanite.IsOwned,
					shopVulcanite.Price,
					effectsInfo);
			}).ToList();
		}

		public async Task TryByuAsync(VulcaniteEntity vulcaniteEntity)
		{
			if (LavaBalance.Value < vulcaniteEntity.Price)
				return;

			var response = await ShopAPI.BuyVulcanite(vulcaniteEntity.Id);
			if (!response.IsSuccess)
				return;

			vulcaniteEntity.Purchase();
			await RefreshPlayerBalance();
		}

		public async Task RefreshPlayerBalance()
		{
			var userStatsModel = (await ShopAPI.GetPlayerBalance()).Data;
			LavaBalance = new LavaBalance(userStatsModel.LavaBalance);
		}
	}

	
}