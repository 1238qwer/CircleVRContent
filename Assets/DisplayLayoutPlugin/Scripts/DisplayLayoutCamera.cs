using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DisplayLayoutCamera : MonoBehaviour
{
    [Serializable]
    public class Config
    {
        [Serializable]
        public class Display
        {
            public string ID;
            public float fov = 60;
            public float aspectRatio = 1.77f;
            public Rect rect;
            public int targetDisplayIndex = 0;
        }

        public Display[] displays;

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

        public void ParseCommandLine()
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
                try
                {
                    Debug.Log(defaultConfigPath);
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(defaultConfigPath), this);
                }
                catch
                {
                    Debug.Log("[display] Not found config : " + defaultConfigPath);
                }
            }

            foreach (var key in pairs.Keys)
            {
                if (!key.Contains("["))
                    continue;

                string ID = GetID(key);
                Debug.Log(ID);
                if (key.Contains("display_fov"))
                {
                    GetDisplay(ID).fov = float.Parse(pairs[key]);
                    Debug.Log("[display] fov changed : " + pairs[key]);
                }
                else if (key.Contains("display_aspect_ratio"))
                {
                    GetDisplay(ID).aspectRatio = float.Parse(pairs[key]);
                    Debug.Log("[display] aspectRatio changed : " + pairs[key]);
                }
                else if (key.Contains("display_rect"))
                {
                    GetDisplay(ID).rect = RectParse(pairs[key]);
                    Debug.Log("[display] rect changed : " + GetDisplay(ID).rect.ToString());
                }
                else if (key.Contains("display_target_display_index"))
                {
                    GetDisplay(ID).targetDisplayIndex = int.Parse(pairs[key]);
                    Debug.Log("[display] targetDisplayIndex changed : " + pairs[key]);
                }
            }
        }

        private Display GetDisplay(string ID)
        {
            foreach (var display in displays)
            {
                if (display.ID == ID)
                    return display;
            }

            return null;
        }

        private Rect RectParse(string rect)
        {
            Rect result = new Rect();

            string[] splits = rect.Split(',');
            foreach (var split in splits)
            {
                if (split.Contains("x"))
                {
                    int startIndex = split.IndexOf(":");
                    result.x = float.Parse(split.Substring(startIndex + 1));
                }
                else if (split.Contains("y"))
                {
                    int startIndex = split.IndexOf(":");
                    result.y = float.Parse(split.Substring(startIndex + 1));
                }
                else if (split.Contains("width"))
                {
                    int startIndex = split.IndexOf(":");
                    result.width = float.Parse(split.Substring(startIndex + 1));
                }
                else if (split.Contains("height"))
                {
                    int startIndex = split.IndexOf(":");
                    result.height = float.Parse(split.Substring(startIndex + 1));
                }
            }

            Debug.Log("[display] parsing result : " + result.ToString());

            return result;
        }

        private static string GetID(string key)
        {
            int idStartIndex = key.IndexOf("[");
            int idEndIndex = key.IndexOf("]");
            int length = idEndIndex - idStartIndex - 1;

            string id = key.Substring(idStartIndex+1, length);
            return id;
        }
    }
    
    public static Config config { private set; get; }

    public string id;
    public Transform target;
    public float t;

    private void Awake()
    {
        GameObject.DontDestroyOnLoad(this);

        if (config == null )
        {
            if(Application.isEditor)
                config = JsonUtility.FromJson<Config>(File.ReadAllText("Assets/DisplayLayoutPlugin/Manifest/config.json"));
            else
            {
                config = new Config();
                config.ParseCommandLine();
            }
        }

        Config.Display data = GetData(id);

        if (data == null)
        {
            Debug.Log("[DLP] Display Inactivated : " + id);
            gameObject.SetActive(false);
            return;
        }

        Camera cam = GetComponent<Camera>();

        cam.stereoTargetEye = StereoTargetEyeMask.None;
        cam.fieldOfView = data.fov;
        cam.aspect = data.aspectRatio;
        cam.rect = data.rect;
        cam.targetDisplay = data.targetDisplayIndex;

        if(UseMultiDisplay())
        {
            try
            {
                if(!Display.displays[data.targetDisplayIndex].active)
                    Display.displays[data.targetDisplayIndex].Activate();
            }
            catch
            {
                Debug.Log("[DisplayLayout] " + data.targetDisplayIndex + " Display is not available");
            }
        }
    }

    private bool UseMultiDisplay()
    {
        int compare = -1;

        foreach (var data in config.displays)
        {
            if(compare == data.targetDisplayIndex || compare == -1)
            {
                compare = data.targetDisplayIndex;
                continue;
            }

            return true;
        }

        return false;
    }

    private Config.Display GetData(string id)
    {
        foreach (var data in config.displays)
        {
            if (data.ID == id)
                return data;
        }

        Debug.Log("[DisplayLayout] Not Found ID : " + id);
        return null;
    }

    private void LateUpdate()
    {
        if (!target)
            return;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, target.rotation, t);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target.position, t);
    }
}
