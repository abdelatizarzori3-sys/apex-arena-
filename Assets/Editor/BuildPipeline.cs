using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildPipeline
{
    private const string ScenePath = "Assets/_Project/Scenes/MainArena.unity";

    public static void BuildAndroid()
    {
        Build(false);
    }

    public static void BuildAndroidBundle()
    {
        Build(true);
    }

    private static void Build(bool appBundle)
    {
        Directory.CreateDirectory("build/Android");
        EditorUserBuildSettings.buildAppBundle = appBundle;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality32Bit;

        var extension = appBundle ? "aab" : "apk";
        var output = Path.Combine("build/Android", $"Apex-Arena-{(appBundle ? "release" : "debug")}.{extension}");
        var options = BuildOptions.None;
        if (!appBundle)
        {
            options |= BuildOptions.Development;
        }

        var report = UnityEditor.BuildPipeline.BuildPlayer(
            new[] { ScenePath },
            output,
            BuildTarget.Android,
            options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Unity Android build failed: {report.summary.result}\n{report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings");
        }

        Debug.Log($"Apex Arena Android build created: {output} ({report.summary.totalSize} bytes)");
    }
}
