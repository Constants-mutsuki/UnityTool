using System;
using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [Serializable]
    [MemoryPackable]
    public partial  class Track
    {
        public bool active;
        public List<Clip> clips;
    }
}
