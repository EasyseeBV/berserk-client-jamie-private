using System;
using System.Collections.Generic;
using System.Text;

namespace RR.Core.Editor.CodeGeneration
{
    public class MethodTextBuilder
    {
        private string methodName;
        private string accessLevelName;
        
        private string modifierName = "";
        private string returnTypeName = "";
        private string methodContent = "";
        private string indent = "";
        
        private List<string> parameters = new List<string>();
        
        private StringBuilder sb = new StringBuilder();
        
        public MethodTextBuilder(string methodName, AccessLevel accessLevel = AccessLevel.@public)
        {
            this.methodName = methodName;
            this.accessLevelName = accessLevel.ToString();
        }

        public MethodTextBuilder SetReturnType(Type returnType)
        {
            returnTypeName = returnType.ToString();

            return this;
        }

        public MethodTextBuilder SetModifier(Modifier modifier)
        {
            modifierName = modifier.ToString();

            return this;
        }
        
        public MethodTextBuilder AddParameter(string parameterName, Type parameterType)
        {
            parameters.Add($"{parameterType.Name} {parameterName}");

            return this;
        }
        
        public MethodTextBuilder AddParameter(string parameterName, string parameterTypeName)
        {
            parameters.Add($"{parameterTypeName} {parameterName}");

            return this;
        }

        public MethodTextBuilder AddContent(string content)
        {
            methodContent = content;

            return this;
        }

        public MethodTextBuilder SetIndentLevel(int tabLevel)
        {
            for (int i = 0; i < tabLevel; i++)
                indent += "\t";

            return this;
        }

        public string Build()
        {
            returnTypeName = returnTypeName == "" ? "void" : returnTypeName;
            
            string methodParamsAppend = "";
            for (int i = 0; i < parameters.Count; i++)
                methodParamsAppend += parameters[i] + ((i == parameters.Count - 1)? "" : ", ");

            sb.AppendLine($"{indent}{accessLevelName} {modifierName} {returnTypeName} {methodName}({methodParamsAppend})");
            sb.AppendLine(indent + "{");
            sb.AppendLine(indent + "\t");
            sb.Append(indent + "}");

            return sb.ToString();
        }

        public MethodTextBuilder Clear()
        {
            returnTypeName = "";
            modifierName = "";
            methodContent = "";
            indent = "";

            sb.Clear();

            return this;
        }
    }
}