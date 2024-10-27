using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [MemoryPackable]
    public partial  class TimelineGraph
    {
        public float length;
        public WarpCategory warpCategory;
        public List<Group> groups;
    }
}
