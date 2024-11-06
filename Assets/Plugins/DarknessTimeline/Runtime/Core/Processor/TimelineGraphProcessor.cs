using System;
using System.Collections.Generic;
using UnityEngine;
#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;

#else
using CMath = System.Math;
using CFloat = System.Single;
#endif

namespace Darkness
{
    /// <summary>
    /// 时间轴运行时逻辑
    /// </summary>
    [ViewModel(typeof(TimelineGraph))]
    public class TimelineGraphProcessor : ITimelineGraph
    {
        private TimelineGraph data;

        private CFloat startTime;
        private CFloat endTime;
        private CFloat currentTime;
        private Action onStop;
        
        private List<GroupProcessor> groups;
        private BlackboardProcessor<string> context;
        private Events<string> events;
        private GameObject m_owner;
        public GameObject Owner
        {
            get => m_owner;
            set => m_owner = value;
        }
        
        
        public WarpCategory PlayingWarpMode => data.warpCategory;
        
        
        public CFloat StartTime
        {
            get => startTime;
            private set => SetStartTime(value);
        }
        
        public CFloat EndTime
        {
            get => endTime;
            private set => SetEndTime(value);
        }
        
        public CFloat Length => data.length;

        public IEnumerable<IDirectable> Children => groups != null ? groups : Array.Empty<IDirectable>();
        
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
        public bool Active { get; private set; }
        
        public CFloat PreviousTime { get; private set; }
        
        public CFloat CurrentTime 
        {
            get => currentTime;
            set => SetCurrentTime(value);
        }
        public TimelineGraphProcessor(TimelineGraph data)
        {
            this.data = data;
            if (data.groups != null)
            {
                groups = new List<GroupProcessor>(data.groups.Count);
                foreach (var group in data.groups)
                {
                    var groupProcessorType = ViewModelFactory.GetViewModelType(group.GetType());
                    if (ObjectPools.Instance.Spawn(groupProcessorType) is GroupProcessor groupProcessor)
                    {
                        groupProcessor.SetUp(group, this);
                        groups.Add(groupProcessor);
                    }
                }
            }
        }
        private void SetCurrentTime(CFloat value)
        {
            currentTime = CMath.Clamp(value, 0, Length);
        }

        private void SetStartTime(CFloat value)
        {
            startTime = CMath.Clamp(value, 0, Length);
        }

        private void SetEndTime(CFloat value)
        {
            endTime = CMath.Clamp(value, 0, Length);
        }
        public void Play()
        {
            Play(0, data.length, null);
        }

        public void Play(CFloat start)
        {
            Play(start, data.length, null);
        }

        public void Play(CFloat start, Action stopCallback)
        {
            Play(start, data.length, stopCallback);
        }
        
        public void Play(CFloat start, CFloat end, Action stopCallback)
        {
            if (Active)
            {
                return;
            }

            if (start > end)
            {
                return;
            }

            this.StartTime = start;
            this.EndTime = end;

            this.Active = true;
            this.StartTime = start;
            this.EndTime = end;
            this.PreviousTime = CMath.Clamp(start, start, end);
            this.CurrentTime = CMath.Clamp(start, start, end);
            this.onStop = stopCallback;

            Sample(currentTime);
        }
        
         public void Sample(CFloat time)
        {
            this.CurrentTime = time;

            if ((CurrentTime == 0 || Mathf.Approximately(CurrentTime, Length)) && Mathf.Approximately(PreviousTime ,CurrentTime))
            {
                return;
            }

            var frameData = new FrameData()
            {
                currentTime = this.CurrentTime,
                previousTime = this.PreviousTime,
                deltaTime = this.CurrentTime - this.PreviousTime,
            };

            if (groups != null)
            {
                var startIndex = time >= frameData.previousTime ? 0 : groups.Count - 1;
                var direction = time >= frameData.previousTime ? 1 : -1;
                if (direction == -1)
                {
                    foreach (var group in groups)
                    {
                        if (!group.IsTriggered)
                            continue;

                        group.Exit(frameData);
                    }
                }

                for (int i = 0; i < groups.Count; i++)
                {
                    var index = startIndex + i * direction;
                    var group = groups[index];

                    if (!group.Active)
                    {
                        continue;
                    }

                    if (!group.IsTriggered && frameData.currentTime >= group.StartTime && frameData.currentTime <= group.EndTime)
                    {
                        group.Enter(frameData);
                    }

                    if (group.IsTriggered)
                    {
                        group.Update(frameData);
                    }

                    if (group.IsTriggered && (frameData.currentTime <= group.StartTime || frameData.currentTime >= group.EndTime))
                    {
                        group.Exit(frameData);
                    }
                }
            }

            switch (PlayingWarpMode)
            {
                case WarpCategory.Once:

                    if (CurrentTime < EndTime)
                    {
                        this.PreviousTime = CurrentTime;
                    }
                    else
                    {
                        Stop();
                    }

                    break;
                case WarpCategory.Loop:
                    if (CurrentTime < EndTime)
                    {
                        this.PreviousTime = CurrentTime;
                    }
                    else
                    {
                        PreviousTime = StartTime;
                        CurrentTime = StartTime;
                    }
                    break;
            }
        }
         
        public void Stop(StopMode stopMode = StopMode.Exit)
        {
            if (!Active)
                return;

            Active = false;

            switch (stopMode)
            {
                case StopMode.Exit:
                {
                    foreach (var group in groups)
                    {
                        if (!group.IsTriggered)
                            continue;

                        var frameData = new FrameData()
                        {
                            currentTime = CurrentTime,
                            previousTime = PreviousTime,
                            deltaTime = CurrentTime - PreviousTime,
                        };

                        group.Exit(frameData);
                    }

                    break;
                }
                case StopMode.Skip:
                {
                    Sample(endTime);
                    break;
                }
            }

            if (onStop != null)
            {
                onStop.Invoke();
                onStop = null;
            }

            CurrentTime = 0;
            PreviousTime = 0;
        }
        public void Reset()
        {
            if (!Active)
            {
                return;
            }

            foreach (var child in Children)
            {
                child.Reset();
            }

            context?.Clear();
            events?.Clear();
        }
        public void Dispose()
        {
            Stop();
            foreach (var child in Children)
            {
                child.Dispose();
            }

            data = null;
            groups.Clear();
            context?.Clear();
            events?.Clear();
        }
    }
}
