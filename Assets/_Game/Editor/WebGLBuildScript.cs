using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MythologyTowerDefense.EditorTools
{
    public static class WebGLBuildScript
    {
        private const string OutputFolder = "Builds/WebGL";

        [MenuItem("Tools/Mythology TD/Build WebGL")]
        public static void Build()
        {
            // Disabled (not Brotli/Gzip) so the build can be served by any plain static file
            // server without needing to set a Content-Encoding response header. Deploying to a
            // host that properly serves .br/.gz with the right headers can re-enable compression
            // later for a smaller download.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            string[] scenes = { "Assets/_Game/Scenes/Main.unity" };

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputFolder,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"WEBGL_BUILD_RESULT: SUCCEEDED - size {summary.totalSize} bytes, output at '{OutputFolder}'.");
            }
            else
            {
                Debug.LogError($"WEBGL_BUILD_RESULT: FAILED - result={summary.result}, totalErrors={summary.totalErrors}, totalWarnings={summary.totalWarnings}.");
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }
    }
}
