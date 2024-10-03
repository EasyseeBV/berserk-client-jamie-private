using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.Models
{
	public class CmdParamsModel
	{
		/// <summary>
		/// Command targets to apply command execution on them
		/// </summary>
		public List<int> TargetObjectsIds { get; set; }
		/// <summary>
		/// IRuntimeGameObject that calls the command to be executed
		/// </summary>
		public int ExecutorObjectId { get; set; }
		/// <summary>
		/// Any json representing command meta data model that is used by specific generic command
		/// </summary>
		public string MetaJson { get; }
		public string CommandId { get; }
		public int TimeHash { get; }

		[JsonConstructor]
		public CmdParamsModel(
			List<int> targetObjectsIds, 
			int executorObjectId, 
			string metaJson, 
			string commandId, 
			int timeHash)
		{
			TargetObjectsIds = targetObjectsIds;
			ExecutorObjectId = executorObjectId;
			MetaJson = metaJson;
			CommandId = commandId;
			TimeHash = timeHash;
		}

		public CmdParamsModel(int timeHash, object meta = null, string commandId = null)
		{
			TimeHash = timeHash;
			CommandId = commandId ?? Guid.NewGuid().ToString();
			TargetObjectsIds = new List<int>();
			
			if (meta != null)
			{
				MetaJson = meta switch
				{
					string str => str,
					_ => JsonConvert.SerializeObject(meta)
				};
			}
		}
	}
}