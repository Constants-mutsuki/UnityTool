using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    [Attachable(typeof(GroupAsset))]
    public abstract class TrackAsset : DirectableAsset
    {
        [SerializeField] private List<ActionClipAsset> actionClips = new List<ActionClipAsset>();

        [SerializeField] [HideInInspector] private bool active = true;
        [SerializeField] [HideInInspector] private bool isLocked = false;

        [SerializeField] private Color color = Color.white;


        public Color Color => color.a > 0.1f ? color : Color.white;

        public string Name
        {
            get => name;
            set => name = value;
        }


        public virtual string info => string.Empty;

        public virtual GroupAsset Parent
        {
            get => (GroupAsset)m_parent;
            set => m_parent = value;
        }

        public override bool IsCollapsed => Parent != null && Parent.IsCollapsed;

        public override bool IsActive
        {
            get => Parent != null && (Parent.IsActive && active);
            set
            {
                if (active != value)
                {
                    active = value;
                }
            }
        }

        public override bool IsLocked
        {
            get => Parent != null && (Parent.IsLocked || isLocked);
            set => isLocked = value;
        }


        public List<ActionClipAsset> Clips
        {
            get => actionClips;
            set => actionClips = value;
        }

        public override float StartTime => 0;


        public override float EndTime => Parent != null ? Parent.EndTime : 0;

        public override bool CanCrossBlend => false;


        public virtual float ShowHeight => 30f;


        #region 增删

#if UNITY_EDITOR
        public T AddAction<T>(float time) where T : ActionClipAsset
        {
            return (T)AddAction(typeof(T), time);
        }

        public ActionClipAsset AddAction(Type type, float time)
        {
            var catAtt =
                type.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
            if (catAtt != null && Clips.Count == 0)
            {
                Name = catAtt.category + " Track";
            }

            var newAction = CreateInstance(type) as ActionClipAsset;

            CreateUtilities.SaveAssetIntoObject(newAction, this);
            DirectorUtility.selectedObject = newAction;

            if (newAction != null)
            {
                newAction.Parent = this;
                newAction.StartTime = time;
                Clips.Add(newAction);

                var nextAction = Clips.FirstOrDefault(a => a.StartTime > newAction.StartTime);
                if (nextAction != null)
                {
                    newAction.EndTime = Mathf.Min(newAction.EndTime, nextAction.StartTime);
                }
            }

            return newAction;
        }

        public void DeleteAction(ActionClipAsset action)
        {
            Clips.Remove(action);
            if (ReferenceEquals(DirectorUtility.selectedObject, action))
            {
                DirectorUtility.selectedObject = null;
            }
        }

        public ActionClipAsset PasteClip(ActionClipAsset clipAsset, float time = 0)
        {
            var newClip = Instantiate(clipAsset);
            if (newClip != null)
            {
                if (time > 0)
                {
                    newClip.StartTime = time;
                    var nextClip = Clips.FirstOrDefault(a => a.StartTime > newClip.StartTime);
                    if (nextClip != null && newClip.EndTime > nextClip.StartTime)
                    {
                        newClip.EndTime = nextClip.StartTime;
                    }
                }

                newClip.Parent = this;
                Clips.Add(newClip);
                CreateUtilities.SaveAssetIntoObject(newClip, this);
            }

            return newClip;
        }
#endif

        #endregion
        

        internal bool IsCompilable()
        {
            return true;
        }


        bool m_CacheSorted;

        public void SortClips()
        {
            if (!m_CacheSorted)
            {
                Clips.Sort((clip1, clip2) => clip1.StartTime.CompareTo(clip2.StartTime));
                m_CacheSorted = true;
            }
        }
    }
}