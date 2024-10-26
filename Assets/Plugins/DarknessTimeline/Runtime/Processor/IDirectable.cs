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
}