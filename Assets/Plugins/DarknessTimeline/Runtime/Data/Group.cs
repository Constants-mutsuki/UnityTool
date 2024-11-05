using System;
using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [Serializable]
    [MemoryPackUnion(0, typeof(ExampleGroup))]
    public partial class Group
    {
        public bool active;
        public List<Track> tracks;
    }
}
