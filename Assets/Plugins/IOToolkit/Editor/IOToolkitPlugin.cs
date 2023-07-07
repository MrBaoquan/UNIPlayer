using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using IOToolkit;

[InitializeOnLoad]
public class IOToolkitPlugin
{
    private const string dataDirSuffix = "_Data";
    private const string pluginsDirName = "Plugins";

    static IOToolkitPlugin()
    {
        try
        {
            IOSettings.SetIOConfigPath(IOToolkitUtil.ConfigPath);
        }
        catch (System.Exception) { }
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("Build completed");
        string _buildName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        var _targetDir = Directory.GetParent(pathToBuiltProject);
        var _separator = Path.DirectorySeparatorChar;

        var _buildDataDir =
            _targetDir.FullName + _separator + _buildName + dataDirSuffix + _separator;
        var _buildPluginsDir =
            _buildDataDir + _separator + pluginsDirName + _separator + _separator;
        var _srcPluginsDir =
            Application.dataPath
            + _separator
            + pluginsDirName
            + _separator
            + "IOToolkit"
            + _separator;

#if UNITY_STANDALONE_WIN
        var _platform =
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64
                ? "x86_64"
                : "x86";

        CopyAll(
            new DirectoryInfo(Path.Combine(_srcPluginsDir, "Config")),
            new DirectoryInfo(Path.Combine(_buildPluginsDir, _platform, "Config"))
        );
        CopyAll(
            new DirectoryInfo(
                Path.Combine(_srcPluginsDir, $"Plugins/{_platform}/ExternalLibraries")
            ),
            new DirectoryInfo(Path.Combine(_buildPluginsDir, $"{_platform}/ExternalLibraries"))
        );

        File.Copy(
            Path.Combine(_srcPluginsDir, $"Plugins/{_platform}", "IODevice.dll"),
            Path.Combine(_buildPluginsDir, $"{_platform}/IODevice.dll"),
            true
        );
        File.Copy(
            Path.Combine(_srcPluginsDir, $"Plugins/{_platform}", "IODevice_C_Wrapper.dll"),
            Path.Combine(_buildPluginsDir, $"{_platform}/IODevice_C_Wrapper.dll"),
            true
        );

        Directory
            .GetFiles(
                Path.Combine(_srcPluginsDir, $"Plugins/{_platform}/ExternalLibraries"),
                "*.dll",
                SearchOption.AllDirectories
            )
            .Select(_ => Path.GetFileName(_))
            .Concat(new string[] { "IODevice.dll", "IODevice_C_Wrapper.dll" })
            .Select(_fileName => Path.Combine(_buildPluginsDir, _fileName))
            .Where(_filePath => File.Exists(_filePath))
            .ToList()
            .ForEach(_filePath => File.Delete(_filePath));

        Directory
            .GetFiles(
                Path.Combine(_buildPluginsDir, $"{_platform}/ExternalLibraries"),
                "*.dll",
                SearchOption.AllDirectories
            )
            .Select(_fullPath => Path.GetFileName(_fullPath))
            .Select(_fileName => Path.Combine(_buildPluginsDir, $"{_platform}/{_fileName}"))
            .ToList()
            .ForEach(_path =>
            {
                if (File.Exists(_path))
                    File.Delete(_path);
            });

#endif
    }

    private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        // Check if the source directory exists, if not, don't do any work.
        if (!Directory.Exists(source.FullName))
        {
            return;
        }

        // Check if the target directory exists, if not, create it.
        if (!Directory.Exists(target.FullName))
        {
            Directory.CreateDirectory(target.FullName);
        }

        // Copy each file into it’s new directory.
        foreach (var fileInfo in source.GetFiles())
        {
            if (Path.GetExtension(fileInfo.Name) == ".meta")
                continue;
            fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (var subDirInfo in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(subDirInfo.Name);
            CopyAll(subDirInfo, nextTargetSubDir);
        }
    }
}
