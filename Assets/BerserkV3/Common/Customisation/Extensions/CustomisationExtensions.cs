using System;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.DataBase;
using Newtonsoft.Json;
using RR.Core.DebugSystem;

namespace BerserkV3.Generic.Customisation
{
	public static class CustomisationExtensions
	{
		public static T GetCustomisationAsset<T>(this CustomisationData data) where T : AssetData
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(data.AssetData);
			}
			catch (Exception e)
			{
				RRLogger.Error(e, $"Cannot create {typeof(T).Name} for id : '{data?.Id}'");
				return default;
			}
		}
		
		public static T GetCustomisationAsset<T>(this CustomisationItemDto itemDto) where T : AssetData
		{
			try
			{
				var itemData = GameDataBaseAdapter.Instance.GetCustomisationData(itemDto.Id);
				return JsonConvert.DeserializeObject<T>(itemData.AssetData);
			}
			catch (Exception e)
			{
				RRLogger.Error(e, $"Cannot create {typeof(T).Name} for id : '{itemDto?.Id}'");
				return default;
			}
		}
	}
}