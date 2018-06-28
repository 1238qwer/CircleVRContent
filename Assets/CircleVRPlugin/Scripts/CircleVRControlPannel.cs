using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVRControlPannel : MonoBehaviour
{
    private string[] commands;

    public static event ControlPannelEvent onCommand;

    public delegate void ControlPannelEvent(string command);

    private void OnGUI()
    {
        if (commands == null)
            return;

        for (int i = 0; i < commands.Length; i++)
        {
            if (GUI.Button(new Rect(i * Screen.width / commands.Length * 0.5f + Screen.width * 0.5f, Screen.height - Screen.height / commands.Length * 0.5f, Screen.width / commands.Length * 0.5f, Screen.height / commands.Length *0.5f), 
                commands[i]))
            {
                if (onCommand != null)
                    onCommand(commands[i]);

                Debug.Log(commands[i]);
            }
        }
    }

    public void Init(string[] commands)
    {
        this.commands = commands;
    }
}
