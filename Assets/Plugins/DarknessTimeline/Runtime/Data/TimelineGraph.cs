using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

namespace Darkness
{
    [MemoryPackable]
    public partial  class TimelineGraph
    {
        public float length;
        public WarpCategory warpCategory;
        [SerializeReference]
        public List<Group> groups;
    }
}
