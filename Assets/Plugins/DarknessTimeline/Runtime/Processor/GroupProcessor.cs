using System;
using System.Collections.Generic;
#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;

#else
using CMath = System.Math;
using CFloat = System.Single;
#endif

namespace Darkness
{

    public abstract class GroupProcessor<T> : GroupProcessor where T : Group
    {
        protected T TData => Data as T;
    }
    
    [ViewModel(typeof(Group))]
    public abstract class GroupProcessor : IDirectable
    {
        private Group data;
        private List<TrackProcessor> tracks;
        private CFloat startTime;
        private CFloat endTime;
        public bool Active => data.active;
        
        public CFloat StartTime => startTime;
        
        public CFloat EndTime => endTime;
        
        public CFloat Length => endTime - startTime;
        public ITimelineGraph Root { get; private set; }
        public IDirectable Parent => null;

        protected Group Data => data;
        
        public IEnumerable<IDirectable> Children => tracks != null ? tracks : Array.Empty<IDirectable>();
        
        public bool IsTriggered { get; private set; }
        
        public void SetUp(Group group, ITimelineGraph graph)
        {
            this.data = group;
            this.Root = graph;
            this.startTime = 0;
            this.endTime = Root.Length;
            if (group.tracks != null)
            {
                this.tracks = new List<TrackProcessor>(group.tracks.Count);
                foreach (var track in group.tracks)
                {
                    var trackProcessorType = ViewModelFactory.GetViewModelType(track.GetType());
                    if (ObjectPools.Instance.Spawn(trackProcessorType) is TrackProcessor trackProcessor)
                    {
                        trackProcessor.SetUp(track, this);
                        tracks.Add(trackProcessor);
                    }
                }
            }
        }
        public void Enter(FrameData frameData)
        {
            IsTriggered = true;
            OnEnter(frameData, frameData);
        }

        public void Update(FrameData frameData)
        {
            var innerCurrentTime = CMath.Clamp(frameData.currentTime - StartTime, 0, Length);
            var innerPreviousTime = CMath.Clamp(frameData.previousTime - StartTime, 0, Length);
            var innerFrameData = new FrameData()
            {
                currentTime = innerCurrentTime,
                previousTime = innerPreviousTime,
                deltaTime = innerCurrentTime - innerPreviousTime,
            };

            if (tracks != null)
            {
                var startIndex = frameData.currentTime >= frameData.previousTime ? 0 : tracks.Count - 1;
                var direction = frameData.currentTime >= frameData.previousTime ? 1 : -1;
                for (int i = 0; i < tracks.Count; i++)
                {
                    var index = startIndex + i * direction;
                    var track = tracks[index];

                    if (!track.Active)
                    {
                        continue;
                    }

                    if (!track.IsTriggered)
                    {
                        if (frameData.previousTime <= track.StartTime && frameData.currentTime >= track.StartTime)
                        {
                            track.Enter(frameData);
                        }
                        else if (frameData.previousTime >= track.EndTime && frameData.currentTime <= track.EndTime)
                        {
                            track.Enter(frameData);
                        }
                    }

                    if (track.IsTriggered)
                    {
                        track.Update(frameData);
                    }

                    if (track.IsTriggered && (frameData.currentTime <= track.StartTime || frameData.currentTime >= track.EndTime))
                    {
                        track.Exit(frameData);
                    }
                }
            }

            OnUpdate(frameData, innerFrameData);
        }

        public void Exit(FrameData frameData)
        {
            if (tracks != null)
            {
                foreach (var track in tracks)
                {
                    if (!track.IsTriggered)
                        continue;

                    track.Exit(frameData);
                }
            }

            IsTriggered = false;
            OnExit(frameData, frameData);
        }

        public void Reset()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }

            OnReset();
        }
        public void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }

            tracks.Clear();

            OnDispose();

            data = null;
            startTime = 0;
            endTime = 0;

            ObjectPools.Instance.Recycle(this);
        }
        protected virtual void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnExit(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnUpdate(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}
