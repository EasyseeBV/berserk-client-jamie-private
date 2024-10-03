using System;
using System.Collections.Generic;
using System.Linq;

namespace Statistics
{
	public class StatisticParam : IStatisticParam
	{
		private Dictionary<string, string> values;

		public StatisticParam(string id, string value)
		{
			values = new Dictionary<string, string>();
			Add(id, value);
		}

		public StatisticParam(params ValueTuple<string, string>[] values)
		{
			if (values == null)
				throw new NullReferenceException("Values is missing");

			if (values.Length == 0)
				throw new InvalidOperationException("There should be no difference in number between " +
				                                    "the id and value parameters");

			this.values = new Dictionary<string, string>();
			foreach (var (id, value) in values)
			{
				Add(id, value);
			}
		}

		public override string ToString()
		{
			if (values == null)
				throw new InvalidOperationException("Values not initized");

			return values.Aggregate(GetType().Name,
			                        (current, param) => current + $"Param : [{param.Key}, {param.Value}], ");
		}

		public string Get(string id)
		{
			if (string.IsNullOrEmpty(id))
				throw new NullReferenceException("Id is missing or empty");

			if (values == null)
				throw new InvalidOperationException("Values not initized");

			if (!values.ContainsKey(id))
				throw new InvalidOperationException($"Value with id {id}, does not represent in collection");

			return values[id];
		}

		private void Add(string id, string value)
		{
			if (string.IsNullOrEmpty(id))
				throw new NullReferenceException("Id is missing or empty");

			if (string.IsNullOrEmpty(value))
				throw new NullReferenceException($"Value for id :{id} is missing or empty");

			values ??= new Dictionary<string, string>();
			
			if(!values.ContainsKey(id))
				values.Add(id, value);
		}
	}
}