using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

namespace Darkness
{
    [MemoryPackable]
    public partial  class Track
    {
        public bool active;
        [SerializeReference]
        public List<Clip> clips;
    }
}
