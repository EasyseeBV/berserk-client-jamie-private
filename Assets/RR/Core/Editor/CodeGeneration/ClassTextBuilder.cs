using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RR.Core.Editor.CodeGeneration
{
	public class ClassTextBuilder
	{
		private string name;
		private string accessLevel;
		private string modifier = string.Empty;
		private string type;
		private string preclassContent = string.Empty;
		private string innerContent = string.Empty;
		private string namespaceName = string.Empty;
		private string baseTypeName = string.Empty;
		private string indent = string.Empty;
		private string summary = string.Empty;
		private List<string> usingNamespaces = new List<string>();

		private bool hasNamespace => !string.IsNullOrWhiteSpace(namespaceName);

		private StringBuilder sb = new StringBuilder();

		public ClassTextBuilder(string name, AccessLevel accessLevel = AccessLevel.@public, ContentType type = ContentType.@class)
		{
			this.name = name;
			this.accessLevel = accessLevel.ToString();
			this.type = type.ToString();
		}

		public ClassTextBuilder AddSummary(string summaryContent)
		{
			var summarySb = new StringBuilder();

			summarySb.AppendLine($"\t/// <summary>");

			foreach (var s in summaryContent.Split('\n', '\r'))
			{
				summarySb.AppendLine($"\t/// {s}");
			}

			summarySb.Append($"\t/// </summary>");

			summary = summarySb.ToString();

			return this;
		}

		public ClassTextBuilder AddUsingNamespace(string namespaceName)
		{
			if (usingNamespaces.Contains(namespaceName) || namespaceName == string.Empty)
				return this;

			usingNamespaces.Add(namespaceName);

			return this;
		}

		public ClassTextBuilder SetPreclassContent(string preclassContent)
		{
			this.preclassContent += preclassContent;

			return this;
		}

		public ClassTextBuilder SetNamespace(string namespaceName)
		{
			this.namespaceName = namespaceName;
			return this;
		}

		public ClassTextBuilder SetBaseType(Type baseType)
		{
			if (baseType == null)
				throw new NullReferenceException("BaseType can't be null!");

			this.baseTypeName = baseType.Name;

			AddUsingNamespace(baseType.Namespace);

			return this;
		}

		public ClassTextBuilder SetBaseTypeName(string baseTypeName)
		{
			if (string.IsNullOrEmpty(baseTypeName))
				throw new NullReferenceException("BaseTypeName can't be null or empty!");

			this.baseTypeName = baseTypeName;

			return this;
		}

		public ClassTextBuilder SetAsStatic()
		{
			this.modifier = "static";

			return this;
		}

		public ClassTextBuilder InsertContent(string content)
		{
			innerContent += content;

			return this;
		}

		public ClassTextBuilder SetIndentLevel(int tabLevel)
		{
			for (var i = 0; i < tabLevel; i++)
				indent += "\t";

			return this;
		}

		public string Build()
		{
			foreach (var usingNamespace in usingNamespaces)
				sb.AppendLine($"using {usingNamespace};");

			if (usingNamespaces.Count > 0)
				sb.AppendLine();

			if (hasNamespace)
			{
				sb.AppendLine($"namespace {namespaceName}");
				sb.AppendLine("{");
			}

			sb.AppendLine(summary);

			if (preclassContent != string.Empty)
				sb.AppendLine(preclassContent);

			if (hasNamespace)
				indent += "\t";

			var baseClassAppend = baseTypeName == string.Empty ? string.Empty : $" : {baseTypeName}";
			var modifierAppend = modifier == string.Empty ? string.Empty : $"{modifier} ";

			sb.AppendLine($"{indent}{accessLevel} {modifierAppend}{type} {name}{baseClassAppend}");
			sb.AppendLine(indent + "{");
			AppendInnerContentWithIndent(sb, innerContent, indent);
			sb.AppendLine(indent + "}");

			if (hasNamespace)
				sb.Append("}");

			return sb.ToString();
		}
		public ClassTextBuilder Clear()
		{
			preclassContent = string.Empty;
			innerContent = string.Empty;
			namespaceName = string.Empty;
			baseTypeName = string.Empty;
			usingNamespaces.Clear();

			sb.Clear();

			return this;
		}

		private void AppendInnerContentWithIndent(StringBuilder sBuilder, string innerContent, string indent)
		{
			string[] lines = innerContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			foreach (var line in lines)
				sBuilder.AppendLine(indent + line);
		}
	}
}