using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Editor.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Editor
{
    [CreateAssetMenu(fileName = "JsonGenerator", menuName = "Tutorial/JsonGenerator", order = 0)]
    public class TutorialJsonGenerator : ScriptableObject
    {
        [Tooltip("Text data export formatting style.")]
        [SerializeField] protected Formatting formatting = Formatting.Indented;
        [Tooltip("Path to save text data.")]
        [SerializeField] protected string savePath = "Assets/Resources";
        [Tooltip("The name and extension of the text file.")]
        [SerializeField] protected string fileName = "TutorData.json";
        [Tooltip("Path to the resource folder, any resource folder by default.")]
        [SerializeField] protected string grabPath = "";
        [Tooltip("Get a list of all id, title, texts in a convenient table format?")]
        [SerializeField] protected bool writeConvenientTableData;
		
        [ContextMenu("Grab and SerializeToJson")]
        [Button("Grab and SerializeToJson")]
        protected virtual void SerializeToJson()
        {
            var items = Resources.LoadAll<TutorialStepEditor>(grabPath);
            var tutorData = new 
            {
                TutorHints = items
                    .OrderBy(x=> x.RefreshHint().GetJsonOrder())
                    .Select(item => item.Hint)
                    .ToArray()
            };

            if (tutorData.TutorHints.Length == 0)
            {
                RRLogger.Error($"[{GetType().Name.Orange()}] Not found any tutor item");
                return;
            }
            
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            
            var json = JsonConvert.SerializeObject(tutorData, formatting);
            File.WriteAllText(Path.Combine(savePath, fileName), json);
            WriteConvenientTableData(tutorData.TutorHints);
        }

        protected virtual void WriteConvenientTableData(params TutorialBlockEditor[] tutorHints)
        {
            if (!writeConvenientTableData || tutorHints == null || tutorHints.Length == 0) 
                return;
            
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            
            var ids = string.Join("\n", tutorHints.Select(x => x.Id));
            // after pasting into Excel, you can replace the BLANK - word with an empty field.
            var titles = string.Join("\n", tutorHints.Select(x => string.IsNullOrEmpty(x.Popup.Title) ? "BLANK" : x.Popup.Title.Replace("\n", " ")));
            var texts = string.Join("\n", tutorHints.Select(x => string.IsNullOrEmpty(x.Popup.Text) ? "BLANK" : x.Popup.Text.Replace("\n", " ")));
            var table = string.Join("\n\n", ids, titles, texts);
            File.WriteAllText(Path.Combine(savePath, "TutorTable.json"), table);
        }
    }
}