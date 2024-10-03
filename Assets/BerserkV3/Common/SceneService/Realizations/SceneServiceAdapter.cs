namespace BerserkV3.Common.SceneService
{
	public class SceneServiceAdapter
	{
		public static ISceneService Service { get; private set; }
		
		public SceneServiceAdapter(ISceneService service)
		{
			Service = service;
		}
	}
}