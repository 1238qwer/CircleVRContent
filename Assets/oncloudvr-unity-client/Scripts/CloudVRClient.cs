using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class CloudVRClientConfigReader {
#pragma warning disable 414
    [SerializeField] private CloudVRClientConfig oncloudvr;
#pragma warning restore 414

    public void ReadParams(string fileFrom, CloudVRClientConfig to) {
        oncloudvr = to;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(fileFrom), this);
    }
}

[Serializable]
public class CloudVRClientConfig {
    public string groupServerAddress;
    public int groupServerPort;
    public string userData;

    private int parseInt(string value, int defaultValue, Func<int, bool> predicate, Action<string> failed = null) {
        int result;
        if (int.TryParse(value, out result) && predicate(result)) {
            return result;
        }

        if (failed != null) {
            failed(value);
        }
        return defaultValue;
    }

    private Dictionary<string, string> parseCommandLine(string[] args) {
        if (args == null) {
            return null;
        }

        Dictionary<string, string> result = new Dictionary<string, string>();
        for (int i = 0; i < args.Length; i++) {
            int splitIndex = args[i].IndexOf("=");
            if (splitIndex <= 0) {
                continue;
            }

            string name = args[i].Substring(0, splitIndex);
            string value = args[i].Substring(splitIndex + 1);
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) {
                continue;
            }

            result.Add(name, value);
        }
        return result.Count > 0 ? result : null;
    }

    public void ParseCommandLineArgs(string[] args) {
        Dictionary<string, string> pairs = parseCommandLine(args);
        if (pairs == null) {
            return;
        }

        string keyConfigFile = "config";
        if (pairs.ContainsKey(keyConfigFile)) {
            if (File.Exists(pairs[keyConfigFile])) {
                try {
                    CloudVRClientConfigReader reader = new CloudVRClientConfigReader();
                    reader.ReadParams(pairs[keyConfigFile], this);
                }
                catch (Exception e) {
                    Debug.LogWarning("[onCloudVR] WARNING: failed to parse " + pairs[keyConfigFile] + " : " + e.ToString());
                }
            }
            pairs.Remove("config");
        }

        foreach (string key in pairs.Keys) {
            if (key.Equals("onairvr_group_server")) {
                groupServerAddress = pairs[key];
            }
            else if (key.Equals("onairvr_gcs_port")) {
                groupServerPort = parseInt(pairs[key], groupServerPort,
                    (parsed) => {
                        return 0 <= parsed && parsed <= 65535;
                    },
                    (val) => {
                        Debug.LogWarning("[onCloudVR] WARNING: Group server port number is invalid : " + val);
                    });
            }
            else if (key.Equals("onairvr_user_data")) {
                userData = WWW.UnEscapeURL(pairs[key]);
            }
        }
    }
}

public class CloudVRClient {
    private static CloudVRClientConfig _config;
    
    public static CloudVRClientConfig config { get { return _config; } }

    public static void Initialize() {
        _config = new CloudVRClientConfig();
        _config.ParseCommandLineArgs(Environment.GetCommandLineArgs());
    }

    public CloudVRClient(string host, int port) {
        this.host = host;
        this.port = port;
    }

    public string host { get; private set; }
    public int port { get; private set; }

    public IEnumerator LoadContentsList(MonoBehaviour caller, Action<CloudVRContent[]> onSucceeded, Action<long, string> onFailed) {
        UnityWebRequest request = CloudVRAPIHelper.Get(this, "/contents");
        yield return caller.StartCoroutine(CloudVRAPIHelper.HandleResponse(request, 
            () => {
                List<object> contents = MiniJSON.Json.Deserialize(request.downloadHandler.text) as List<object>;
                if (contents == null || contents.Count == 0) {
                    onSucceeded(null);
                    return;
                }

                CloudVRContent[] result = new CloudVRContent[contents.Count];
                for (int i = 0; i < contents.Count; i++) {
                    result[i] = new CloudVRContent(this, contents[i] as Dictionary<string, object>);
                }
                onSucceeded(result);
            }, 
            () => {
                onFailed(request.responseCode, request.error);
            }));
    }

    public IEnumerator GetTrackers(MonoBehaviour caller, Action<CloudVRTracker[]> onSucceeded, Action<long, string> onFailed) {
        UnityWebRequest request = CloudVRAPIHelper.Get(this, "/trackers");
        yield return caller.StartCoroutine(CloudVRAPIHelper.HandleResponse(request, 
            () => {
                List<object> trackers = MiniJSON.Json.Deserialize(request.downloadHandler.text) as List<object>;
                if (trackers == null || trackers.Count == 0) {
                    onSucceeded(null);
                    return;
                }
                
                CloudVRTracker[] result = new CloudVRTracker[trackers.Count];
                for (int i = 0; i < trackers.Count; i++) {
                    result[i] = new CloudVRTracker(trackers[i] as Dictionary<string, object>);
                }
                onSucceeded(result);
            },
            () => {
                onFailed(request.responseCode, request.error);
            }
        ));
    }
    
    public IEnumerator RequestGroupLinkage(MonoBehaviour caller, Action<CloudVRGroupLinkage> onSucceeded, Action<long, string> onFailed) {
        UnityWebRequest request = CloudVRAPIHelper.Get(this, "/linkage");
        yield return caller.StartCoroutine(CloudVRAPIHelper.HandleResponse(request,
            () => {
                Dictionary<string, object> linkage =
                    MiniJSON.Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;

                if (linkage == null) {
                    onSucceeded(null);
                    return;
                }
                
                onSucceeded(new CloudVRGroupLinkage(linkage));
            },
            () => {
                onFailed(request.responseCode, request.error);
            }
        ));
    }
}
