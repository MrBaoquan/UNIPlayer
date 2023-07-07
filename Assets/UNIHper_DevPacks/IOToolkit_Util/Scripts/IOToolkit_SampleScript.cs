using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNIHper;
using IOToolkit;

public class IOToolkit_SampleScript : SceneScriptBase
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod, UnityEditor.InitializeOnEnterPlayMode]
    public static void AddAssemblyToUNIHper()
    {
        var _currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        UNIHperSettings.AddAssemblyToSettingsIfNotExists(_currentAssembly);
    }
#endif

    private void Awake()
    {
        Managements.Resource.AddConfig("IOToolkit_resources");
        Managements.UI.AddConfig("IOToolkit_uis");
    }

    // Called once after scene is loaded
    private void Start()
    {
        // Load IODevice.xml
        IOToolkit_Extension.IORoot.Instance.Load();
        IOToolkit_Extension.IORoot.Instance.Save();
        IODeviceController.UnLoad();
        IODeviceController.Load();
        Debug.LogWarning("------------------");
    }

    // Called per frame after Start
    private void Update()
    {
        IODeviceController.Update();
    }

    // Called when scene is unloaded
    private void OnDestroy()
    {
        IODeviceController.UnLoad();
    }

    // Called when application is quit
    private void OnApplicationQuit() { }
}
