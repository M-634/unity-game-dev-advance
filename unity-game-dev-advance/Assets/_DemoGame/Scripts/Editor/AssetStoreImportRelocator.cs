using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AssetStoreImportRelocator
{
    private const string RootFolder = "Assets";
    private const string SubFolder = "_GitIgnore";

    private static readonly string TargetRoot = $"{RootFolder}/{SubFolder}";
    private const string SnapshotKey = "AssetStoreImportRelocator.Snapshot";

    private static readonly string[] Excluded =
    {
        "Plugins", "Editor", "Editor Default Resources", "Gizmos",
        "StreamingAssets", "Standard Assets"
    };

    static AssetStoreImportRelocator()
    {
        AssetDatabase.importPackageStarted += OnImportStarted;
        AssetDatabase.importPackageCompleted += OnImportCompleted;
    }

    private static void OnImportStarted(string packageName)
    {
        SessionState.SetString(SnapshotKey, string.Join("|", AssetDatabase.GetSubFolders(RootFolder)));
    }

    private static void OnImportCompleted(string packageName)
    {
        string[] before = SessionState.GetString(SnapshotKey, "").Split('|');
        var added = AssetDatabase.GetSubFolders(RootFolder)
            .Except(before)
            .Where(path => path != TargetRoot && !Excluded.Contains(Path.GetFileName(path)));

        if (!AssetDatabase.IsValidFolder(TargetRoot))
        {
            AssetDatabase.CreateFolder(RootFolder, SubFolder);
        }

        foreach (string path in added)
        {
            var destination = $"{TargetRoot}/{Path.GetFileName(path)}";
            string error = AssetDatabase.MoveAsset(path, destination);
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log($"Moved {path} -> {destination}");
            }
            else
            {
                Debug.LogWarning($"Could not move {path}: {error}");
            }
        }
    }
}
