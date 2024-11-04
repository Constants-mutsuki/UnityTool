using System;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    [Attachable(typeof(TrackAsset))]
    public abstract class ClipAsset : DirectableAsset
    {

        private Clip m_clip;
        
        
        [SerializeField]
        private float startTime;

        public override TimelineGraphAsset Root
        {
            get => Parent?.Root;
            set { }
        }

        public override DirectableAsset Parent
        {
            get => m_parent;
            set => m_parent = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public override float StartTime
        {
            get => startTime;
            set
            {
                if (Math.Abs(startTime - value) > 0.0001f)
                {
                    startTime = Mathf.Max(value, 0);
                    BlendIn = Mathf.Clamp(BlendIn, 0, Length - BlendOut);
                    BlendOut = Mathf.Clamp(BlendOut, 0, Length - BlendIn);
                }
            }
        }


        public override float EndTime
        {
            get => StartTime + Length;
            set
            {
                if (Math.Abs(StartTime + Length - value) > 0.0001f) //if (StartTime + length != value)
                {
                    Length = Mathf.Max(value - StartTime, 0);
                    BlendOut = Mathf.Clamp(BlendOut, 0, Length - BlendIn);
                    BlendIn = Mathf.Clamp(BlendIn, 0, Length - BlendOut);
                }
            }
        }

        public override bool IsActive => Parent && Parent.IsActive;
        public override bool IsCollapsed => Parent != null && Parent.IsCollapsed;
        public override bool IsLocked => Parent != null && Parent.IsLocked;

        public virtual float Length
        {
            get => 0;
            set { }
        }


        public virtual string info
        {
            get
            {
                var nameAtt = GetType().RTGetAttribute<NameAttribute>(true);
                if (nameAtt != null)
                {
                    return nameAtt.name;
                }

                return GetType().Name.SplitCamelCase();
            }
        }

        public virtual bool isValid => true;


        public ClipAsset GetNextClip()
        {
            return this.GetNextSibling<ClipAsset>();
        }

        public float GetClipWeight(float time)
        {
            return GetClipWeight(time, this.BlendIn, this.BlendOut);
        }

        public float GetClipWeight(float time, float blendInOut)
        {
            return GetClipWeight(time, blendInOut, blendInOut);
        }

        public float GetClipWeight(float time, float blendIn, float blendOut)
        {
            return this.GetWeight(time, blendIn, blendOut);
        }

        public void TryMatchSubClipLength()
        {
            if (this is ISubClipContainable subClipContainable)
            {
                Length = subClipContainable.SubClipLength / subClipContainable.SubClipSpeed;
            }
        }

        public void TryMatchPreviousSubClipLoop()
        {
            if (this is ISubClipContainable)
            {
                Length = (this as ISubClipContainable).GetPreviousLoopLocalTime();
            }
        }

        public void TryMatchNexSubClipLoop()
        {
            if (this is ISubClipContainable)
            {
                var targetLength = (this as ISubClipContainable).GetNextLoopLocalTime();
                var nextClip = GetNextClip();
                if (nextClip == null || StartTime + targetLength <= nextClip.StartTime)
                {
                    Length = targetLength;
                }
            }
        }

        public void SetUp(Clip clip)
        {
            m_clip = clip;
        }

        #region Unity Editor

#if UNITY_EDITOR

        public void ShowClipGUI(Rect rect)
        {
            OnClipGUI(rect);
        }

        public void ShowClipGUIExternal(Rect left, Rect right)
        {
            OnClipGUIExternal(left, right);
        }

        protected virtual void OnClipGUI(Rect rect)
        {
        }

        protected virtual void OnClipGUIExternal(Rect left, Rect right)
        {
        }

#endif

        #endregion
    }
}