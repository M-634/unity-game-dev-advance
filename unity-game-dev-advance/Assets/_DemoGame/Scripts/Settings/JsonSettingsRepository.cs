using System;
using System.IO;
using UnityEngine;

namespace DemoGame
{
    // persistentDataPath に JSON で保存する。ファイルが無い・壊れているときは既定値を返す。
    public sealed class JsonSettingsRepository : ISettingsRepository
    {
        private const string FileName = "settings.json";

        private readonly string _path;

        public JsonSettingsRepository()
            : this(Path.Combine(Application.persistentDataPath, FileName))
        {
        }

        public JsonSettingsRepository(string path)
        {
            _path = path;
        }

        public SettingsData Load()
        {
            if (!File.Exists(_path)) return new SettingsData();

            try
            {
                var data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(_path));
                if (data == null) return new SettingsData();

                data.Graphics ??= new GraphicsSettingsData();
                data.Sound ??= new SoundSettingsData();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{nameof(JsonSettingsRepository)}: 設定を読めなかったので既定値を使います。{_path}\n{e.Message}");
                return new SettingsData();
            }
        }

        public void Save(SettingsData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
                File.WriteAllText(_path, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(JsonSettingsRepository)}: 設定を保存できませんでした。{_path}\n{e.Message}");
            }
        }
    }
}
