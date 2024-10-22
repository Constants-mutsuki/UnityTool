using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

namespace Darkness
{
    [MemoryPackable]
    public partial class Group
    {
        public bool active;
        [SerializeReference]
        public List<Track> tracks;
    }
}
