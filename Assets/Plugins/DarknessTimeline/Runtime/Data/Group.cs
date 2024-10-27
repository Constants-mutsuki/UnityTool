using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [MemoryPackable]
    [MemoryPackUnion(0,typeof(ExampleGroup))]
    public abstract partial class Group
    {
        public bool active;
        public List<Track> tracks;
    }
}
