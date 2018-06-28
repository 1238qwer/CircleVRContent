using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DisplayLayoutFade : MonoBehaviour
{
    private static List<DisplayLayoutFade> _cameraFades = new List<DisplayLayoutFade>();

    public static IEnumerator FadeAllCameras(MonoBehaviour caller, bool fadeIn, float duration)
    {
        foreach (DisplayLayoutFade cameraFade in _cameraFades)
        {
            caller.StartCoroutine(cameraFade.Fade(fadeIn , duration));
        }

        for (bool anyCameraIsFading = true; anyCameraIsFading;)
        {
            anyCameraIsFading = false;
            foreach (DisplayLayoutFade cameraFade in _cameraFades)
            {
                anyCameraIsFading = anyCameraIsFading || cameraFade.isFading;
                if (anyCameraIsFading)
                {
                    break;
                }
            }
            if (anyCameraIsFading)
            {
                yield return null;
            }
        }
    }

    public static void FadeAllCamerasImmediately(bool fadeIn)
    {
        foreach (DisplayLayoutFade cameraFade in _cameraFades)
        {
            cameraFade.FadeImmediately(fadeIn);
        }
    }

    private Material _fadeMaterial;
    private Color _startFadeColor;
    private Color _endFadeColor;
    private float _startTimeToFade;

    public bool fadeInOnStart;

    [SerializeField]
    internal Color fadeOutColor = Color.black;

    private void Awake()
    {
        _fadeMaterial = new Material(Shader.Find("DisplayLayout/Unlit transparent color"));
        _fadeMaterial.color = Color.clear;

        _cameraFades.Add(this);
    }

    private void Start()
    {
        if (fadeInOnStart)
            StartCoroutine(Fade(true));
    }

    private void OnDestroy()
    {
        _cameraFades.Remove(this);
    }

    private void OnPostRender()
    {
        if (_fadeMaterial.color != Color.clear)
        {
            _fadeMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Color(_fadeMaterial.color);
            GL.Begin(GL.QUADS);
            GL.Vertex3(0.0f, 0.0f, -1.0f);
            GL.Vertex3(0.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 0.0f, -1.0f);
            GL.End();
            GL.PopMatrix();
        }
    }

    public bool isFading { get; private set; }

    public float duration;

    public IEnumerator Fade(bool fadeIn, Action onStart = null, Action onComplete = null)
    {
        yield return Fade(fadeIn, duration, onStart, onComplete);
    }

    public IEnumerator Fade(bool fadeIn, float duration, Action onStart = null, Action onComplete = null)
    {
        if (onStart != null)
            onStart.Invoke();

        _startFadeColor = isFading ? _fadeMaterial.color : (fadeIn ? fadeOutColor : Color.clear);
        _endFadeColor = fadeIn ? Color.clear : fadeOutColor;

        _startTimeToFade = Time.realtimeSinceStartup;

        if (isFading == false)
        {
            isFading = true;
            _fadeMaterial.color = _startFadeColor;
            while (_fadeMaterial.color != _endFadeColor)
            {
                _fadeMaterial.color = Color.Lerp(_startFadeColor, _endFadeColor, (Time.realtimeSinceStartup - _startTimeToFade) / duration);
                yield return null;
            }
            isFading = false;
        }

        if (onComplete != null)
            onComplete.Invoke();
    }

    public void FadeImmediately(bool fadeIn)
    {
        _startFadeColor = fadeIn ? fadeOutColor : Color.clear;
        _endFadeColor = fadeIn ? Color.clear : fadeOutColor;

        _fadeMaterial.color = _endFadeColor;
    }
}
