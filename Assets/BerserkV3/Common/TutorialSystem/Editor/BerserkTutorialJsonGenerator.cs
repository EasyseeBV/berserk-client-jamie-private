using System.IO;
using System.Linq;
using Berserk.Shared.Data.Tutor;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Editor;
using UnityEngine;

namespace BerserkV3.Common.TutorialSystem.Editor
{

	[CreateAssetMenu(fileName = "BerserkJsonGenerator", menuName = "Tutorial/BerserkJsonGenerator", order = 1)]
	public class BerserkTutorialJsonGenerator : TutorialJsonGenerator
	{
		protected override void SerializeToJson()
		{
			var items = Resources.LoadAll<TutorialStepEditor>(grabPath);
			var tutorData = items
				.OrderBy(x => x.RefreshHint().GetJsonOrder())
				.Select(item => new TutorHintModel
					{Id = item.Id, Data = JsonConvert.SerializeObject(item.Hint, Formatting.None)})
				.ToArray();

			if (tutorData.Length == 0)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Not found any tutor item");
				return;
			}

			if (!Directory.Exists(savePath))
				Directory.CreateDirectory(savePath);

			var json = JsonConvert.SerializeObject(new {TutorHints = tutorData}, formatting);
			File.WriteAllText(Path.Combine(savePath, fileName), json);
			WriteConvenientTableData(items.Select(x => x.Hint).ToArray());
		}
	}

}