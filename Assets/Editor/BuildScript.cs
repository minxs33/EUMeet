using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Headless Linux Server")]
    [System.Obsolete]
    public static void BuildHeadless()
    {
        string[] scenes = { 
            "Assets/Scenes/Authenticate.unity",
            "Assets/Scenes/Lobby.unity" 
            };
        string buildPath = "Builds/Headless/Server.x86_64";
        
        // Ensure the build directory exists
        Directory.CreateDirectory("Builds/Headless");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.EnableHeadlessMode
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
