using System.Collections.Generic;
using System.Linq;
using RR.UIService.Data;

namespace RR.UIService
{
	public class UIServiceRepository : IUIServiceRepository
	{
		private readonly List<WindowItem> storage = new();
		
		public WindowItem Get<T>(string windowId, string groupId)
		{
			var checkId = !string.IsNullOrEmpty(windowId);
			var checkGroup = !string.IsNullOrEmpty(groupId);
			var requestType = typeof(T);
			if (requestType.IsInterface || requestType.IsAbstract)
				return storage.FirstOrDefault(x => x.Window is T && (!checkId || x.Id == windowId) && (!checkGroup || x.GroupId == groupId));
				
			return storage.FirstOrDefault(x => x.CachedType == requestType && (!checkId || x.Id == windowId) && (!checkGroup || x.GroupId == groupId));
		}

		public IEnumerable<WindowItem> GetAll<T>(string groupId)
		{
			var requestType = typeof(T);
			if (requestType.IsInterface || requestType.IsAbstract)
				return storage.Where(x => x.Window is T && (groupId == null || groupId == x.GroupId));
			
			return storage.Where(x => x.CachedType == requestType && (groupId == null || groupId == x.GroupId));
		}

		public void Add(WindowItem value) 
			=> storage.Add(value);

		public bool Remove(WindowItem value) 
			=> storage.Remove(value);
	}
}