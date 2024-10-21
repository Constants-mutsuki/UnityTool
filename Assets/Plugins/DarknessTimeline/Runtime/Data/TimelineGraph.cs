using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    public class TimelineGraph
    {
        public float length;
        public WarpCategory warpCategory;
        [SerializeReference]
        public List<Group> groups;
    }
}
