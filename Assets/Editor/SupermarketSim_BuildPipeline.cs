using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class SupermarketSim_BuildPipeline
{
    public static void BuildWindows()
    {
        string outputPath = GetArgument("-buildOutput");
        if (string.IsNullOrWhiteSpace(outputPath))
            outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SupermarketSimBuild", "SupermarketSim.exe");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Scenes/Menu.unity",
                "Assets/Scenes/Game.unity"
            },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Build failed: {report.summary.result}");
    }

    private static string GetArgument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }
}
