using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Darkness
{
    public delegate void CallbackFunction();

    public delegate void OpenAssetFunction(TimelineGraphAsset timelineGraphAsset);

    public class App
    {
        private static TextAsset m_textAsset;
        public static CallbackFunction OnInitialize;
        public static CallbackFunction OnDisable;
        public static OpenAssetFunction OnOpenAsset;
        public static CallbackFunction OnPlay;
        public static CallbackFunction OnStop;

        public static TimelineGraphAsset GraphAsset { get; set; }

        public static TimelineGraph GraphModel { get; set; }

        public static void OnObjectPickerConfig(Object obj)
        {
            if (obj is TimelineGraphAsset a)
            {
                GraphAsset = a;
                GraphAsset.Validate();
            }
        }

        public static void SaveAsset()
        {
            if (GraphAsset)
                EditorUtility.SetDirty(GraphAsset);
        }


        #region AutoSave
        private static DateTime m_lastSaveTime = DateTime.Now;
        public static DateTime LastSaveTime => m_lastSaveTime;


        /// <summary>
        /// 尝试自动保存
        /// </summary>
        public static void TryAutoSave()
        {
            var timespan = DateTime.Now - m_lastSaveTime;
            if (timespan.Seconds > Prefs.autoSaveSeconds)
            {
                AutoSave();
            }
        }

        public static void AutoSave()
        {
            m_lastSaveTime = DateTime.Now;
            SaveAsset();
        }
        #endregion

        #region 播放相关
        private static AssetPlayer m_player => AssetPlayer.Instance;

        public static bool IsStop => Application.isPlaying ? m_player.IsPaused || !m_player.IsActive : EditorPlaybackState == EditorPlaybackState.Stopped;

        internal static EditorPlaybackState EditorPlaybackState = EditorPlaybackState.Stopped;

        public static WrapMode EditorPlaybackWrapMode = WrapMode.Loop;

        public static bool IsPlay => m_player.CurrentTime > 0;

        public static void Play(Action callback = null)
        {
            Play(EditorPlaybackState.PlayingForwards, callback);
        }

        private static void Play(EditorPlaybackState playbackState, Action callback = null)
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorPlaybackState = playbackState;
            OnPlay?.Invoke();
        }

        public static void Pause()
        {
            EditorPlaybackState = EditorPlaybackState.Stopped;
            OnStop?.Invoke();
        }

        public static void Stop(bool forceRewind)
        {
            if (GraphAsset)
                m_player.CurrentTime = 0;
            EditorPlaybackState = EditorPlaybackState.Stopped;
            OnStop?.Invoke();
        }

        public static void StepForward()
        {
            if (Math.Abs(m_player.CurrentTime - m_player.Length) < 0.00001f)
            {
                m_player.CurrentTime = 0;
                return;
            }

            m_player.CurrentTime += Prefs.snapInterval;
        }

        public static void StepBackward()
        {
            if (m_player.CurrentTime == 0)
            {
                m_player.CurrentTime = m_player.Length;
                return;
            }

            m_player.CurrentTime -= Prefs.snapInterval;
        }
        #endregion
    }
}
