using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    public class Group
    {
        public bool active;
        [SerializeReference]
        public List<Track> tracks;
    }
}
