using System;
using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [Serializable]
    [MemoryPackable]
    public partial class TimelineGraph
    {
        public float length;
        public WarpCategory warpCategory;
        public List<Group> groups;
    }
}
