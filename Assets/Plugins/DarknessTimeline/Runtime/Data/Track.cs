using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darkness
{
    [Serializable]
    public class Track
    {
        public bool active;
        [SerializeReference]
        public List<Clip> clips;
    }
}
