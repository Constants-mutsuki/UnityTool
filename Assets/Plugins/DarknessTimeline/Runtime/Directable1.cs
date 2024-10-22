using System;
using System.Collections.Generic;
using CZToolKit.Blackboard;
using UnityEngine;

#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;

#else
using CMath = System.Math;
using CFloat = System.Single;
#endif

namespace CZToolKit.Directable
{
    public enum WrapMode
    {
        Once,
        Loop,
    }

    public enum StopMode
    {
        Exit,
        Skip,
    }

    [Serializable]
    public class DirectableGraph
    {
        public CFloat length;
        public WrapMode warpMode;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeReference]
#endif
        public List<Group> groups;
    }

    [Serializable]
    public class Group
    {
        public bool active;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeReference]
#endif
        public List<Track> tracks;
    }

    [Serializable]
    public class Track
    {
        public bool active;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeReference]
#endif
        public List<Clip> clips;
    }

    [Serializable]
    public class Clip
    {
        public CFloat startTime;
        public CFloat length;
    }

    public partial interface IDirector : IDisposable
    {
        IEnumerable<IDirectable> Children { get; }
        BlackboardProcessor<string> Context { get; }
        Events<string> Events { get; }

        bool Active { get; }

        CFloat Length { get; }
        CFloat CurrentTime { get; set; }
        CFloat PreviousTime { get; }

        void Reset();
    }

    public interface IDirectable : IDisposable
    {
        bool Active { get; }

        IDirector Root { get; }

        IDirectable Parent { get; }

        IEnumerable<IDirectable> Children { get; }

        public CFloat Length { get; }
        public CFloat StartTime { get; }
        public CFloat EndTime { get; }

        void Enter(FrameData frameData);

        void Update(FrameData frameData);

        void Exit(FrameData frameData);

        void Reset();
    }

    public struct FrameData
    {
        public CFloat previousTime;
        public CFloat currentTime;
        public CFloat deltaTime;
    }

    [ViewModel(typeof(DirectableGraph))]
    public class DirectableGraphBehavior : IDirector
    {
        private DirectableGraph data;

        private CFloat startTime;
        private CFloat endTime;
        private CFloat currentTime;
        private Action onStop;

        private List<GroupBehavior> groups;
        private BlackboardProcessor<string> context;
        private Events<string> events;

        public WrapMode PlayingWarpMode => data.warpMode;

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

        public BlackboardProcessor<string> Context
        {
            get
            {
                if (context == null)
                {
                    context = new BlackboardProcessor<string>(new Blackboard<string>(), Events);
                }

                return context;
            }
        }

        public Events<string> Events
        {
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

        public DirectableGraphBehavior(DirectableGraph data)
        {
            this.data = data;
            if (data.groups != null)
            {
                groups = new List<GroupBehavior>(data.groups.Count);
                foreach (var group in data.groups)
                {
                    var groupBehaviorType = ViewModelFactory.GetViewModelType(group.GetType());
                    if (ObjectPools.Instance.Spawn(groupBehaviorType) is GroupBehavior groupBehavior)
                    {
                        groupBehavior.SetUp(group, this);
                        groups.Add(groupBehavior);
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

            if ((CurrentTime == 0 || Mathf.Approximately(CurrentTime, Length)) && PreviousTime == CurrentTime)
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
                case WrapMode.Once:

                    if (CurrentTime < EndTime)
                    {
                        this.PreviousTime = CurrentTime;
                    }
                    else
                    {
                        Stop();
                    }

                    break;
                case WrapMode.Loop:
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

    [ViewModel(typeof(Group))]
    public class GroupBehavior : IDirectable
    {
        private Group data;
        private List<TrackBehavior> tracks;
        private CFloat startTime;
        private CFloat endTime;

        public bool Active => data.active;

        public CFloat StartTime => startTime;

        public CFloat EndTime => endTime;

        public CFloat Length => endTime - startTime;

        public IDirector Root { get; private set; }

        public IDirectable Parent => null;

        public IEnumerable<IDirectable> Children => tracks != null ? tracks : Array.Empty<IDirectable>();

        public bool IsTriggered { get; private set; }

        public void SetUp(Group group, IDirector graph)
        {
            this.data = group;
            this.Root = graph;
            this.startTime = 0;
            this.endTime = Root.Length;
            if (group.tracks != null)
            {
                this.tracks = new List<TrackBehavior>(group.tracks.Count);
                foreach (var track in group.tracks)
                {
                    var trackBehaviorType = ViewModelFactory.GetViewModelType(track.GetType());
                    if (ObjectPools.Instance.Spawn(trackBehaviorType) is TrackBehavior trackBehavior)
                    {
                        trackBehavior.SetUp(track, this);
                        tracks.Add(trackBehavior);
                    }
                }
            }
        }

        public void Enter(FrameData frameData)
        {
            IsTriggered = true;
            OnEnter(frameData, frameData);
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

    [ViewModel(typeof(Track))]
    public class TrackBehavior : IDirectable
    {
        private Track data;
        private List<ClipBehavior> clips;
        private CFloat startTime;
        private CFloat endTime;

        protected Track Data => data;

        public bool Active => data.active;

        public CFloat StartTime => startTime;

        public CFloat EndTime => endTime;

        public CFloat Length => endTime - startTime;

        public IDirector Root { get; private set; }

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
                this.clips = new List<ClipBehavior>(track.clips.Count);
                foreach (var clip in track.clips)
                {
                    var clipBehaviorType = ViewModelFactory.GetViewModelType(clip.GetType());
                    if (ObjectPools.Instance.Spawn(clipBehaviorType) is ClipBehavior clipBehavior)
                    {
                        clipBehavior.SetUp(clip, this);
                        clips.Add(clipBehavior);
                    }
                }
            }
        }

        public void Enter(FrameData frameData)
        {
            IsTriggered = true;
            OnEnter(frameData, frameData);
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

    [ViewModel(typeof(Clip))]
    public class ClipBehavior : IDirectable
    {
        private Clip data;

        public bool Active => true;

        public CFloat StartTime => data.startTime;

        public CFloat EndTime => data.startTime + data.length;

        public CFloat Length => data.length;

        protected Clip Data => data;

        public IDirector Root { get; private set; }

        public IDirectable Parent { get; private set; }

        public IEnumerable<IDirectable> Children => Array.Empty<IDirectable>();

        public bool IsTriggered { get; private set; }

        public void SetUp(Clip clip, IDirectable track)
        {
            this.data = clip;
            this.Root = track.Root;
            this.Parent = track;
        }

        public void Enter(FrameData frameData)
        {
            var innerCurrentTime = CMath.Clamp(frameData.currentTime - StartTime, 0, Length);
            var innerPreviousTime = CMath.Clamp(frameData.previousTime - StartTime, 0, Length);
            var innerFrameData = new FrameData()
            {
                currentTime = innerCurrentTime,
                previousTime = innerPreviousTime,
                deltaTime = innerCurrentTime - innerPreviousTime,
            };
            IsTriggered = true;
            OnEnter(frameData, innerFrameData);
        }

        public void Exit(FrameData frameData)
        {
            var innerCurrentTime = CMath.Clamp(frameData.currentTime - StartTime, 0, Length);
            var innerPreviousTime = CMath.Clamp(frameData.previousTime - StartTime, 0, Length);
            var innerFrameData = new FrameData()
            {
                currentTime = innerCurrentTime,
                previousTime = innerPreviousTime,
                deltaTime = innerCurrentTime - innerPreviousTime,
            };
            IsTriggered = false;
            OnExit(frameData, innerFrameData);
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
            OnUpdate(frameData, innerFrameData);
        }

        public void Reset()
        {
            OnReset();
        }

        public void Dispose()
        {
            OnDispose();

            data = null;
            Root = null;
            Parent = null;
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

    public class TrackBehavior<T> : TrackBehavior where T : Track
    {
        protected T TData
        {
            get { return Data as T; }
        }
    }

    public class ClipBehavior<T> : ClipBehavior where T : Clip
    {
        protected T TData
        {
            get { return Data as T; }
        }
    }
}