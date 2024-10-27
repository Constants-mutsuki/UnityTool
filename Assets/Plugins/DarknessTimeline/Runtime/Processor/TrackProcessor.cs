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

    public abstract class TrackProcessor<T> : TrackProcessor where T : Track
    {
        protected T TData => Data as T;
    }
    
    [ViewModel(typeof(Track))]
    public abstract class TrackProcessor : IDirectable
    {
        private Track data;
        private List<ClipProcessor> clips;
        private CFloat startTime;
        private CFloat endTime;

        protected Track Data => data;
        public bool Active => data.active;
        
        public CFloat StartTime => startTime;
        
        public CFloat EndTime => endTime;
        
        public CFloat Length => endTime - startTime;
        public ITimelineGraph Root { get; private set; }
        
        public IDirectable Parent { get; private set; }
        
        public IEnumerable<IDirectable> Children => clips != null ? clips : Array.Empty<IDirectable>();
        
        public bool IsTriggered { get; private set; }
        
        public void SetUp(Track track, IDirectable group)
        {
            this.data = track;
            this.Parent = group;
            this.Root = group.Root;
            this.startTime = 0;
            this.endTime = Root.Length;
            if (track.clips != null)
            {
                this.clips = new List<ClipProcessor>(track.clips.Count);
                foreach (var clip in track.clips)
                {
                    var clipProcessorType = ViewModelFactory.GetViewModelType(clip.GetType());
                    if (ObjectPools.Instance.Spawn(clipProcessorType) is ClipProcessor clipProcessor)
                    {
                        clipProcessor.SetUp(clip, this);
                        clips.Add(clipProcessor);
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

            if (clips != null)
            {
                var startIndex = frameData.currentTime >= frameData.previousTime ? 0 : clips.Count - 1;
                var direction = frameData.currentTime >= frameData.previousTime ? 1 : -1;
                for (int i = 0; i < clips.Count; i++)
                {
                    var index = startIndex + i * direction;
                    var clip = clips[index];
                    if (!clip.Active)
                    {
                        continue;
                    }

                    if (!clip.IsTriggered)
                    {
                        if (frameData.previousTime <= clip.StartTime && frameData.currentTime >= clip.StartTime)
                        {
                            clip.Enter(frameData);
                        }
                        else if (frameData.previousTime >= clip.EndTime && frameData.currentTime <= clip.EndTime)
                        {
                            clip.Enter(frameData);
                        }
                    }

                    if (clip.IsTriggered)
                    {
                        clip.Update(frameData);
                    }

                    if (clip.IsTriggered && (frameData.currentTime <= clip.StartTime || frameData.currentTime >= clip.EndTime))
                    {
                        clip.Exit(frameData);
                    }
                }
            }

            OnUpdate(frameData, innerFrameData);
        }

        public void Exit(FrameData frameData)
        {
            if (clips == null)
                return;

            foreach (var clip in clips)
            {
                if (!clip.IsTriggered)
                    continue;

                clip.Exit(frameData);
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

            clips.Clear();

            OnDispose();

            data = null;
            Root = null;
            Parent = null;
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
