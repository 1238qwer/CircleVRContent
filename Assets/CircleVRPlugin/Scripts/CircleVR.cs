using System;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class Configuration
{
    [Serializable]
    public class TrackerAndUserIDPair
    {
        public string userID;
        public string trackerID;
    }

    [Serializable]
    public class CircleVR
    {
        public TrackerAndUserIDPair[] pairs;
        public string serverIp;
        public int serverPort;
        public float safetyBarrierRadius;
        public bool showBarrier;
        public string[] commands;
    }

    public CircleVR circlevr;

    public CloudVRClient cloudVRClient { get; private set; }

    public void parseCommandLine()
    {
        string defaultConfigPath = Application.dataPath + "/../config.json";

        Dictionary<string, string> pairs = parseCommandLine(Environment.GetCommandLineArgs());

        string config = "config";

        if (pairs != null && pairs.ContainsKey(config) && File.Exists(pairs[config]))
        {
            Debug.Log(File.ReadAllText(pairs[config]));
            JsonUtility.FromJsonOverwrite(File.ReadAllText(pairs[config]), this);
        }
        else
        {
            Debug.Log(defaultConfigPath);
            JsonUtility.FromJsonOverwrite(File.ReadAllText(defaultConfigPath), this);
        }

        foreach (var key in pairs.Keys)
        {
            if (key.Equals("circlevr_server_port"))
            {
                int.TryParse(pairs[key], out circlevr.serverPort);
                Debug.Log("[circlevr] server port override! : " + pairs[key]);
            }
            else if (key.Equals("circlevr_safety_barrier_radius"))
            {
                float.TryParse(pairs[key], out circlevr.safetyBarrierRadius);
                Debug.Log("[circlevr] safety barrier radius override! : " + pairs[key]);
            }
            else if (key.Equals("circlevr_show_barrier"))
            {
                bool.TryParse(pairs[key], out circlevr.showBarrier);
                Debug.Log("[circlevr] show barrier override! : " + pairs[key]);
            }
        }
    }

    private Dictionary<string, string> parseCommandLine(string[] args)
    {
        if (args == null)
        {
            return null;
        }

        Dictionary<string, string> result = new Dictionary<string, string>();
        for (int i = 0; i < args.Length; i++)
        {
            int splitIndex = args[i].IndexOf("=");
            if (splitIndex <= 0)
            {
                continue;
            }

            string name = args[i].Substring(0, splitIndex);
            string value = args[i].Substring(splitIndex + 1);
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
            {
                continue;
            }

            result.Add(name, value);
        }
        return result.Count > 0 ? result : null;
    }

    private void setTrackers(CloudVRTracker[] trackers)
    {
        circlevr.pairs = new TrackerAndUserIDPair[trackers.Length];
        for (int i = 0; i < trackers.Length; i++)
        {
            circlevr.pairs[i] = new TrackerAndUserIDPair();

            if (trackers[i].userID != circlevr.pairs[i].userID)
            {
                circlevr.pairs[i].userID = trackers[i].userID;
            }
            circlevr.pairs[i].trackerID = trackers[i].trackerID;
        }
    }

    public bool isHost
    {
        get
        {
            return CloudVRClient.config.groupServerPort > 0;
        }
    }

    public IEnumerator Load(MonoBehaviour caller)
    {
        parseCommandLine();

        if (isHost)
        {
            circlevr.serverIp = "127.0.0.1";

            cloudVRClient = new CloudVRClient(circlevr.serverIp, CloudVRClient.config.groupServerPort);
            yield return caller.StartCoroutine(cloudVRClient.GetTrackers(caller,
                (trackers) =>
                {
                    setTrackers(trackers);
                },
                (errorCode, error) =>
                {
                    Debug.Log("CloudVRClient.GetTrackers failed : " + errorCode + " : " + error);
                }
            ));
        }
        else
        {
            AirVRServer.LoadOnce();

            circlevr.serverIp = CloudVRClient.config.groupServerAddress;
            JsonUtility.FromJsonOverwrite(CloudVRClient.config.userData, circlevr);
        }
    }
}

public class CircleVR : MonoBehaviour
{
    private static CircleVR instance = null;

    public static CircleVR Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<CircleVR>();

