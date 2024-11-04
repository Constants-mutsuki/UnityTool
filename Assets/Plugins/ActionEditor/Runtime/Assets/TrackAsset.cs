﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    [Attachable(typeof(GroupAsset))]
    public abstract class TrackAsset : DirectableAsset
    {
        [SerializeField]
        private List<ClipAsset> actionClips = new List<ClipAsset>();

        [SerializeField]
        [HideInInspector]
        private bool active = true;

        [SerializeField]
        [HideInInspector]
        private bool isLocked = false;

        [SerializeField]
        private Color color = Color.white;


        public Color Color => color.a > 0.1f ? color : Color.white;

        public string Name
        {
            get => name;
            set => name = value;
        }


        public virtual string info => string.Empty;

        public override TimelineGraphAsset Root
        {
            get => Parent?.Root;
            set { }
        }

        public override DirectableAsset Parent
        {
            get => (GroupAsset)m_parent;
            set => m_parent = value;
        }

        public override bool IsCollapsed => Parent && Parent.IsCollapsed;

        public override bool IsActive
        {
            get => Parent && Parent.IsActive && active;
            set => active = value;
        }

        public override bool IsLocked
        {
            get => Parent && (Parent.IsLocked || isLocked);
            set => isLocked = value;
        }


        public List<ClipAsset> Clips
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
        public T AddClip<T>(float time) where T : ClipAsset
        {
            return (T)AddClip(typeof(T), time);
        }

        public ClipAsset AddClip(Type type, float time)
        {
            if (type.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() is CategoryAttribute catAtt && Clips.Count == 0)
            {
                Name = catAtt.category + " Track";
            }

            var newAction = CreateInstance(type) as ClipAsset;

            CreateUtilities.SaveAssetIntoObject(newAction, this);
            DirectorUtility.SelectedObject = newAction;

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

        public void DeleteClip(ClipAsset action)
        {
            Clips.Remove(action);
            if (ReferenceEquals(DirectorUtility.SelectedObject, action))
            {
                DirectorUtility.SelectedObject = null;
            }
        }

        public ClipAsset PasteClip(ClipAsset clipAsset, float time = 0)
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