using System;

namespace RR.Core.ResourceManagament
{
	public class ResourceServiceAdapter : IDisposable
	{
		public static IResourceService Instance => service ?? SERVICE_MOCK;
		
		private static IResourceService service;
		
		private static readonly IResourceService SERVICE_MOCK = new ResourceServiceMock();

		public ResourceServiceAdapter(IResourceService service)
		{
			Initialize(service);
		}
		
		public static void Initialize(IResourceService instance)
		{
			service = instance ?? SERVICE_MOCK;
		}

		public void Dispose()
		{
			service = null;
		}
	}
}