            return instance;
        }
    }

    public const int maxClientCount = 5;

    [SerializeField] private string contentName;
    [SerializeField] private AirVRStereoCameraRig customizedRig;

    public Material barrierMaterial;

    public Transform trackerOrigin;

    private static Configuration config;

    public static string ContentName;

    public static bool IsEditorPlatform()
    {
#if UNITY_EDITOR
        return true;
#else
            return false;
#endif
    }

    private void Awake() 
    {
        CloudVRClient.Initialize();
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(LoadConfigure());

        DisplayInit();

        Setup(config, maxClientCount, customizedRig);

        CircleVRTrackingSystem.Init(config, trackerOrigin);

        GameObject.DontDestroyOnLoad(gameObject);
        ContentName = contentName;
    }

    private static void DisplayInit()
    {
        DontDestroyInstantiate("DisplayManager", Vector3.zero, Quaternion.identity).AddComponent<CircleVRDisplay>().Init();
    }

    private void Setup(Configuration config , int maxClientCount , AirVRStereoCameraRig customizedRig)
    {
        if (IsEditorPlatform())
        {
            CreateControlPannel(config);
            CircleVRHost host = CreateHost(config);
            CreateClientEmulator(host);
        }
        else
        {
            CreateNetwork();

            if (config.isHost)
            {
                CircleVRHost host = CreateHost(config);
                CreateHostNetwork(config, host, maxClientCount);
                return;
            }

            CreateClientNetwork();
        }

        CreateClient(config, customizedRig);
    }

    private static void CreateControlPannel(Configuration config)
    {
        DontDestroyInstantiate("ControlPannel", Vector3.zero, Quaternion.identity).AddComponent<CircleVRControlPannel>().Init(config.circlevr.commands);
    }

    private void CreateClientNetwork()
    {
        DontDestroyInstantiate("ClientNetwork" , Vector3.zero , Quaternion.identity).AddComponent<CircleVRClientNetwork>();
    }
    private void CreateClientEmulator(CircleVRHost host)
    {
        DontDestroyInstantiate("ClientEmulator", Vector3.zero, Quaternion.identity).AddComponent<CircleVRClientEmulator>().host = host;
        Debug.Log("[CircleVR Client Emulator] Instantiated");
    }

    private void CreateClient(Configuration config , AirVRStereoCameraRig rig)
    {
        DontDestroyInstantiate("Client", Vector3.zero, Quaternion.identity).AddComponent<CircleVRClient>().Init(config , rig);
    }

    public static Configuration.TrackerAndUserIDPair GetPair(string userID)
    {
        foreach (Configuration.TrackerAndUserIDPair pair in config.circlevr.pairs)
        {
            if (pair.userID == userID)
                return pair;
        }

        Debug.Log("[CircleVR Error] Not Found Pair In Data : " + userID);
        return null;
    }

    public static bool HasUserIDInPairs(string userID)
    {
        foreach (Configuration.TrackerAndUserIDPair pair in config.circlevr.pairs)
        {
            if (pair.userID == userID)
                return true;
        }

        return false;
    }

    public static bool HasTrackerIDInPairs(string trackerID)
    {
        foreach (Configuration.TrackerAndUserIDPair pair in config.circlevr.pairs)
        {
            if (pair.trackerID == trackerID)
                return true;
        }

        return false;
    }

    private void CreateNetwork()
    {
        DontDestroyInstantiate("Network", Vector3.zero, Quaternion.identity).AddComponent<CircleVRNetwork>();
    }

    private CircleVRHostNetwork CreateHostNetwork(Configuration config , CircleVRHost host , int maxClientCount)
    {
        CircleVRHostNetwork hostNetwork = DontDestroyInstantiate("HostNetwork", Vector3.zero, Quaternion.identity).AddComponent<CircleVRHostNetwork>();
        hostNetwork.Init(config, host, maxClientCount);
        return hostNetwork;
    }

    private CircleVRHost CreateHost(Configuration config)
    {
        CircleVRDisplay.InitBarrier(config.circlevr.safetyBarrierRadius, config.circlevr.showBarrier);
        CircleVRHost host = DontDestroyInstantiate("Host" , Vector3.zero , Quaternion.identity).AddComponent<CircleVRHost>();
        host.Init(config);

        return host;
    }

    public static GameObject DontDestroyInstantiate(GameObject prefab, Vector3 position, Quaternion quaternion)
    {
        GameObject newGameObject = GameObject.Instantiate(prefab, position, quaternion);
        GameObject.DontDestroyOnLoad(newGameObject);
        return newGameObject;
    }

    public static GameObject DontDestroyInstantiate(string name, Vector3 position, Quaternion quaternion)
    {
        GameObject newGameObject = new GameObject(name);
        newGameObject.transform.position = position;
        newGameObject.transform.rotation = quaternion;
        GameObject.DontDestroyOnLoad(newGameObject);
        return newGameObject;
    }

    private IEnumerator LoadConfigure()
    {
        if(IsEditorPlatform())
        {
            config = JsonUtility.FromJson<Configuration>(File.ReadAllText("Assets/CircleVRPlugin/Manifest/config.json"));
            yield break;
        }

        config = new Configuration();
        yield return StartCoroutine(config.Load(this));
    }
}