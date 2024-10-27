using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [MemoryPackable]
    [MemoryPackUnion(0,typeof(ExampleTrack))]
    public abstract partial  class Track
    {
        public bool active;
        public List<Clip> clips;
    }
}
