using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darkness
{
    public class TimelineGraphPreviewProcessor
    {
        private GameObject m_owner;
        
        private List<IDirectableTimePointer> timePointers;

        /// <summary>
        /// 预览器
        /// </summary>
        private List<IDirectableTimePointer> unsortedStartTimePointers;

        private float playTimeMin;
        private float playTimeMax;
        private float currentTime;
        
        private BlackboardProcessor<string> context;
        
        private Events<string> events;

        public float previousTime { get; private set; }

        private bool preInitialized;

        public TimelineGraphAsset TimelineGraphAsset { get; private set; }
        
        public BlackboardProcessor<string> Context {
            get
            {
                if (context == null)
                {
                    context = new BlackboardProcessor<string>(new Blackboard<string>(), Events);
                }

                return context;
            }
        }
        
        public Events<string> Events {
            get
            {
                if (events == null)
                {
                    events = new Events<string>();
                }

                return events;
            }
        }
        public GameObject Owner
        {
            get => m_owner;
            set => m_owner = value;
        }
        

        /// <summary>
        /// 当前时间
        /// </summary>
        public float CurrentTime
        {
            get => currentTime;
            set => currentTime = Mathf.Clamp(value, 0, Length);
        }

        public bool IsActive { get; set; }

        public bool IsPaused { get; set; }

        public float PlayTimeMin
        {
            get => playTimeMin;
            set => playTimeMin = Mathf.Clamp(value, 0, PlayTimeMax);
        }

        public float PlayTimeMax
        {
            get => playTimeMax;
            set => playTimeMax = Mathf.Clamp(value, PlayTimeMin, Length);
        }

        
        public float Length
        {
            get
            {
                if (TimelineGraphAsset != null)
                {
                    return TimelineGraphAsset.Length;
                }
        
                return 0;
            }
        }

        public TimelineGraphPreviewProcessor(TimelineGraphAsset asset)
        {
            this.TimelineGraphAsset = asset;
        }

        public void Sample()
        {
            Sample(currentTime);
        }

        public void Sample(float time)
        {
            CurrentTime = time;
            // if (currentTime == 0 || Math.Abs(currentTime - Length) < 0.0001f)
            if ((currentTime == 0 || currentTime == Length) && previousTime == currentTime)
            {
                return;
            }
            // Debug.Log($"CurrentTime={CurrentTime}");

            if (!preInitialized && currentTime > 0 && previousTime == 0)
            {
                InitializePreviewPointers();
            }


            if (timePointers != null)
            {
                InternalSamplePointers(currentTime, previousTime);
            }

            previousTime = currentTime;
        }

        void InternalSamplePointers(float currentTime, float previousTime)
        {
            if (!Application.isPlaying || currentTime > previousTime)
            {
                foreach (var t in timePointers)
                {
                    try
                    {
                        t.TriggerForward(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }


            if (!Application.isPlaying || currentTime < previousTime)
            {
                for (var i = timePointers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        timePointers[i].TriggerBackward(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (unsortedStartTimePointers != null)
            {
                foreach (var t in unsortedStartTimePointers)
                {
                    try
                    {
                        t.Update(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化时间指针预览器
        /// </summary>
        public void InitializePreviewPointers()
        {
            timePointers = new List<IDirectableTimePointer>();
            unsortedStartTimePointers = new List<IDirectableTimePointer>();

            Dictionary<Type, Type> typeDic = new Dictionary<Type, Type>();
            var childs = EditorTools.GetTypeMetaDerivedFrom(typeof(PreviewLogic));
            foreach (var t in childs)
            {
                var arrs = t.Type.GetCustomAttributes(typeof(CustomPreviewAttribute), true);
                foreach (var arr in arrs)
                {
                    if (arr is CustomPreviewAttribute c)
                    {
                        var bindT = c.PreviewType;
                        var iT = t.Type;
                        if (!typeDic.ContainsKey(bindT))
                        {
                            if (!iT.IsAbstract) typeDic[bindT] = iT;
                        }
                        else
                        {
                            var old = typeDic[bindT];
                            //如果不是抽象类，且是子类就更新
                            if (!iT.IsAbstract && iT.IsSubclassOf(old))
                            {
                                typeDic[bindT] = iT;
                            }
                        }
                    }
                }
            }

            foreach (var group in TimelineGraphAsset.groups.AsEnumerable().Reverse())
            {
                if (!group.IsActive) continue;
                foreach (var track in group.Tracks.AsEnumerable().Reverse())
                {
                    if (!track.IsActive) continue;
                    var tType = track.GetType();
                    if (typeDic.TryGetValue(tType, out var t1))
                    {
                        if (Activator.CreateInstance(t1) is PreviewLogic preview)
                        {
                            preview.SetTarget(track);
                            var p3 = new StartTimePreviewPointer(preview);
                            timePointers.Add(p3);
                
                            unsortedStartTimePointers.Add(p3);
                            timePointers.Add(new EndTimePreviewPointer(preview));
                        }
                    }
                
                    foreach (var clip in track.Clips)
                    {
                        var cType = clip.GetType();
                        if (typeDic.TryGetValue(cType, out var t))
                        {
                            if (Activator.CreateInstance(t) is PreviewLogic preview)
                            {
                                preview.SetTarget(clip);
                                var p3 = new StartTimePreviewPointer(preview);
                                timePointers.Add(p3);
                
                                unsortedStartTimePointers.Add(p3);
                                timePointers.Add(new EndTimePreviewPointer(preview));
                            }
                        }
                    }
                }
            }

            preInitialized = true;
        }
    }
}