#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_Script
    {
        static IEnumerable<Type> AllComponentTypes => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(UnityEngine.Component).IsAssignableFrom(type) && !type.IsAbstract);

        public static class Error
        {
            static string ComponentsPrinted => string.Join("\n", AllComponentTypes.Select(type => type.FullName));

            public static string ScriptPathIsEmpty()
                => "[Error] Script path is empty. Please provide a valid path. Sample: \"Assets/Scripts/MyScript.cs\".";

            public static string ScriptFileNotFound(string filePath)
                => $"[Error] File not found: {filePath}. Please check the path and try again.";

            public static string FilePathMustEndsWithCs()
                => "[Error] File path must end with \".cs\". Please provide a valid C# file path.";
        }
    }
}