using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.EventLayer;
using RR.Core.EventLayer.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.Core
{

#if RR_ASSETBUS

	[ExecuteInEditMode]
	public class AssetProcessorBus : EventBus
	{
		[ExecuteInEditMode, HideInLog]
		public static RREvent<List<MonoBehaviour>> AddedAssets = new RREvent<List<MonoBehaviour>>();

		[ExecuteInEditMode, HideInLog] public static RREvent<Type> RequestAssetsFromDb = new RREvent<Type>();
		[ExecuteInEditMode, HideInLog] public static State<IEnumerable<Object>> ReceiveAssetsFromDb = new State<IEnumerable<Object>>();

		public static void RequestAssets<T>(Action<IEnumerable<T>> doOnResult)
		{
			void Callback(IEnumerable<Object> x) => doOnResult(x.OfType<T>());

			ReceiveAssetsFromDb.SubscribeRaw(Callback);
			RequestAssetsFromDb += typeof(T);
			ReceiveAssetsFromDb.Unsubscribe(Callback);
		}

		static AssetProcessorBus()
		{
			InitFields<AssetProcessorBus>();
			AddedAssets.HideInLog = true;
		}
	}

#endif
}
