using System;
using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [Serializable]
    public class TimelineGraph
    {
        public float length;
        public WarpCategory warpCategory;
        public List<Group> groups;
    }
}
