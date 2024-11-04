
namespace Darkness
{
    public interface IDirectableTimePointer
    {
        PreviewLogic target { get; }
        float time { get; }
        void TriggerForward(float currentTime, float previousTime);
        void TriggerBackward(float currentTime, float previousTime);
        void Update(float currentTime, float previousTime);
    }

    public struct StartTimePointer : IDirectableTimePointer
    {
        private bool triggered;
        private float lastTargetStartTime;
        public PreviewLogic target { get; private set; }
        float IDirectableTimePointer.time => target.Directable.StartTime;

        public StartTimePointer(PreviewLogic target)
        {
            this.target = target;
            triggered = false;
            lastTargetStartTime = target.Directable.StartTime;
        }

        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime >= target.Directable.StartTime)
            {
                if (!triggered)
                {
                    triggered = true;
                    target.Enter();
                    target.Update(target.Directable.ToLocalTime(currentTime), 0);
                }
            }
        }

        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime >= target.Directable.StartTime && currentTime < target.Directable.EndTime &&
                currentTime > 0)
            {
                var deltaMoveClip = target.Directable.StartTime - lastTargetStartTime;
                var localCurrentTime = target.Directable.ToLocalTime(currentTime);
                var localPreviousTime = target.Directable.ToLocalTime(previousTime + deltaMoveClip);

                target.Update(localCurrentTime, localPreviousTime);
                lastTargetStartTime = target.Directable.StartTime;
            }
        }

        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime < target.Directable.StartTime || currentTime <= 0)
            {
                if (triggered)
                {
                    triggered = false;
                    target.Update(0, target.Directable.ToLocalTime(previousTime));
                    target.Reverse();
                }
            }
        }
    }

    public struct EndTimePointer : IDirectableTimePointer
    {
        private bool triggered;
        public PreviewLogic target { get; private set; }
        float IDirectableTimePointer.time => target.Directable.EndTime;

        public EndTimePointer(PreviewLogic target)
        {
            this.target = target;
            triggered = false;
        }

        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime >= target.Directable.EndTime)
            {
                if (!triggered)
                {
                    triggered = true;
                    target.Update(target.Directable.GetLength(), target.Directable.ToLocalTime(previousTime));
                    target.Exit();
                }
            }
        }


        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
            
        }


        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime < target.Directable.EndTime || currentTime <= 0)
            {
                if (triggered)
                {
                    triggered = false;
                    target.ReverseEnter();
                    target.Update(target.Directable.ToLocalTime(currentTime), target.Directable.GetLength());
                }
            }
        }
    }
}