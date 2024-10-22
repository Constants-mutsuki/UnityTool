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
        private static TextAsset _textAsset;

        public static CallbackFunction OnInitialize;
        public static CallbackFunction OnDisable;
        public static OpenAssetFunction OnOpenAsset;
        public static CallbackFunction OnPlay;
        public static CallbackFunction OnStop;

        public static TimelineGraphAsset TimelineGraphAssetData { get; set; } = null;


        public static void OnObjectPickerConfig(Object obj)
        {
            if (obj is TimelineGraphAsset a)
            {
                TimelineGraphAssetData = a;
            }
        }

        public static void SaveAsset()
        {
            if (TimelineGraphAssetData != null)
                EditorUtility.SetDirty(TimelineGraphAssetData);
        }


        #region AutoSave
        public static DateTime LastSaveTime => _lastSaveTime;

        private static DateTime _lastSaveTime = DateTime.Now;

        /// <summary>
        /// 尝试自动保存
        /// </summary>
        public static void TryAutoSave()
        {
            var timespan = DateTime.Now - _lastSaveTime;
            if (timespan.Seconds > Prefs.autoSaveSeconds)
            {
                AutoSave();
            }
        }

        public static void AutoSave()
        {
            _lastSaveTime = DateTime.Now;
            SaveAsset();
        }
        #endregion

        #region 播放相关
        private static AssetPlayer _player => AssetPlayer.Inst;

        public static bool IsStop => Application.isPlaying ? _player.IsPaused || !_player.IsActive : EditorPlaybackState == EditorPlaybackState.Stopped;

        internal static EditorPlaybackState EditorPlaybackState = EditorPlaybackState.Stopped;

        public static WrapMode EditorPlaybackWrapMode = WrapMode.Loop;

        public static bool IsPlay => _player.CurrentTime > 0;

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
            if (TimelineGraphAssetData != null)
                _player.CurrentTime = 0;
            EditorPlaybackState = EditorPlaybackState.Stopped;
            // WillRepaint = true;

            OnStop?.Invoke();
        }

        public static void StepForward()
        {
            if (Math.Abs(_player.CurrentTime - _player.Length) < 0.00001f)
            {
                _player.CurrentTime = 0;
                return;
            }

            _player.CurrentTime += Prefs.snapInterval;
        }

        public static void StepBackward()
        {
            if (_player.CurrentTime == 0)
            {
                _player.CurrentTime = _player.Length;
                return;
            }

            _player.CurrentTime -= Prefs.snapInterval;
        }
        #endregion
    }
}
