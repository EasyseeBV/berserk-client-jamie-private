using System;
using System.Collections.Generic;
using System.Linq;

namespace Statistics
{
	public class StatisticModel
	{
		public readonly string Id;
		public readonly List<IStatisticParam> Params;
		
		public StatisticModel(string id, params IStatisticParam[] args)
		{
			if (string.IsNullOrEmpty(id))
				throw new NullReferenceException("Id is missing or empty");

			if (args == null || args.Length == 0)
				throw new NullReferenceException("Params is missing");

			Id = id;
			Params = args.ToList();
		}
	}
}
