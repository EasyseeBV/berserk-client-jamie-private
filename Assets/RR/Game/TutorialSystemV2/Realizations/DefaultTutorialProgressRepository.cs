using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Utilities;
using RR.Game.TutorialSystemV2.Abstraction;

namespace RR.Game.TutorialSystemV2.Realizations
{
	public class DefaultTutorialProgressRepository : ITutorialProgressRepository
	{
		protected IDictionary<string, bool> Progress = new Dictionary<string, bool>();
		protected virtual string DefaultFileName => "TutorialData.json";

		public virtual async Task InitAsync(IEnumerable<string> ids)
		{
			try
			{
				if (RRFile.Exists(DefaultFileName))
				{
					var jObject = RRFile.LoadJson(DefaultFileName);
					Progress = JsonConvert.DeserializeObject<Dictionary<string, bool>>(jObject.ToString());
				}
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
			finally
			{
				await FillAsync(ids);
			}
		}

		public virtual Task SaveAsync()
		{
			try
			{
				RRFile.Save(DefaultFileName, JsonConvert.SerializeObject(Progress));
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
			
			return Task.CompletedTask;
		}

		public virtual void SetCompleted(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				RRLogger.Error($"[{GetType().Name.Orange()}.{nameof(SetCompleted).Orange()}] Has empty id.");
				return;
			}

			Progress[id] = true;
		}

		public virtual void SetUnCompleted(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				RRLogger.Error($"[{GetType().Name.Orange()}.{nameof(SetUnCompleted).Orange()}] Has empty id.");
				return;
			}

			Progress[id] = false;
		}

		public virtual bool IsCompleted(string id)
		{
			return string.IsNullOrEmpty(id)
			       || !Progress.TryGetValue(id, out var isCompleted) 
			       || isCompleted;
		}

		public virtual bool IsCompleted()
		{
			return Progress.Count > 0 && Progress.Values.All(completed => completed);
		}

		public virtual void CompleteAll()
		{
			Progress.Keys.ToArray().ForEach(SetCompleted);
		}

		public virtual void ResetAll()
		{
			if (RRFile.Exists(DefaultFileName))
				RRFile.Delete(DefaultFileName);
			
			Progress.Clear();
		}

		protected virtual Task FillAsync(IEnumerable<string> ids)
		{
			ids?.ForEach(id =>
			{
				if (string.IsNullOrEmpty(id) || Progress.TryGetValue(id, out _))
					return;

				Progress[id] = false;
			});
			return Task.CompletedTask;
		}
	}
}