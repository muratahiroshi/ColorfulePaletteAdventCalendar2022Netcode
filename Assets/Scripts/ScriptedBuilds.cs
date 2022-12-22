using UnityEngine;

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;

public class ScriptedBuilds
{
    // Invoked via command line only
    static void PerformServerLinuxBuild()
    {
        PerformServerBuild(BuildTarget.StandaloneLinux64, BuildOptions.StrictMode, StandaloneBuildSubtarget.Server);
    }

    // Invoked via command line only
    static void PerformServerOSXBuild()
    {
        PerformServerBuild(BuildTarget.StandaloneOSX, BuildOptions.StrictMode, StandaloneBuildSubtarget.Server);
    }

    static void PerformPlayerLinuxBuild()
    {
        PerformServerBuild(BuildTarget.StandaloneLinux64, BuildOptions.StrictMode, StandaloneBuildSubtarget.Player);
    }

    static void PerformPlayerOSXBuild()
    {
        PerformServerBuild(BuildTarget.StandaloneOSX, BuildOptions.StrictMode, StandaloneBuildSubtarget.Player);
    }

    static void PerformServerBuild(BuildTarget buildTarget, BuildOptions buildOptions,
        StandaloneBuildSubtarget subtarget)
    {
        var buildPath = Path.Combine(Application.dataPath, "BUILD");

        var args = Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildPath")
            {
                buildPath = args[i + 1];
            }
        }

        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        BuildPipeline.BuildPlayer(new BuildPlayerOptions()
        {
            scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
            locationPathName = buildPath,
            target = buildTarget,
            subtarget = (int)subtarget,
            options = buildOptions
        });
    }
}
#endif