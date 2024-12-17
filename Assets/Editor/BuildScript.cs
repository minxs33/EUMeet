using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build Headless Linux Server")]
    [System.Obsolete]
    public static void BuildHeadless()
    {

        // Define preprocessor directive for headless build
        string[] defineSymbols = {
            "UNITY_SERVER",
            "FUSION_WEAVER",
            "FUSION2"
        };
        string defineSymbolsString = string.Join(";", defineSymbols);

        BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbolsString);

        string[] scenes = {
            "Assets/Scenes/Authenticate.unity",
            "Assets/Scenes/Lobby.unity"
        };
        string buildPath = "Builds/Headless/Server.x86_64";

        Directory.CreateDirectory("Builds/Headless");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.EnableHeadlessMode
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Reset define symbols after build
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "");
    }
}
