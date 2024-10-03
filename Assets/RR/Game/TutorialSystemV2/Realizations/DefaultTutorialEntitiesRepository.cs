using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Realizations
{
    public class DefaultTutorialEntitiesRepository : ITutorialEntitiesRepository, IDisposable
    {
        protected readonly IList<ITutorialHintEntity> Entities;
        protected virtual string FileName => "TutorData";
        protected virtual string FilePath => "";
        protected bool Initialized;
        public DefaultTutorialEntitiesRepository()
        {
            Entities = new List<ITutorialHintEntity>();
        }

        public virtual async Task InitAsync()
        {
	        if (Initialized)
		        return;
	        
	        try
            {
	            var tcs = new TaskCompletionSource<string>();
	            var request = Resources.LoadAsync<TextAsset>(Path.Combine(FilePath, FileName));
	            request.completed += _ => tcs.TrySetResult(request.asset is TextAsset asset ? asset.text : string.Empty);
	            var json = await tcs.Task;
	            if (string.IsNullOrEmpty(json))
	            {
		            RRLogger.Error("Tutor data json is missing!");
		            return;
	            }
	            
	            var data = JsonConvert.DeserializeObject<List<TutorialBlock>>(json);
	            data!.ForEach(block => Entities.Add(new DefaultTutorialHintEntity(block)));
	            Initialized = true;
            }
            catch (Exception e)
            {
	            RRLogger.Error($"Tutor data cannot be deserialized!, Original exception: {e}");
	            Initialized = false;
            }
        }

        public virtual void Dispose()
        {
	        Entities.Clear();
	        Initialized = false;
        }

        public virtual void ResetAll()
        {
            Entities.Clear();
            Initialized = false;
        }

        public virtual ITutorialHintEntity Get(string id)
        {
            return Entities.FirstOrDefault(x => x.Id == id);
        }

        public virtual IEnumerable<ITutorialHintEntity> GetAll()
        {
            return Entities.ToArray();
        }
    }
}