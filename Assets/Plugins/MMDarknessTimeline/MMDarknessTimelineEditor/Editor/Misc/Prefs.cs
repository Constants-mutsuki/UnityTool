﻿#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;


namespace Darkness
{
    public static class Prefs
    {
        public static readonly string ConfigPath = $"{Application.dataPath}/../ProjectSettings/NBCActionEditor.txt";

        [Serializable]
        public enum TimeStepMode
        {
            Seconds,
            Frames
        }

        [Serializable]
        class SerializedData
        {
            public TimeStepMode TimeStepMode = TimeStepMode.Seconds;
            public float SnapInterval = 0.1f;
            public int FrameRate = 30;

            public int AutoSaveSeconds;
            public string SavePath = "Assets/";
            public string SerialieSavePath = "Assets/";
            public bool ScrollWheelZooms = true;

            public bool MagnetSnapping = true;
            public float TrackListLeftMargin = 180f;
        }


        private static SerializedData _data;

        private static SerializedData data
        {
            get
            {
                if (_data == null)
                {
                    if (File.Exists(ConfigPath))
                    {
                        var json = File.ReadAllText(ConfigPath);
                        _data = JsonUtility.FromJson<SerializedData>(json);
                    }

                    if (_data == null)
                    {
                        _data = new SerializedData();
                    }
                }

                return _data;
            }
        }

        public static readonly float[] snapIntervals = new float[] { 0.001f, 0.01f, 0.1f };
        public static readonly int[] frameRates = new int[] { 24, 25, 30, 60 };

        public static bool scrollWheelZooms
        {
            get => data.ScrollWheelZooms;
            set
            {
                if (data.ScrollWheelZooms != value)
                {
                    data.ScrollWheelZooms = value;
                    Save();
                }
            }
        }


        public static int autoSaveSeconds
        {
            get => data.AutoSaveSeconds;
            set
            {
                if (data.AutoSaveSeconds != value)
                {
                    data.AutoSaveSeconds = value;
                    Save();
                }
            }
        }

        public static string savePath
        {
            get => data.SavePath;
            set
            {
                if (data.SavePath != value)
                {
                    data.SavePath = value;
                    Save();
                }
            }
        }

        public static string SerializeSavePath
        {
            get => data.SerialieSavePath;
            set
            {
                if (data.SerialieSavePath != value)
                {
                    data.SerialieSavePath = value;
                    Save();
                }
            }
        }

        public static bool magnetSnapping
        {
            get => data.MagnetSnapping;
            set
            {
                if (data.MagnetSnapping != value)
                {
                    data.MagnetSnapping = value;
                    Save();
                }
            }
        }

        public static float trackListLeftMargin
        {
            get => data.TrackListLeftMargin;
            set
            {
                if (Math.Abs(data.TrackListLeftMargin - value) > 0.001f)
                {
                    data.TrackListLeftMargin = value;
                    Save();
                }
            }
        }

        public static TimeStepMode timeStepMode
        {
            get => data.TimeStepMode;
            set
            {
                if (data.TimeStepMode != value)
                {
                    data.TimeStepMode = value;
                    frameRate = value == TimeStepMode.Frames ? 30 : 10;
                    Save();
                }
            }
        }

        public static int frameRate
        {
            get => data.FrameRate;
            set
            {
                if (data.FrameRate != value)
                {
                    data.FrameRate = value;
                    snapInterval = 1f / value;
                    Save();
                }
            }
        }

        public static float snapInterval
        {
            get => Mathf.Max(data.SnapInterval, 0.001f);
            set
            {
                if (Math.Abs(data.SnapInterval - value) > 0.001f)
                {
                    data.SnapInterval = Mathf.Max(value, 0.001f);
                    Save();
                }
            }
        }

        static void Save()
        {
            System.IO.File.WriteAllText(ConfigPath, JsonUtility.ToJson(data));
        }


        public static readonly Dictionary<string, Type> AssetTypes = new Dictionary<string, Type>();
        public static readonly List<string> AssetNames = new List<string>();

        public static void InitializeAssetTypes()
        {
            AssetTypes.Clear();
            AssetNames.Clear();
            var types = ReflectionTools.GetImplementationsOf(typeof(TimelineGraphAsset));
            foreach (var t in types)
            {
                var typeName = t.GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault() is NameAttribute nameAtt ? nameAtt.Name : t.Name.SplitCamelCase();
                AssetTypes[typeName] = t;
                AssetNames.Add(typeName);
            }
        }

        public static string GetAssetTypeName(Type type)
        {
            foreach (var key in AssetTypes.Keys)
            {
                var v = AssetTypes[key];
                if (v == type)
                {
                    return key;
                }
            }

            return string.Empty;
        }
    }
}

#endif
