using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

public class CloudVRBuilder : Editor {
    [Serializable]
    private class Manifest {
        [Serializable]
        public class Launch {
#pragma warning disable 414
            public string path;
#pragma warning disable 414
        }
        public Launch launch;
    }

    public static void Build(string targetFolder) {
        if (Directory.Exists(targetFolder) == false) {
            Directory.CreateDirectory(targetFolder);
        }

        string name = Path.GetFileNameWithoutExtension(targetFolder) + ".exe";
        try {
            Manifest manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Application.dataPath, "onCloudVR/Manifest/content.json")));
            name = manifest.launch.path;
        }
        catch (Exception) { }

        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, Path.Combine(targetFolder, name), BuildTarget.StandaloneWindows64, BuildOptions.None);

        string manifestDest = Path.Combine(targetFolder, ".onairvr");
        if (Directory.Exists(manifestDest) == false) {
            Directory.CreateDirectory(manifestDest);
        }
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "onCloudVR/Manifest"))) {
            if (Path.GetExtension(file).Equals(".meta")) {
                continue;
            }
            try {
                File.Copy(file, Path.Combine(manifestDest, Path.GetFileName(file)), true);
            }
            catch (Exception) { }
        }
    }

    [MenuItem("onCloudVR/Build...")]
    public static void Build() {
        string targetFolder = EditorUtility.SaveFolderPanel("Build...", "", "");
        if (string.IsNullOrEmpty(targetFolder)) {
            Debug.Log("[onCloudVR] Build cancelled");
            return;
        }

        Debug.Log("[onCloudVR] Building at " + targetFolder + " ...");
        Build(targetFolder);

        Debug.Log("[onCloudVR] Build done.");
    }
}
