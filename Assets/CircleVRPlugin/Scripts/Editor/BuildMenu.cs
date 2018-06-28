using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class BuildMenu : Editor {
    private static string _productName;
    private static bool _lastVRSupported;

    private static void clearFolder(string targetFolder) {
        foreach (string file in Directory.GetFiles(targetFolder)) {
            File.Delete(file);
        }
        foreach (string dir in Directory.GetDirectories(targetFolder)) {
            Directory.Delete(dir, true);
        }
    }

    private static void saveSettings() {
        _productName = PlayerSettings.productName;
        _lastVRSupported = PlayerSettings.virtualRealitySupported;
    }

    private static void setSettings(string productNamePostfix, bool virtualRealitySupported) {
        PlayerSettings.productName = _productName + "_" + productNamePostfix;
        PlayerSettings.virtualRealitySupported = virtualRealitySupported;
    }

    private static void restoreSettings() {
        PlayerSettings.virtualRealitySupported = _lastVRSupported;
        PlayerSettings.productName = _productName;
    }

    private static void buildStandaloneClient(string targetFolder) {
        Debug.Log("[BUILD] Building client at " + targetFolder + " ...");

        setSettings("Client", false);
        CloudVRBuilder.Build(targetFolder);
    }

    private static void buildStandaloneHost(string targetFolder) {
        Debug.Log("[BUILD] Building host at " + targetFolder + " ...");

        setSettings("Host", true);
        CloudVRBuilder.Build(targetFolder);
    }

    [MenuItem("CircleVR/Build All...")]
    public static void BuildAll() {
        string targetFolder = EditorUtility.SaveFolderPanel("Build All...", "", "");
        if (string.IsNullOrEmpty(targetFolder)) {
            Debug.Log("[BUILD] Build cancelled");
            return;
        }

        saveSettings();

        buildStandaloneClient(Path.Combine(targetFolder, "Client"));
        buildStandaloneHost(Path.Combine(targetFolder, "Host"));

        restoreSettings();

        Debug.Log("[BUILD] Done");
    }

    [MenuItem("CircleVR/Build Client...")]
    public static void BuildClient() {
        string targetFolder = EditorUtility.SaveFolderPanel("Build Client...", "", "");
        if (string.IsNullOrEmpty(targetFolder)) {
            Debug.Log("[BUILD] Build cancelled");
            return;
        }

        saveSettings();

        clearFolder(targetFolder);
        buildStandaloneClient(targetFolder);

        restoreSettings();

        Debug.Log("[BUILD] Done");
    }

    [MenuItem("CircleVR/Build Host...")]
    public static void BuildHost() {
        string targetFolder = EditorUtility.SaveFolderPanel("Build Host...", "", "");
        if (string.IsNullOrEmpty(targetFolder)) {
            Debug.Log("[BUILD] Build cancelled");
            return;
        }

        saveSettings();

        clearFolder(targetFolder);
        buildStandaloneHost(targetFolder);

        restoreSettings();

        Debug.Log("[BUILD] Done");
    }
}
