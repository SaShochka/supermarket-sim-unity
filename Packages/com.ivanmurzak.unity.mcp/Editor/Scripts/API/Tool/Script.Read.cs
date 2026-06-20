#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.IO;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Script
    {
        [McpPluginTool
        (
            "Script_Read",
            Title = "Read Script content"
        )]
        [Description("Reads the content of a script file and returns it as a string.")]
        public string Read
        (
            [Description("The path to the file. Sample: \"Assets/Scripts/MyScript.cs\".")]
            string filePath
        )
        {
            if (string.IsNullOrEmpty(filePath))
                return Error.ScriptPathIsEmpty();

            if (!filePath.EndsWith(".cs"))
                return Error.FilePathMustEndsWithCs();

            if (File.Exists(filePath) == false)
                return Error.ScriptFileNotFound(filePath);

            var csharpCode = File.ReadAllText(filePath);
            return csharpCode;
        }
    }
}