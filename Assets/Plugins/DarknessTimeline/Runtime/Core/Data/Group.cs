﻿using System;
using System.Collections.Generic;
using MemoryPack;


namespace Darkness
{
    [Serializable]
    [MemoryPackable]
    public partial class Group
    {
        public List<Track> tracks;
    }
}
