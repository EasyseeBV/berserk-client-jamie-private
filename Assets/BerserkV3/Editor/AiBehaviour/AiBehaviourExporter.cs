using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using BerserkV3.GameCore.AiBehaviour.Nodes.BaseNodes;
using BerserkV3.GameCore.AiBehaviour.Scriptables;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class AiBehaviourExporter : EditorWindow
{
	private BehaviourGraph[] graphs;
	private string output;
	private Vector2 scrollPosition;
	private int behavioursCount;

	[MenuItem("Tools/AI behaviour exporter")]
	private static void Open()
	{
		var window = (AiBehaviourExporter)GetWindow(typeof(AiBehaviourExporter));
		window.Show();
	}

	private void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			behavioursCount = EditorGUILayout.IntField(behavioursCount);

			if (graphs == null || graphs.Length != behavioursCount)
				graphs = new BehaviourGraph[behavioursCount];

			for (var i = 0; i < behavioursCount; i++)
				graphs[i] =
					EditorGUILayout.ObjectField("AI Behaviour Graph", graphs[i], typeof(BehaviourGraph), true) as
						BehaviourGraph;

			if (GUILayout.Button("Export to Json")) output = ConvertToJson(graphs);

			if (!string.IsNullOrEmpty(output))
			{
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
				{
					EditorGUILayout.SelectableLabel(output, EditorStyles.textArea, GUILayout.ExpandHeight(true));
				}
				EditorGUILayout.EndScrollView();
			}
		}
		EditorGUILayout.EndVertical();
	}

	private string ConvertToJson(BehaviourGraph[] graphs)
	{
		var aiBehavoursData = graphs
			.Select(g => new AiBehaviourData(g.name, Convert(g.EnterNode)))
			.ToArray();

		var json = JsonConvert.SerializeObject(aiBehavoursData, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			NullValueHandling = NullValueHandling.Ignore
		});

		return json;
	}

	private AiNode Convert(BaseBehaviourNode node)
	{
		var aiNode = new AiNode
		{
			NodeType = node.NodeType,
			Model = node.Model
		};

		if (node is BaseBehaviourOutputNode outputNode)
		{
			var outputNodes = outputNode.GetOutputNodes();
			if (outputNodes != null)
			{
				aiNode.Childs = new List<AiNode>();
				foreach (var childNode in outputNodes) aiNode.Childs.Add(Convert(childNode));
			}
		}

		return aiNode;
	}
}