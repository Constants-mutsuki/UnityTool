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

            TimelineGraphProcessor timelineGraphProcessor =
                new TimelineGraphProcessor(TimelineGraphAsset.GraphModel);

            for (int i =  TimelineGraphAsset.groups.Count-1 ; i>=0 ;i--)
            {
                var groupAsset = TimelineGraphAsset.groups[i];
                var groupProcessor = timelineGraphProcessor.Children.ElementAt(i);
                if (!groupAsset.IsActive) continue;
                for (int j = groupAsset.Tracks.Count-1;  j>=0;  j--)
                {
                    var trackAsset = groupAsset.Tracks[j];
                    var trackProcessor = groupProcessor.Children.ElementAt(j);
                    if (!trackAsset.IsActive) continue;
                    var tType = trackProcessor.GetType();
                    PreviewLogic trackpreview;
                    
                    if (typeDic.TryGetValue(tType, out var t1))
                    {
                        trackpreview = Activator.CreateInstance(t1) as PreviewLogic;
                    }
                    else
                    {
                        trackpreview = new PreviewLogic();
                    }
                    
                    trackpreview.SetTarget(trackProcessor,trackAsset);
                    var trackp3 = new StartTimePreviewPointer(trackpreview);
                    timePointers.Add(trackp3);
                
                    unsortedStartTimePointers.Add(trackp3);
                    timePointers.Add(new EndTimePreviewPointer(trackpreview));
                
                    for (int k = 0; k < trackAsset.Clips.Count;i++)
                    {
                        var clipAsset = trackAsset.Clips[k];
                        var clipProcessor = trackProcessor.Children.ElementAt(k);
                        var cType = clipProcessor.GetType();
                        PreviewLogic clippreview;
                        if (typeDic.TryGetValue(cType, out var t))
                        {
                            clippreview = Activator.CreateInstance(t) as PreviewLogic;
                        }
                        else
                        {
                            clippreview = new PreviewLogic();
                        }
                        clippreview.SetTarget(clipProcessor,clipAsset);
                        var clipp3 = new StartTimePreviewPointer(clippreview);
                        timePointers.Add(clipp3);
                
                        unsortedStartTimePointers.Add(clipp3);
                        timePointers.Add(new EndTimePreviewPointer(clippreview));
                    }
                }
            }
        }
    }
}