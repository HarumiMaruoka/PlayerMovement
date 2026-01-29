using Game.Player.Movement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.Debug
{
    public class Recorder
    {
        [Serializable]
        public struct Data
        {
            public Vector2 Position;
        }

        [Serializable]
        private class RecordListWrapper
        {
            public List<Data> Records = new List<Data>();
        }

        public List<Data> records = new List<Data>();

        public List<Data> GetRecords()
        {
            return new List<Data>(records);
        }

        public void Record(Recorder.Data data)
        {
            records.Add(data);
        }

        public void Save(string filePath)
        {
            var path = Path.Combine(Application.persistentDataPath, filePath);
            var wrapper = new RecordListWrapper { Records = records };
            var json = JsonUtility.ToJson(wrapper);
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, json);
        }

        public static List<Data> Load(string filePath)
        {
            var path = Path.Combine(Application.persistentDataPath, filePath);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<RecordListWrapper>(json);
                return wrapper?.Records;
            }
            return null;
        }

        public static List<Data> Load(TextAsset textAsset)
        {
            if (textAsset != null)
            {
                var json = textAsset.text;
                var wrapper = JsonUtility.FromJson<RecordListWrapper>(json);
                return wrapper?.Records;
            }
            return null;
        }
    }
}